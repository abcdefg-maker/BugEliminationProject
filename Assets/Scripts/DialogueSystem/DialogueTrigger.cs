/*using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DialogueData dialogueData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogueManager == null)
            {
                Debug.LogError("DialogueManager 没有绑定！");
                return;
            }

            if (dialogueData == null)
            {
                Debug.LogError("DialogueData 没有绑定！");
                return;
            }

            dialogueManager.StartDialogue(dialogueData);
        }

    }
}
*/
using System.Collections.Generic;
using UnityEngine;

public class MultiDialogueTrigger : MonoBehaviour
{
    [Header("对话系统引用")]
    public DialogueManager dialogueManager;    // 你的 DialogueManager
    public List<DialogueStage> dialogueStages; // 多段对话

    [System.Serializable]
    public class DialogueStage
    {
        public string unlockFlag;              // 解锁条件（剧情标识）
        public bool requireUnlock;             // 是否需要flag才能触发
        public DialogueData dialogueData;      // 当前阶段的对话
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        DialogueData selected = GetCurrentDialogue();
        if (selected != null)
        {
            dialogueManager.StartDialogue(selected);
        }
        else
        {
            Debug.Log("没有符合条件的对话可以播放。");
        }
    }

    private DialogueData GetCurrentDialogue()
    {
        DialogueData result = null;

        // 遍历所有阶段，找到满足条件的“最新阶段”
        foreach (var stage in dialogueStages)
        {
            if (stage.requireUnlock)
            {
                if (GameStateManager.Instance != null &&
                    GameStateManager.Instance.CheckFlag(stage.unlockFlag))
                {
                    result = stage.dialogueData;
                }
            }
            else
            {
                result = stage.dialogueData; // 默认阶段（没有flag）
            }
        }

        return result;
    }
}
