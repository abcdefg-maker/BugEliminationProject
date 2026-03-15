using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WorkSceneTrigger : MonoBehaviour
{
    [Header("场景设置")]
    public string targetSceneName = "WorkScene";

    [Header("UI 引用")]
    public GameObject confirmPanel;  // 对话框 Panel
    public GameObject RLabel;
    public Button yesButton;         // YES 按钮
    public Button noButton;          // NO 按钮

    [Header("剧情条件设置")]
    [Tooltip("需要触发的剧情标志名，未满足则无法进入场景")]
    public string requiredFlag = "CanEnterWorkScene";  // 所需 flag 名称


    [Header("角色控制")]
    public GameObject Wang;
    public GameObject Boss;

    private bool isPlayerNearby = false;

    void Start()
    {
        if(RLabel != null) //如果
            RLabel.SetActive(false);

        // 初始隐藏对话框
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // 绑定按钮事件
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYesClicked);

        if (noButton != null)
            noButton.onClick.AddListener(OnNoClicked);
    }

    void Update()
    {

        if (GameStateManager.Instance.CheckFlag(requiredFlag))
            RLabel.SetActive(true);

        // 玩家在触发范围内，按下 R 弹出UI
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.R))
        {
            //Debug.Log("RRRRRR");
            ShowConfirmPanel();
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("玩家进入触发区域");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            Debug.Log(" 玩家离开触发区域");

            // 如果离开区域，自动关闭弹窗
            if (confirmPanel != null && confirmPanel.activeSelf)
            {
                confirmPanel.SetActive(false);
            }
        }
    }

    void ShowConfirmPanel()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
        else
        {
            Debug.Log("ConfirmPanel is null!");
        }
    }

    public void OnYesClicked()
    {
        Debug.Log("点击 YES，跳转场景");
        SceneStateManager.Instance.ManualSave();
        Boss.SetActive(false);
        SceneManager.LoadScene(targetSceneName);
    }

    public void OnNoClicked()
    {
        Debug.Log("点击 NO，关闭弹窗");
        confirmPanel.SetActive(false);
    }
}
