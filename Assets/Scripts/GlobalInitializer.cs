using UnityEngine;

/// <summary>
/// 全局初始化器：
/// 确保 DayNightManager 在任意场景都存在
/// （放在第一个加载的场景中执行）
/// </summary>
public class GlobalInitializer : MonoBehaviour
{
    private void Awake()
    {
        // 如果 DayNightManager 已经存在，则不重复生成
        if (DayNightManager.Instance != null)
        {
            Debug.Log(" DayNightManager 已存在：" + DayNightManager.Instance.name);
            return;
        }

        // 从 Resources 加载预制体
        GameObject prefab = Resources.Load<GameObject>("DayNightManager");

        if (prefab == null)
        {
            Debug.LogError(" 未在 Resources 中找到 DayNightManager.prefab！");
            return;
        }

        // 实例化并标记为不销毁
        GameObject instance = Instantiate(prefab);
        instance.name = "DayNightManager (Auto)";

        // 防止多重销毁
        if (DayNightManager.Instance == null)
        {
            var manager = instance.GetComponent<DayNightManager>();
            if (manager != null)
            {
                DontDestroyOnLoad(instance);
                Debug.Log(" 已自动实例化并保持 DayNightManager 常驻！");
            }
            else
            {
                Debug.LogError(" 预制体中缺少 DayNightManager 脚本！");
            }
        }
    }
}
