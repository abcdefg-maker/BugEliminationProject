using UnityEngine;
using System.Collections;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    [Header("白天与夜晚的 SpriteRenderer")]
    public SpriteRenderer daySprite;
    public SpriteRenderer nightSprite;

    [Header("过渡参数")]
    public float transitionDuration = 2f; // 淡入淡出时长

    private bool isTransitioning = false;
    private bool isDay = true; // 当前是否为白天

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); //  场景切换不销毁
        foreach (Transform child in transform)
        {
            DontDestroyOnLoad(child.gameObject);
        }
    }


    private void Start()
    {
        // 初始化透明度
        SetAlpha(daySprite, 1f);
        SetAlpha(nightSprite, 0f);
    }

    /// <summary>
    /// 一键切换日夜状态（自动判断当前状态）
    /// </summary>
    public void SwitchDayNight()
    {
        if (isTransitioning) return;

        if (isDay)
            StartCoroutine(Transition(daySprite, nightSprite));
        else
            StartCoroutine(Transition(nightSprite, daySprite));
    }

    /// <summary>
    /// 直接切到夜晚
    /// </summary>
    public void SwitchToNight()
    {
        if (!isDay && !isTransitioning) return;
        if (!isTransitioning)
            StartCoroutine(Transition(daySprite, nightSprite));
    }

    /// <summary>
    /// 直接切到白天
    /// </summary>
    public void SwitchToDay()
    {
        if (isDay && !isTransitioning) return;
        if (!isTransitioning)
            StartCoroutine(Transition(nightSprite, daySprite));
    }

    private IEnumerator Transition(SpriteRenderer from, SpriteRenderer to)
    {
        isTransitioning = true;
        float timer = 0f;

        while (timer < transitionDuration)
        {
            float t = timer / transitionDuration;
            SetAlpha(from, 1 - t);
            SetAlpha(to, t);
            timer += Time.deltaTime;
            yield return null;
        }

        SetAlpha(from, 0f);
        SetAlpha(to, 1f);

        isDay = (to == daySprite);
        isTransitioning = false;
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
