using System.Collections.Generic;
using UnityEngine;

namespace BugElimination
{
    public class MultiDialogueTrigger : MonoBehaviour
    {
        [Header("�Ի�ϵͳ����")]
        public DialogueManager dialogueManager;    // ��� DialogueManager
        public List<DialogueStage> dialogueStages; // ��ζԻ�

        [System.Serializable]
        public class DialogueStage : IDialogueStage
        {
            public string unlockFlag;
            public bool requireUnlock;
            public DialogueData dialogueData;

            string IDialogueStage.UnlockFlag => unlockFlag;
            bool IDialogueStage.RequireUnlock => requireUnlock;
            DialogueData IDialogueStage.Dialogue => dialogueData;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(GameConstants.Tags.Player)) return;

            DialogueData selected = GetCurrentDialogue();
            if (selected != null)
            {
                dialogueManager.StartDialogue(selected);
            }
            else
            {
                Debug.Log("û�з��������ĶԻ����Բ��š�");
            }
        }

        private DialogueData GetCurrentDialogue()
        {
            return DialogueStageResolver.Resolve(dialogueStages);
        }
    }
}
