using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 全局剧情状态管理器：
/// 负责记录游戏中各种事件的状态（flag）
/// 可以被任意脚本调用，用于剧情控制
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    // 用来存储所有触发过的剧情标志
    private HashSet<string> flags = new HashSet<string>();

    public int currentDay = 1;

    public GameObject Boss;
    public GameObject Wang;
    public GameObject player;
    public GameObject Rlabel;
    public ChasePlayerWithDeathTMP monster;

    private void Awake()
    {
        // 单例模式，全局唯一
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景也不会销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Instance.SetFlag("CanTalkToBossDay1");
        SceneStateManager.Instance.ManualSave();
    }

    private void Update()
    {
        if (CheckFlag("Thanks"))
        {
            SceneStateManager.Instance.ManualSave();
            SceneManager.LoadScene("ThanksScene");
            RemoveFlag("Thanks");
        }
        if (CheckFlag("Result"))
        {
            Debug.Log("跳转至结局");
            SceneStateManager.Instance.ManualSave();
            SceneManager.LoadScene("ResultScene");
            RemoveFlag("Result");
        }
        if (CheckFlag("CanTalkToBossDay4"))
        {
            Wang.transform.position = new Vector3(2.7f, -1.78f, 0);
        }
        if (CheckFlag("Day4"))
        {
            Boss = GameObject.Find("Boss");
            Boss.transform.position = new Vector3(17, 2, 0); //把老板送去办公室
            Boss.SetActive(true);
            player = GameObject.Find("Player");
            player.transform.position = new Vector3(-6, 4, 0);//把主角挪到门口
            Wang = GameObject.Find("Wang"); // 根据名字找
            Wang.SetActive(true);
            Wang.transform.position = new Vector3(-3, 3.6f, 0);//把wang移到门口
            Rlabel = GameObject.Find("R_Label");
            Rlabel.SetActive(false);
        }
        if (CheckFlag("InDanger"))
        {
            monster = FindObjectOfType<ChasePlayerWithDeathTMP>();
            monster.SetChasingRange(100000);
        }
        if (CheckFlag("Day1End"))//第一天结束之后的设置
        {
            Wang = GameObject.Find("Wang"); // 根据名字找
            Wang.SetActive(false);
            //GameStateManager.Instance.RunEffectAfterFlag("Day1End", 3f, () =>
            //{
            //    DayNightManager.Instance.SwitchToDay();
                
            //});
            currentDay++;
            RemoveFlag("Day1End");
            SetFlag("Day2Start");
            Boss = GameObject.Find("Boss");
            Boss.transform.position = new Vector3(17, 2, 0); //把老板送去办公室
            player = GameObject.Find("Player");
            player.transform.position = new Vector3(-6, 4, 0);//把主角挪到门口
            Wang.SetActive(true);
            Rlabel = GameObject.Find("R_Label");
            Rlabel.SetActive(true);
            SceneStateManager.Instance.ManualSave();
        }
        if (CheckFlag("SelfTalkDay2"))
        {
            Boss = GameObject.Find("Boss");
            Boss.SetActive(false);
            Wang = GameObject.Find("Wang"); 
            Wang.SetActive(false);
            player = GameObject.Find("Player");
            player.transform.position = new Vector3(-1.7f, -1.5f, 0);
        }
        if (CheckFlag("Day2End"))
        {
            currentDay++;
            SceneStateManager.Instance.ManualSave();
            StartCoroutine(TransitionToScene("MessScene"));
            RemoveFlag("Day2End");
        }

    }

    IEnumerator TransitionToScene(string sceneName)
    {
        SceneStateManager.Instance.isSwitchingScene = true;
        SceneStateManager.Instance.ManualSave();

        yield return new WaitForSeconds(0.5f); // 可选延时
        SceneManager.LoadScene(sceneName);
        SceneStateManager.Instance.isSwitchingScene = false;
    }
    /// <summary>
    /// 设置一个剧情标志
    /// </summary>
    public void SetFlag(string flag)
    {
        if (!flags.Contains(flag))
        {
            flags.Add(flag);
            Debug.Log($"【GameState】已设置标志：{flag}");
        }
    }

    /// <summary>
    /// 检查某个剧情标志是否已触发
    /// </summary>
    public bool CheckFlag(string flag)
    {
        return flags.Contains(flag);
    }

    /// <summary>
    /// 移除剧情标志（用于调试或重置）
    /// </summary>
    public void RemoveFlag(string flag)
    {
        if (flags.Contains(flag))
        {
            flags.Remove(flag);
            Debug.Log($"【GameState】已移除标志：{flag}");
        }
    }

    /// <summary>
    /// 清空所有剧情标志（重新开始游戏时使用）
    /// </summary>
    public void ClearAllFlags()
    {
        flags.Clear();
        Debug.Log("【GameState】已清空所有剧情标志");
    }



    public void RunEffectAfterFlag(string flag, float delay, System.Action onEffect)
    {
        StartCoroutine(WaitForFlagAndRun(flag, delay, onEffect));
    }

    private IEnumerator WaitForFlagAndRun(string flag, float delay, System.Action onEffect)
    {
        // 等待 flag 出现
        yield return new WaitUntil(() => CheckFlag(flag));

        Debug.Log($"【GameState】检测到标志 {flag} 被触发，将在 {delay} 秒后执行效果。");
        yield return new WaitForSeconds(delay);

        onEffect?.Invoke();
        Debug.Log($"【GameState】标志 {flag} 延迟效果已执行。");
    }

}



