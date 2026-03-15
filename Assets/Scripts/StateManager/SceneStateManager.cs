using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SceneData
{
    public string sceneName;
    public List<ObjectState> objectStates = new List<ObjectState>();
}

[Serializable]
public class ObjectState
{
    public string objectID;
    public bool isActive;
    public Vector3 position;
}

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance { get; private set; }

    private Dictionary<string, SceneData> allScenes = new Dictionary<string, SceneData>();
    private string savePath;

    // ✅ 防止重复保存标识
    private string lastSavedScene = null;

    public bool isSwitchingScene = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "sceneStates.json");

            LoadAllScenes();
            Debug.Log("✅ SceneStateManager 初始化成功，并已设置为常驻对象。");
        }
        else
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("⚠️ 检测到重复 SceneStateManager，已销毁多余实例。");
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // -------------------- 场景卸载：保存状态 --------------------
    private void OnSceneUnloaded(Scene scene)
    {
        if (isSwitchingScene)
        {
            Debug.Log($"⏸ 跳过保存（正在切换场景）：{scene.name}");
            return;
        }

        if (scene == null || string.IsNullOrEmpty(scene.name))
        {
            Debug.LogWarning("⚠️ OnSceneUnloaded 收到空场景名，跳过保存。");
            return;
        }

        // 防止重复保存
        if (scene.name == lastSavedScene)
        {
            Debug.Log($"⚠️ 重复卸载触发被忽略：{scene.name}");
            return;
        }

        lastSavedScene = scene.name;
        SaveScene(scene);
    }


    private void SaveScene(Scene scene)
    {
        if (scene == null || string.IsNullOrEmpty(scene.name))
        {
            Debug.LogError("❌ 保存失败：scene.name 为空！");
            return;
        }

        var objs = FindObjectsOfType<PersistentObject>();
        if (objs.Length == 0)
        {
            Debug.LogWarning($"⚠️ 场景 {scene.name} 中没有可保存对象，跳过保存。");
            return;
        }

        SceneData data = new SceneData { sceneName = scene.name };

        foreach (var obj in objs)
        {
            ObjectState s = new ObjectState
            {
                objectID = obj.objectID,
                isActive = obj.gameObject.activeSelf,
                position = obj.transform.position
            };
            data.objectStates.Add(s);
        }

        allScenes[scene.name] = data;
        SaveAllScenes();
        Debug.Log($"💾 保存场景：{scene.name}（共 {data.objectStates.Count} 个物体）");
    }

    // -------------------- 场景加载：异步恢复 --------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene == null)
        {
            Debug.LogError("❌ OnSceneLoaded 收到空 Scene！");
            return;
        }

        Debug.Log($"[DEBUG] OnSceneLoaded 触发：{scene.name} (mode={mode})");
        StartCoroutine(WaitUntilSceneReady(scene));
    }

    private IEnumerator WaitUntilSceneReady(Scene scene)
    {
        // 防御性：如果场景名为空，先取当前激活场景
        if (string.IsNullOrEmpty(scene.name))
        {
            scene = SceneManager.GetActiveScene();
            Debug.LogWarning($"⚠️ 传入空场景引用，自动改为当前场景：{scene.name}");
        }

        Debug.Log($"[DEBUG] 等待场景 {scene.name} 完全加载...");

        float timeout = 3f;
        Scene realScene = SceneManager.GetSceneByName(scene.name);

        // 等待场景真正 isLoaded = true
        while (!realScene.isLoaded && timeout > 0f)
        {
            yield return null;
            realScene = SceneManager.GetSceneByName(scene.name);
            timeout -= Time.deltaTime;
        }

        if (!realScene.isLoaded)
        {
            Debug.LogError($"❌ 场景 {scene.name} 加载超时或失败，终止恢复！");
            yield break;
        }

        // 再多等一点，确保 Awake/Start 都跑完
        yield return new WaitForSecondsRealtime(0.2f);

        Debug.Log($"✅ 场景 {realScene.name} 已完全加载，开始恢复状态");
        StartCoroutine(DelayedRestore(realScene));
    }


    private IEnumerator DelayedRestore(Scene scene)
    {
        yield return null;

        if (scene == null || string.IsNullOrEmpty(scene.name))
        {
            Debug.LogError("❌ DelayedRestore 收到空场景名，终止恢复！");
            yield break;
        }

        if (allScenes == null)
        {
            Debug.LogWarning("⚠️ allScenes 为空，重新初始化");
            allScenes = new Dictionary<string, SceneData>();
        }

        if (allScenes.TryGetValue(scene.name, out SceneData data))
        {
            int restoredCount = 0;
            foreach (var obj in FindObjectsOfType<PersistentObject>())
            {
                var state = data.objectStates.Find(s => s.objectID == obj.objectID);
                if (state != null)
                {
                    obj.gameObject.SetActive(state.isActive);
                    obj.transform.position = state.position;
                    restoredCount++;
                }
            }

            Debug.Log($"♻️ 恢复场景：{scene.name}（成功恢复 {restoredCount} 个物体状态）");
        }
        else
        {
            Debug.Log($"⚠️ 没找到 {scene.name} 的保存记录，使用默认初始状态。");
        }
    }

    // -------------------- JSON 存取 --------------------
    private void SaveAllScenes()
    {
        try
        {
            string json = JsonUtility.ToJson(new SceneCollection(allScenes), true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 保存场景数据失败：{e.Message}");
        }
    }

    private void LoadAllScenes()
    {
        if (!File.Exists(savePath))
        {
            allScenes = new Dictionary<string, SceneData>();
            Debug.Log("ℹ️ 未找到保存文件，创建新数据。");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SceneCollection wrapper = JsonUtility.FromJson<SceneCollection>(json);
            allScenes = wrapper.ToDictionary();
            Debug.Log($"✅ 已加载场景数据：{allScenes.Count} 个场景");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 加载场景数据失败：{e.Message}");
            allScenes = new Dictionary<string, SceneData>();
        }
    }

    // -------------------- 公共接口 --------------------
    public void ManualSave()
    {
        var scene = SceneManager.GetActiveScene();
        SaveScene(scene);
    }

    public void LoadSceneWithRestore(string sceneName)
    {
        StartCoroutine(LoadAndRestore(sceneName));
    }

    private IEnumerator LoadAndRestore(string sceneName)
    {
        isSwitchingScene = true; // ✅ 标志切换中

        var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!async.isDone)
            yield return null;

        isSwitchingScene = false; // ✅ 切换完毕，恢复正常保存
        Debug.Log($"✅ 场景 {sceneName} 完全加载，开始恢复");

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        StartCoroutine(DelayedRestore(loadedScene));
    }

}

// -------------------- JSON 数据封装 --------------------
[Serializable]
public class SceneCollection
{
    public List<SceneData> scenes = new List<SceneData>();

    public SceneCollection(Dictionary<string, SceneData> dict)
    {
        foreach (var pair in dict)
            scenes.Add(pair.Value);
    }

    public Dictionary<string, SceneData> ToDictionary()
    {
        Dictionary<string, SceneData> dict = new Dictionary<string, SceneData>();
        foreach (var s in scenes)
        {
            if (!string.IsNullOrEmpty(s.sceneName))
                dict[s.sceneName] = s;
        }
        return dict;
    }
}
