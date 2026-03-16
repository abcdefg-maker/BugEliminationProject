using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugElimination
{
    /// <summary>
    /// ͨ�� NPC �ࣺ
    /// ֧�ֶ�׶ζԻ������� flag ���ơ��Զ��ƶ��ȹ���
    /// </summary>
    public class NPC : MonoBehaviour
    {
        [Header("������Ϣ")]
        public string npcName;
        public Sprite npcPortrait;
        public float moveSpeed = 2f;

        [Header("�Ի�ϵͳ")]
        public DialogueManager dialogueManager;
        public List<NPCDialogueStage> dialogueStages = new List<NPCDialogueStage>();

        [Header("������Ϊ")]
        public string triggerFlagAfterTalk; // �Ի����Զ����õľ����־����Ϊ�գ�
        public bool canMoveAfterTalk = false;
        public Vector3 moveTargetPosition;

        private bool _isTalking = false;
        private bool _hasMoved = false;

        [System.Serializable]
        public class NPCDialogueStage : IDialogueStage
        {
            public string unlockFlag;
            public bool requireUnlock;
            public DialogueData dialogue;

            string IDialogueStage.UnlockFlag => unlockFlag;
            bool IDialogueStage.RequireUnlock => requireUnlock;
            DialogueData IDialogueStage.Dialogue => dialogue;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameConstants.Tags.Player) && !_isTalking)
            {
                DialogueData selected = GetAvailableDialogue();
                if (selected != null && dialogueManager != null)
                {
                    _isTalking = true;
                    dialogueManager.onDialogueEnd += OnDialogueEnded;
                    dialogueManager.StartDialogue(selected);
                }
                else
                {
                    Debug.Log($"{npcName} ��ǰû�п��öԻ���δ���� DialogueManager��");
                }
            }
        }

        /// <summary>
        /// ��ѡ��ǰ�ܲ��ŵĶԻ��׶�
        /// </summary>
        private DialogueData GetAvailableDialogue()
        {
            return DialogueStageResolver.Resolve(dialogueStages);
        }

        private void OnDialogueEnded()
        {
            _isTalking = false;

            if (!string.IsNullOrEmpty(triggerFlagAfterTalk))
            {
                GameStateManager.Instance.SetFlag(triggerFlagAfterTalk);
            }

            if (canMoveAfterTalk && !_hasMoved)
            {
                StartCoroutine(MoveToTarget());
            }
        }

        /// <summary>
        /// NPC �Զ��ƶ�
        /// </summary>
        private IEnumerator MoveToTarget()
        {
            _hasMoved = true;

            while (Vector3.Distance(transform.position, moveTargetPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, moveTargetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            Debug.Log($"{npcName} �ƶ���ɡ�");
        }
    }
}
