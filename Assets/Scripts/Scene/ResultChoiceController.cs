using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BugElimination
{
    /// <summary>
    /// 结局分支控制器：序章对话 → 弹出选项 → 播放对应分支对话
    /// </summary>
    public class ResultChoiceController : MonoBehaviour
    {
        [Header("对话")]
        public DialogueManager dialogueManager;
        public DialogueData prologueDialogue;   // 序章（共通部分）
        public DialogueData choiceADialogue;     // 选择A：螺丝钉结局
        public DialogueData choiceBDialogue;     // 选择B：意义结局

        [Header("选项UI")]
        public GameObject choicePanel;           // 选项面板
        public Button buttonA;                   // "还是快点把bug消除好了"
        public Button buttonB;                   // "把bug调出来看看"

        private bool _triggered;

        private void Start()
        {
            if (choicePanel != null)
                choicePanel.SetActive(false);

            if (buttonA != null)
                buttonA.onClick.AddListener(OnChoiceA);

            if (buttonB != null)
                buttonB.onClick.AddListener(OnChoiceB);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag(GameConstants.Tags.Player)) return;

            _triggered = true;
            dialogueManager.onDialogueEnd += OnPrologueEnd;
            dialogueManager.StartDialogue(prologueDialogue);
        }

        private void OnPrologueEnd()
        {
            StartCoroutine(ShowChoiceNextFrame());
        }

        private IEnumerator ShowChoiceNextFrame()
        {
            yield return null;

            // 重新激活 Canvas
            dialogueManager.canvasForDialogue.SetActive(true);

            // 隐藏对话面板，使 DialogueManager.Update() 中
            // dialoguePanel.activeSelf 为 false，不再拦截鼠标点击
            dialogueManager.dialoguePanel.SetActive(false);

            if (choicePanel != null)
                choicePanel.SetActive(true);
        }

        private void OnChoiceA()
        {
            choicePanel.SetActive(false);
            dialogueManager.dialoguePanel.SetActive(true);
            dialogueManager.onDialogueEnd += OnBranchEnd;
            dialogueManager.StartDialogue(choiceADialogue);
        }

        private void OnChoiceB()
        {
            choicePanel.SetActive(false);
            dialogueManager.dialoguePanel.SetActive(true);
            dialogueManager.onDialogueEnd += OnBranchEnd;
            dialogueManager.StartDialogue(choiceBDialogue);
        }

        /// <summary>
        /// 分支对话结束后的兜底：如果 GameStateManager 不存在
        /// （例如从 ResultScene 直接测试），手动跳转 ThanksScene
        /// </summary>
        private void OnBranchEnd()
        {
            if (GameStateManager.Instance == null)
            {
                Debug.Log("【ResultChoiceController】GameStateManager 不存在，手动跳转 ThanksScene");
                SceneManager.LoadScene(GameConstants.Scenes.Thanks);
            }
        }
    }
}
