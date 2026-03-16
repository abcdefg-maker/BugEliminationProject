using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace BugElimination
{
    public class ChasePlayerWithDeathTMP : MonoBehaviour
    {
        [Header("追击设置")]
        public float moveSpeed = 3f;
        public float chaseRange = 8f;
        public float stopDistance = 1.0f;

        [Header("寻路设置")]
        [Tooltip("网格中心（世界坐标）")]
        public Vector2 gridCenter;
        [Tooltip("网格覆盖范围")]
        public Vector2 gridSize = new Vector2(30f, 30f);
        [Tooltip("单元格大小，越小越精确但越慢")]
        public float cellSize = 0.5f;
        [Tooltip("墙壁所在的物理层")]
        public LayerMask wallLayer;
        [Tooltip("路径重新计算间隔（秒）")]
        public float pathUpdateInterval = 0.3f;

        [Header("死亡弹窗设置")]
        public GameObject deathPanel;
        public TMP_Text deathText;
        public string deathMessage = "你被清除了！再试一次！跑出去！";
        public float reloadDelay = 2.5f;

        [Header("玩家控制禁用")]
        public bool disablePlayerOnDeath = true;
        public MonoBehaviour[] playerControlScripts;

        private Rigidbody2D rb;
        private Transform player;
        private bool hasKilled = false;
        private bool frozen = false;

        // 寻路
        private GridPathfinder pathfinder;
        private List<Vector2> currentPath;
        private int pathIndex;
        private float pathUpdateTimer;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // 让怪物不与墙壁做物理碰撞（完全靠寻路避墙）
            // 这样怪物不会被墙角卡住
            var myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                foreach (var wallCol in FindObjectsOfType<EdgeCollider2D>())
                {
                    Physics2D.IgnoreCollision(myCollider, wallCol);
                }
            }

            // 构建寻路网格
            pathfinder = new GridPathfinder(gridCenter, gridSize, cellSize, wallLayer);

            // 找主角
            GameObject p = GameObject.FindGameObjectWithTag(GameConstants.Tags.Player);
            if (p != null)
                player = p.transform;
            else
                Debug.LogWarning("没找到 Tag 为 'Player' 的对象，请确认主角已设置 Tag。");

            if (deathPanel != null)
                deathPanel.SetActive(false);
        }

        void Update()
        {
            if (!frozen && GameStateManager.Instance.CheckFlag(GameConstants.Flags.ArriveExit))
            {
                frozen = true;
                if (rb != null) rb.velocity = Vector2.zero;
                currentPath = null;
            }

            if (frozen || player == null || hasKilled) return;

            // 死亡判定
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer <= stopDistance)
            {
                HandlePlayerCaught();
                return;
            }

            // 在追击范围内时定期更新路径
            if (distToPlayer <= chaseRange)
            {
                pathUpdateTimer -= Time.deltaTime;
                if (pathUpdateTimer <= 0f)
                {
                    pathUpdateTimer = pathUpdateInterval;
                    var newPath = pathfinder.FindPath(transform.position, player.position);
                    if (newPath != null)
                    {
                        currentPath = newPath;
                        pathIndex = 0;
                    }
                    else
                    {
                        Debug.LogWarning($"寻路失败: 怪物({transform.position}) → 玩家({player.position})");
                    }
                }
            }
            else
            {
                currentPath = null;
            }
        }

        void FixedUpdate()
        {
            if (frozen || player == null || hasKilled) return;
            if (currentPath == null || pathIndex >= currentPath.Count) return;

            Vector2 currentPos = (Vector2)transform.position;
            Vector2 target = currentPath[pathIndex];

            // 到达当前路径点，前进到下一个
            float distToWaypoint = Vector2.Distance(currentPos, target);
            if (distToWaypoint < cellSize * 0.5f)
            {
                pathIndex++;
                if (pathIndex >= currentPath.Count) return;
                target = currentPath[pathIndex];
            }

            Vector2 dir = (target - currentPos).normalized;

            if (rb != null)
            {
                rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
            }
            else
            {
                transform.position += (Vector3)(dir * moveSpeed * Time.fixedDeltaTime);
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

            if (rb != null) rb.velocity = Vector2.zero;

            if (deathPanel != null)
            {
                deathPanel.SetActive(true);
                if (deathText != null)
                    deathText.text = deathMessage;
            }

            if (disablePlayerOnDeath && player != null)
            {
                foreach (var s in playerControlScripts)
                    if (s != null)
                        s.enabled = false;
            }

            StartCoroutine(ReloadSceneAfterDelay(reloadDelay));
        }

        private System.Collections.IEnumerator ReloadSceneAfterDelay(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnDrawGizmosSelected()
        {
            // 追击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stopDistance);

            // 寻路网格范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube((Vector3)gridCenter, (Vector3)gridSize);

            // 当前路径
            if (currentPath != null && currentPath.Count > 1)
            {
                Gizmos.color = Color.green;
                for (int i = pathIndex; i < currentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                }
                if (pathIndex < currentPath.Count)
                {
                    Gizmos.DrawLine(transform.position, currentPath[pathIndex]);
                }
            }
        }
    }
}
