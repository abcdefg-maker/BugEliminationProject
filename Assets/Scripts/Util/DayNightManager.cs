using UnityEngine;
using System.Collections;

namespace BugElimination
{
    public class DayNightManager : MonoBehaviour
    {
        public static DayNightManager Instance { get; private set; }

        [Header("������ҹ���� SpriteRenderer")]
        public SpriteRenderer daySprite;
        public SpriteRenderer nightSprite;

        [Header("���ɲ���")]
        public float transitionDuration = 2f; // ���뵭��ʱ��

        private bool isTransitioning = false;
        private bool isDay = true; // ��ǰ�Ƿ�Ϊ����

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); //  �����л�������
            foreach (Transform child in transform)
            {
                DontDestroyOnLoad(child.gameObject);
            }
        }


        private void Start()
        {
            // ��ʼ��͸����
            SetAlpha(daySprite, 1f);
            SetAlpha(nightSprite, 0f);
        }

        /// <summary>
        /// һ���л���ҹ״̬���Զ��жϵ�ǰ״̬��
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
        /// ֱ���е�ҹ��
        /// </summary>
        public void SwitchToNight()
        {
            if (!isDay && !isTransitioning) return;
            if (!isTransitioning)
                StartCoroutine(Transition(daySprite, nightSprite));
        }

        /// <summary>
        /// ֱ���е�����
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
}
