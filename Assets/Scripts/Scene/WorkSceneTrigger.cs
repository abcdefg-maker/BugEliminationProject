using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace BugElimination
{
    public class WorkSceneTrigger : MonoBehaviour
    {
        [Header("��������")]
        public string targetSceneName = "WorkScene";

        [Header("UI ����")]
        public GameObject confirmPanel;  // �Ի��� Panel
        public GameObject RLabel;
        public Button yesButton;         // YES ��ť
        public Button noButton;          // NO ��ť

        [Header("������������")]
        [Tooltip("��Ҫ�����ľ����־����δ�������޷����볡��")]
        public string requiredFlag = "CanEnterWorkScene";  // ���� flag ����


        [Header("��ɫ����")]
        public GameObject Wang;
        public GameObject Boss;

        private bool isPlayerNearby = false;

        void Start()
        {
            if(RLabel != null) //���
                RLabel.SetActive(false);

            // ��ʼ���ضԻ���
            if (confirmPanel != null)
                confirmPanel.SetActive(false);

            // �󶨰�ť�¼�
            if (yesButton != null)
                yesButton.onClick.AddListener(OnYesClicked);

            if (noButton != null)
                noButton.onClick.AddListener(OnNoClicked);
        }

        void Update()
        {

            if (GameStateManager.Instance.CheckFlag(requiredFlag))
                RLabel.SetActive(true);

            // ����ڴ�����Χ�ڣ����� R ����UI
            if (isPlayerNearby && Input.GetKeyDown(KeyCode.R))
            {
                //Debug.Log("RRRRRR");
                ShowConfirmPanel();
            }

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameConstants.Tags.Player))
            {
                isPlayerNearby = true;
                Debug.Log("��ҽ��봥������");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(GameConstants.Tags.Player))
            {
                isPlayerNearby = false;
                Debug.Log(" ����뿪��������");

                // ����뿪�����Զ��رյ���
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
            Debug.Log("��� YES����ת����");
            SceneStateManager.Instance.ManualSave();
            Boss.SetActive(false);
            SceneManager.LoadScene(targetSceneName);
        }

        public void OnNoClicked()
        {
            Debug.Log("��� NO���رյ���");
            confirmPanel.SetActive(false);
        }
    }
}
