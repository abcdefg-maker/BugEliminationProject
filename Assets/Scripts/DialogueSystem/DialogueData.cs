using UnityEngine;
using System.Collections.Generic;

namespace BugElimination
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;       // ˵��������
        [TextArea(2, 5)]
        public string text;              // �þ�̨��
        public Sprite speakerPortrait;   // ˵����ͷ��
        public AudioClip voiceClip;      // ˵������������ѡ��
    }

    [CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/DialogueData")]
    public class DialogueData : ScriptableObject
    {
        [Header("����ɫ��ϵͳ�����ֶΣ��ɺ��ԣ�")]
        public string characterName;
        public Sprite characterSprite;
        [TextArea(3, 10)]
        public string[] sentences;
        public AudioClip voiceClip;

        [Header("��ϵͳ�����ɫ�Ի�����")]
        public List<DialogueLine> lines = new List<DialogueLine>();

        [Header("�����¼�����")]
        [Tooltip("�Ի�������Ҫ����Ϊ true �� flag ����")]
        public List<string> flagsToSet = new List<string>();

        [Tooltip("�Ի�������Ҫ�Ƴ��� flag ����")]
        public List<string> flagsToRemove = new List<string>();
    }
}
