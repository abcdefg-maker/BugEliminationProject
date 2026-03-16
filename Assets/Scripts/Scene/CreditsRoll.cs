using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BugElimination
{
    public class CreditsRoll : MonoBehaviour
    {
        [Header("滚动文本对象")]
        public RectTransform creditsContent;    // 包含所有文字的 RectTransform

        [Header("滚动参数")]
        public float scrollSpeed = 50f;         // 滚动速度（像素/秒）
        public float endDelay = 2f;             // 滚动完后的停留时间（秒）

        [Header("跳转设置")]
        public string nextScene = "MainMenu";   // 滚动结束后跳转的场景名

        private float startY;
        private float endY;
        private bool isRolling = false;

        void Start()
        {
            if (creditsContent == null)
            {
                Debug.LogError("❌ 请在 Inspector 中指定 creditsContent！");
                return;
            }

            // 记录初始位置
            startY = creditsContent.anchoredPosition.y;

            // 根据文字高度计算目标位置
            float screenHeight = Screen.height;
            endY = creditsContent.sizeDelta.y + screenHeight;

            // 开始滚动
            isRolling = true;
        }

        void Update()
        {
            if (!isRolling) return;

            // 不断上移内容
            Vector2 pos = creditsContent.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;
            creditsContent.anchoredPosition = pos;

            // 检查是否滚动到目标
            if (pos.y >= endY)
            {
                isRolling = false;
                StartCoroutine(WaitAndLoad());
            }
        }

        private System.Collections.IEnumerator WaitAndLoad()
        {
            Debug.Log("🎬 鸣谢滚动结束，准备返回主菜单。");
            yield return new WaitForSeconds(endDelay);
            SceneManager.LoadScene(nextScene);
        }
    }
}
