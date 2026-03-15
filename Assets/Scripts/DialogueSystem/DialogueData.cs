using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;       // 说话者名字
    [TextArea(2, 5)]
    public string text;              // 该句台词
    public Sprite speakerPortrait;   // 说话者头像
    public AudioClip voiceClip;      // 说话者语音（可选）
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("单角色旧系统兼容字段（可忽略）")]
    public string characterName;
    public Sprite characterSprite;
    [TextArea(3, 10)]
    public string[] sentences;
    public AudioClip voiceClip;

    [Header("新系统：多角色对话内容")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("剧情事件触发")]
    [Tooltip("对话结束后要设置为 true 的 flag 名称")]
    public List<string> flagsToSet = new List<string>();

    [Tooltip("对话结束后要移除的 flag 名称")]
    public List<string> flagsToRemove = new List<string>();
}
