using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 用于处理字符串选择、高亮、光标闪烁、字符串删除（只允许删除指定 targetString）
/// 支持从本地文件读取 originalString 与 targetString 配置
/// </summary>
public class StringSelectionHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI References")]
    public TMP_Text sequenceText;         // 显示 0/1 序列的 TextMeshPro 组件
    public TMP_Text hintText;             // 提示目标字符串（例如 "目标字符串：101"）
    public RectTransform cursor;          // 光标 UI（竖线），通过 Image 控制闪烁

    [Header("配置")]
    [TextArea(3, 10)]
    public string originalString;         // 原始 0/1 序列
    public string targetString = "101";   // 允许删除的目标字符串
    public char placeholderChar = '·';    // 删除后显示的占位符字符
    public float cursorBlinkSpeed = 0.5f; // 光标闪烁速度（单位：秒）

    [Header("文件路径")]
    public string relativeTxtPath = "Imf/sequence_config.txt"; // 从 Assets 开始的相对路径

    // —— 内部变量 ——
    private int selectionStartIndex = -1;  // 选择起点下标
    private int selectionEndIndex = -1;    // 选择终点下标
    private bool isSelecting = false;      // 是否正在选择中
    private Coroutine blinkCoroutine;      // 光标闪烁协程句柄

    [Header("完成提示")]
    public TMP_Text completionText;
    public float completionFadeTime = 2f; // 淡出时长（秒）



    void Start()
    {
        // 启动时从配置文件读取字符串
        LoadConfigFromRelativePath();

        // 显示初始字符串与目标提示
        sequenceText.text = originalString;
        hintText.text = $"目标字符串:<color=yellow>{targetString}</color>";

        // 隐藏光标（初始不显示）
        if (cursor != null)
            cursor.gameObject.SetActive(false);

    }

    /// <summary>
    /// 从文本文件读取 originalString 和 targetString
    /// 文件格式：
    /// original: 010101
    /// target: 101
    /// </summary>
    void LoadConfigFromRelativePath()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativeTxtPath).Replace("\\", "/");

        Debug.Log("配置文件路径：" + fullPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError("配置文件未找到：" + fullPath);
            return;
        }

        string[] lines = File.ReadAllLines(fullPath);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // 遍历文本行
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("original:"))
            {
                // 读取原始字符串
                sb.Append(line.Substring("original:".Length).Trim());
            }
            else if (line.StartsWith("target:"))
            {
                // 读取目标字符串
                targetString = line.Substring("target:".Length).Trim();
            }
            else
            {
                sb.AppendLine();
                sb.Append(line.Trim());
            }
        }

        // 清理换行符等
        originalString = sb.ToString().Replace("\r", "");
        targetString = targetString.Trim();

        Debug.Log("originalString:\n" + originalString);
        Debug.Log("targetString: " + targetString);
    }

    /// <summary>
    /// 当鼠标按下（或触摸按下）时，确定选择的起点
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("文本点击事件触发！点击位置：" + eventData.position);
        // 找到点击位置对应的字符索引
        int intersect = TMP_TextUtilities.FindIntersectingCharacter(sequenceText, eventData.position, null, true);
        Debug.Log("intersect is " + intersect);
        int nearest = TMP_TextUtilities.FindNearestCharacter(sequenceText, eventData.position, null, true);

        if (intersect == -1)
        {
            // 没有点到字符，清空选择并隐藏光标
            ClearSelection();
            HideCursor();
        }
        else
        {
            // 记录起始位置
            selectionStartIndex = nearest;
            selectionEndIndex = nearest;
            isSelecting = true;

            HighlightSelection(); // 立刻高亮当前字符
            StartCoroutine(ShowCursorWithDelay(nearest)); // 稍后显示光标
        }
    }

    /// <summary>
    /// 鼠标拖拽时，更新 selectionEndIndex 并高亮选区
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isSelecting) return;
        int nearest = TMP_TextUtilities.FindNearestCharacter(sequenceText, eventData.position, null, true);
        selectionEndIndex = nearest;
        HighlightSelection();
        HideCursor(); // 拖动时不显示光标
    }

    /// <summary>
    /// 鼠标抬起时，结束选择
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isSelecting = false;
    }

    /// <summary>
    /// 延迟一点点再显示光标，避免点选后立刻被隐藏
    /// </summary>
    IEnumerator ShowCursorWithDelay(int index)
    {
        yield return new WaitForSeconds(0.05f);
        ShowCursorAtIndex(index);
    }

    /// <summary>
    /// 高亮当前选中的字符串（使用 TextMeshPro 的 mark 标签）
    /// </summary>
    void HighlightSelection()
    {
        if (selectionStartIndex == -1 || selectionEndIndex == -1) return;

        int start = Mathf.Min(selectionStartIndex, selectionEndIndex);
        int end = Mathf.Max(selectionStartIndex, selectionEndIndex);

        string before = originalString.Substring(0, start);
        string selected = originalString.Substring(start, end - start + 1);
        string after = originalString.Substring(end + 1);

        // 使用黄色半透明高亮
        sequenceText.text = $"{before}<mark=#FFFF00AA>{selected}</mark>{after}";
    }

    /// <summary>
    /// 在指定字符位置显示光标
    /// </summary>
    void ShowCursorAtIndex(int index)
    {
        if (cursor == null) return;
        if (index < 0 || index >= sequenceText.textInfo.characterCount) return;

        var charInfo = sequenceText.textInfo.characterInfo[index];
        Vector3 worldPos = charInfo.bottomLeft; // 字符左下角的世界坐标

        Vector2 localPoint;
        // 将世界坐标转换为 UI 本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sequenceText.rectTransform,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPoint
        );

        cursor.anchoredPosition = localPoint;
        cursor.gameObject.SetActive(true);
        StartBlink();
    }

    /// <summary>
    /// 启动光标闪烁协程
    /// </summary>
    void StartBlink()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(CursorBlink());
    }

    /// <summary>
    /// 光标闪烁逻辑：不断切换 Image 的可见状态
    /// </summary>
    IEnumerator CursorBlink()
    {
        Image img = cursor.GetComponent<Image>();
        while (cursor.gameObject.activeSelf)
        {
            img.enabled = !img.enabled;
            yield return new WaitForSeconds(cursorBlinkSpeed);
        }
    }

    /// <summary>
    /// 隐藏光标并停止闪烁
    /// </summary>
    void HideCursor()
    {
        if (cursor == null) return;
        cursor.gameObject.SetActive(false);
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    /// <summary>
    /// 清空选择状态并恢复原始字符串显示
    /// </summary>
    public void ClearSelection()
    {
        selectionStartIndex = -1;
        selectionEndIndex = -1;
        sequenceText.text = originalString;
    }

    /// <summary>
    /// 删除选中的字符串，但只能删除与 targetString 完全匹配的内容
    /// </summary>
    public void DeleteSelection()
    {
        if (selectionStartIndex == -1 || selectionEndIndex == -1) return;

        int start = Mathf.Min(selectionStartIndex, selectionEndIndex);
        int end = Mathf.Max(selectionStartIndex, selectionEndIndex);
        int length = end - start + 1;

        string selected = originalString.Substring(start, length);

        // 检查是否是目标字符串
        if (selected != targetString)
        {
            Debug.LogWarning($"只能删除 \"{targetString}\" ，当前选中的是 \"{selected}\"");
            StartCoroutine(FlashInvalidSelection(start, length));
            return;
        }

        // 替换为占位符
        var sb = new System.Text.StringBuilder(originalString);
        for (int i = start; i <= end; i++)
        {
            sb[i] = placeholderChar;
        }
        originalString = sb.ToString();

        // 更新 UI
        sequenceText.text = originalString;
        ClearSelection();
        HideCursor();

        // 注意：检测应该在更新 originalString 后
        if (!originalString.Contains(targetString))
        {
            Debug.Log("检测到所有目标字符都已删除！");
            ShowCompletionMessage();
        }
    }



    /// <summary>
    /// 当删除的内容不合法时，红色闪烁高亮提醒用户
    /// </summary>
    IEnumerator FlashInvalidSelection(int start, int length)
    {
        string before = originalString.Substring(0, start);
        string selected = originalString.Substring(start, length);
        string after = originalString.Substring(start + length);

        for (int i = 0; i < 2; i++)
        {
            // 红色闪烁
            sequenceText.text = $"{before}<mark=#FF0000AA>{selected}</mark>{after}";
            yield return new WaitForSeconds(0.15f);
            sequenceText.text = originalString;
            yield return new WaitForSeconds(0.15f);
        }
        HighlightSelection(); // 恢复原有高亮
    }

    //显示完成任务之后的UI界面
    void ShowCompletionMessage()
    {
        if (completionText == null) return;

        // 显示提示
        sequenceText.gameObject .SetActive(false);
        hintText.gameObject .SetActive(false);
        completionText.gameObject.SetActive(true);
        completionText.text = "恭喜你!\n你完成了今天的工作!";

        // 先重置透明度
        Color c = completionText.color;
        c.a = 1f;
        completionText.color = c;

        // 启动淡出协程
        StartCoroutine(FadeOutCompletion());
    }

    IEnumerator FadeOutCompletion()
    {
        yield return new WaitForSeconds(2f);

        float duration = completionFadeTime;
        float elapsed = 0f;
        Color startColor = completionText.color;
        Color endColor = startColor; endColor.a = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            completionText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        completionText.gameObject.SetActive(false);


        // 切回办公室
        GameStateManager.Instance.RemoveFlag("CanEnterWorkScene");

        if (GameStateManager.Instance.currentDay == 1)
        {
            GameStateManager.Instance.SetFlag("CanTalkToWang?Day1");
            SceneStateManager.Instance.ManualSave();
            SceneManager.LoadScene("OfficeScene");
        }
        else if (GameStateManager.Instance.currentDay == 2)
        {
            GameStateManager.Instance.SetFlag("SelfTalkDay2");
            SceneStateManager.Instance.ManualSave();
            SceneManager.LoadScene("OfficeScene");
        }
        else if(GameStateManager.Instance.currentDay == 3)
        {
            GameStateManager.Instance.SetFlag("SelfTalkDay3");
            SceneStateManager.Instance.ManualSave();
            SceneManager.LoadScene("PuzzleScene");
        }
        else
        {
            Debug.Log("忘记设置跳转回哪里了吧。。。");
        }
    }


}
