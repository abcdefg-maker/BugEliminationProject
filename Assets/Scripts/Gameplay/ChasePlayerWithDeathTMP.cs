using UnityEngine;
using TMPro;                    // ✅ 引入 TextMeshPro 命名空间
using UnityEngine.SceneManagement;

namespace BugElimination
{
    public class ChasePlayerWithDeathTMP : MonoBehaviour
    {
        [Header("追击设置")]
        public float moveSpeed = 3f;
        public float chaseRange = 8f;
        public float stopDistance = 1.0f;

        [Header("死亡弹窗设置")]
        public GameObject deathPanel;    // 指向 Canvas 下的面板
        public TMP_Text deathText;       // ✅ TextMeshPro 组件
        public string deathMessage = "你被清除了！再试一次！跑出去！";
        public float reloadDelay = 2.5f; // 死亡后延迟重载的时间

        [Header("玩家控制禁用")]
        public bool disablePlayerOnDeath = true;
        public MonoBehaviour[] playerControlScripts;

        private Transform player;
        private bool hasKilled = false;

        void Start()
        {
            // 找主角
            GameObject p = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
            if (p != null)
                player = p.transform;
            else
                Debug.LogWarning("⚠️ 没找到 Tag 为 'Player' 的对象，请确认主角已设置 Tag。");

            if (deathPanel != null)
                deathPanel.SetActive(false);
        }

        void Update()
        {
            if (GameStateManager.Instance.CheckFlag(GameConstants.Flags.ArriveExit))
            {
                moveSpeed = 0;
            }
            if (player == null || hasKilled) return;

            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= stopDistance)
            {
                HandlePlayerCaught();
                return;
            }

            if (dist <= chaseRange)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
            }
        }

        public void SetChasingRange(int range)
        {
            chaseRange = range;
        }
        private void HandlePlayerCaught()
        {
            if (hasKilled) return;
            hasKilled = true;

            // ✅ 显示 TMP 文本弹窗
            if (deathPanel != null)
            {
                deathPanel.SetActive(true);
                if (deathText != null)
                    deathText.text = deathMessage;
            }

            // 禁用主角控制
            if (disablePlayerOnDeath && player != null)
            {
                foreach (var s in playerControlScripts)
                    if (s != null)
                        s.enabled = false;
            }

            // 延迟重载当前场景
            StartCoroutine(ReloadSceneAfterDelay(reloadDelay));
        }

        private System.Collections.IEnumerator ReloadSceneAfterDelay(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }
}
