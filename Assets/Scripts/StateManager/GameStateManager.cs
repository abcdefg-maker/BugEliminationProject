using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugElimination
{
    /// <summary>
    /// ȫ�־���״̬��������
    /// �����¼��Ϸ�и����¼���״̬��flag��
    /// ���Ա�����ű����ã����ھ������
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        private static readonly Vector3 BossOffScreenPos = new Vector3(17, 2, 0);
        private static readonly Vector3 PlayerDoorPos = new Vector3(-6, 4, 0);
        private static readonly Vector3 WangDay4Pos = new Vector3(2.7f, -1.78f, 0);
        private static readonly Vector3 WangDoorPos = new Vector3(-3, 3.6f, 0);
        private static readonly Vector3 PlayerSelfTalkPos = new Vector3(-1.7f, -1.5f, 0);

        public static GameStateManager Instance;

        private HashSet<string> flags = new HashSet<string>();

        public int currentDay = 1;

        public GameObject Boss;
        public GameObject Wang;
        public GameObject player;
        public GameObject Rlabel;
        public ChasePlayerWithDeathTMP monster;

        private bool _day4Initialized;
        private bool _day1EndHandled;
        private bool _selfTalkDay2Initialized;
        private bool _inDangerHandled;

        private void Awake()
        {
            // ����ģʽ��ȫ��Ψһ
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // �л�����Ҳ��������
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Instance.SetFlag(GameConstants.Flags.CanTalkToBossDay1);
            SceneStateManager.Instance.ManualSave();
        }

        private void Update()
        {
            HandleSceneTransitionFlags();
            HandleCanTalkToBossDay4();
            HandleDay4Setup();
            HandleInDanger();
            HandleDay1End();
            HandleSelfTalkDay2();
            HandleDay2End();
        }

        private void HandleSceneTransitionFlags()
        {
            if (CheckFlag(GameConstants.Flags.Thanks))
            {
                SceneStateManager.Instance.ManualSave();
                SceneManager.LoadScene(GameConstants.Scenes.Thanks);
                RemoveFlag(GameConstants.Flags.Thanks);
            }
            if (CheckFlag(GameConstants.Flags.Result))
            {
                Debug.Log("��ת�����");
                SceneStateManager.Instance.ManualSave();
                SceneManager.LoadScene(GameConstants.Scenes.Result);
                RemoveFlag(GameConstants.Flags.Result);
            }
        }

        private void HandleCanTalkToBossDay4()
        {
            if (CheckFlag(GameConstants.Flags.CanTalkToBossDay4))
            {
                Wang.transform.position = WangDay4Pos;
            }
        }

        private void HandleDay4Setup()
        {
            if (CheckFlag(GameConstants.Flags.Day4) && !_day4Initialized)
            {
                _day4Initialized = true;
                Boss = GameObject.Find("Boss");
                Boss.transform.position = BossOffScreenPos;
                Boss.SetActive(true);
                player = GameObject.Find("Player");
                player.transform.position = PlayerDoorPos;
                Wang = GameObject.Find("Wang");
                Wang.SetActive(true);
                Wang.transform.position = WangDoorPos;
                Rlabel = GameObject.Find("R_Label");
                Rlabel.SetActive(false);
            }
        }

        private void HandleInDanger()
        {
            if (CheckFlag(GameConstants.Flags.InDanger) && !_inDangerHandled)
            {
                _inDangerHandled = true;
                monster = FindObjectOfType<ChasePlayerWithDeathTMP>();
                monster.SetChasingRange(100000);
            }
        }

        private void HandleDay1End()
        {
            if (CheckFlag(GameConstants.Flags.Day1End) && !_day1EndHandled)
            {
                _day1EndHandled = true;
                Wang = GameObject.Find("Wang");
                Wang.SetActive(false);
                currentDay++;
                RemoveFlag(GameConstants.Flags.Day1End);
                SetFlag(GameConstants.Flags.Day2Start);
                Boss = GameObject.Find("Boss");
                Boss.transform.position = BossOffScreenPos;
                player = GameObject.Find("Player");
                player.transform.position = PlayerDoorPos;
                Wang.SetActive(true);
                Rlabel = GameObject.Find("R_Label");
                Rlabel.SetActive(true);
                SceneStateManager.Instance.ManualSave();
            }
        }

        private void HandleSelfTalkDay2()
        {
            if (CheckFlag(GameConstants.Flags.SelfTalkDay2) && !_selfTalkDay2Initialized)
            {
                _selfTalkDay2Initialized = true;
                Boss = GameObject.Find("Boss");
                Boss.SetActive(false);
                Wang = GameObject.Find("Wang");
                Wang.SetActive(false);
                player = GameObject.Find("Player");
                player.transform.position = PlayerSelfTalkPos;
            }
        }

        private void HandleDay2End()
        {
            if (CheckFlag(GameConstants.Flags.Day2End))
            {
                currentDay++;
                SceneStateManager.Instance.ManualSave();
                StartCoroutine(TransitionToScene(GameConstants.Scenes.Mess));
                RemoveFlag(GameConstants.Flags.Day2End);
            }
        }

        IEnumerator TransitionToScene(string sceneName)
        {
            SceneStateManager.Instance.isSwitchingScene = true;
            SceneStateManager.Instance.ManualSave();

            yield return new WaitForSeconds(0.5f); // ��ѡ��ʱ
            SceneManager.LoadScene(sceneName);
            SceneStateManager.Instance.isSwitchingScene = false;
        }
        /// <summary>
        /// ����һ�������־
        /// </summary>
        public void SetFlag(string flag)
        {
            if (!flags.Contains(flag))
            {
                flags.Add(flag);
                Debug.Log($"��GameState�������ñ�־��{flag}");
            }
        }

        /// <summary>
        /// ���ĳ�������־�Ƿ��Ѵ���
        /// </summary>
        public bool CheckFlag(string flag)
        {
            return flags.Contains(flag);
        }

        /// <summary>
        /// �Ƴ������־�����ڵ��Ի����ã�
        /// </summary>
        public void RemoveFlag(string flag)
        {
            if (flags.Contains(flag))
            {
                flags.Remove(flag);
                Debug.Log($"��GameState�����Ƴ���־��{flag}");
            }
        }

        /// <summary>
        /// ������о����־�����¿�ʼ��Ϸʱʹ�ã�
        /// </summary>
        public void ClearAllFlags()
        {
            flags.Clear();
            Debug.Log("��GameState����������о����־");
        }



        public void RunEffectAfterFlag(string flag, float delay, System.Action onEffect)
        {
            StartCoroutine(WaitForFlagAndRun(flag, delay, onEffect));
        }

        private IEnumerator WaitForFlagAndRun(string flag, float delay, System.Action onEffect)
        {
            // �ȴ� flag ����
            yield return new WaitUntil(() => CheckFlag(flag));

            Debug.Log($"��GameState����⵽��־ {flag} ������������ {delay} ���ִ��Ч����");
            yield return new WaitForSeconds(delay);

            onEffect?.Invoke();
            Debug.Log($"��GameState����־ {flag} �ӳ�Ч����ִ�С�");
        }

    }
}
