using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用 NPC 类：
/// 支持多阶段对话、剧情 flag 控制、自动移动等功能
/// </summary>
public class NPC : MonoBehaviour
{
    [Header("基础信息")]
    public string npcName;
    public Sprite npcPortrait;
    public float moveSpeed = 2f;

    [Header("对话系统")]
    public DialogueManager dialogueManager;
    public List<NPCDialogueStage> dialogueStages = new List<NPCDialogueStage>();

    [Header("剧情行为")]
    public string triggerFlagAfterTalk; // 对话后自动设置的剧情标志（可为空）
    public bool canMoveAfterTalk = false;
    public Vector3 moveTargetPosition;

    private bool _isTalking = false;
    private bool _hasMoved = false;

    [System.Serializable]
    public class NPCDialogueStage
    {
        public string unlockFlag;       // 解锁条件
        public bool requireUnlock;      // 是否需要flag
        public DialogueData dialogue;   // 对应对话数据
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isTalking)
        {
            DialogueData selected = GetAvailableDialogue();
            if (selected != null && dialogueManager != null)
            {
                _isTalking = true;
                dialogueManager.StartDialogue(selected);

                // 在对话结束后执行剧情事件
                StartCoroutine(WaitForDialogueEnd());
            }
            else
            {
                Debug.Log($"{npcName} 当前没有可用对话或未分配 DialogueManager。");
            }
        }
    }

    /// <summary>
    /// 挑选当前能播放的对话阶段
    /// </summary>
    private DialogueData GetAvailableDialogue()
    {
        DialogueData result = null;

        foreach (var stage in dialogueStages)
        {
            if (stage.requireUnlock)
            {
                if (GameStateManager.Instance != null &&
                    GameStateManager.Instance.CheckFlag(stage.unlockFlag))
                {
                    result = stage.dialogue;
                }
            }
            else
            {
                result = stage.dialogue; // 默认阶段
            }
        }

        return result;
    }

    /// <summary>
    /// 等待对话结束后执行剧情操作
    /// （检测 dialogueManager 的激活状态）
    /// </summary>
    private IEnumerator WaitForDialogueEnd()
    {
        // 等待对话面板关闭（表示对话结束）
        while (dialogueManager != null && dialogueManager.dialoguePanel.activeSelf)
        {
            yield return null;
        }

        _isTalking = false;

        // 对话结束后设置剧情标志
        if (!string.IsNullOrEmpty(triggerFlagAfterTalk))
        {
            GameStateManager.Instance.SetFlag(triggerFlagAfterTalk);
        }

        // 可选：触发NPC移动
        if (canMoveAfterTalk && !_hasMoved)
        {
            StartCoroutine(MoveToTarget());
        }
    }

    /// <summary>
    /// NPC 自动移动
    /// </summary>
    private IEnumerator MoveToTarget()
    {
        _hasMoved = true;

        while (Vector3.Distance(transform.position, moveTargetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTargetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log($"{npcName} 移动完成。");
    }
}
