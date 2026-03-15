using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 元素")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI nameTextInBottom;
    public TextMeshProUGUI dialogueText;
    public Image characterImage;
    public GameObject dialoguePanel;
    public GameObject canvasForDialogue;

    [Header("对话参数")]
    public float typingSpeed = 0.03f;

    [Header("角色语音")]
    public AudioSource audioSource;
    public AudioClip defaultVoiceClip;
    private AudioClip currentVoiceClip;

    // 支持旧结构
    private string[] sentences;
    private int currentIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;

    // 支持新结构
    private DialogueData currentDialogue;
    private bool useNewSystem = false;


    private void Start()
    {
        canvasForDialogue.SetActive(false);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        

        canvasForDialogue.SetActive(true);
        currentDialogue = dialogue;
        currentIndex = 0;

        // 检测是否使用新版 lines
        useNewSystem = (dialogue.lines != null && dialogue.lines.Count > 0);

        if (useNewSystem)
        {
            // 多角色对话模式
            ShowDialogueLine();
        }
        else
        {
            // 旧单角色模式
            nameText.text = dialogue.characterName;
            nameTextInBottom.text = dialogue.characterName;

            if (dialogue.characterSprite != null)
            {
                characterImage.sprite = dialogue.characterSprite;
                characterImage.gameObject.SetActive(true);
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }

            sentences = dialogue.sentences;
            ShowSentence();
        }
    }

    // ——新版多角色台词——
    private void ShowDialogueLine()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        var line = currentDialogue.lines[currentIndex];

        // 显示说话者信息
        nameText.text = line.speakerName;
        nameTextInBottom.text = line.speakerName;

        if (line.speakerPortrait != null)
        {
            characterImage.sprite = line.speakerPortrait;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }

        // 播放该角色语音（如有）
        currentVoiceClip = line.voiceClip != null ? line.voiceClip : defaultVoiceClip;

        typingCoroutine = StartCoroutine(TypeSentence(line.text));
        Debug.Log($"当前显示文本: {line.text}");
    }

    // ——旧版单角色台词——
    private void ShowSentence()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(sentences[currentIndex]));
        Debug.Log($"当前显示文本: {sentences[currentIndex]}");
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        if (currentVoiceClip != null && audioSource != null)
        {
            audioSource.clip = currentVoiceClip;
            audioSource.Play();
        }

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        isTyping = false;
    }

    public void NextSentence()
    {
        if (isTyping) return;
        currentIndex++;

        if (useNewSystem)
        {
            // 多角色对话
            if (currentIndex < currentDialogue.lines.Count)
            {
                ShowDialogueLine();
            }
            else
            {
                EndDialogue();
            }
        }
        else
        {
            // 单角色对话
            if (currentIndex < sentences.Length)
            {
                ShowSentence();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void EndDialogue()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        // 对话结束后修改剧情状态
        if (currentDialogue != null)
        {
            // 设置 flag
            foreach (var flag in currentDialogue.flagsToSet)
            {
                GameStateManager.Instance.SetFlag(flag);
                Debug.Log($"【DialogueManager】已触发 SetFlag: {flag}");
            }

            // 移除 flag
            foreach (var flag in currentDialogue.flagsToRemove)
            {
                GameStateManager.Instance.RemoveFlag(flag);
                Debug.Log($"【DialogueManager】已触发 RemoveFlag: {flag}");
            }
        }

        canvasForDialogue.SetActive(false);

       
    }


    private void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            NextSentence();
        }
    }

    public void SetCharacterSide(bool isLeft)
    {
        RectTransform rt = characterImage.GetComponent<RectTransform>();
        Vector2 pos = rt.anchoredPosition;
        pos.x = isLeft ? -300 : 300;
        rt.anchoredPosition = pos;
    }
}
