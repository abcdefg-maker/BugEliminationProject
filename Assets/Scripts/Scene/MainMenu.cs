using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugElimination
{
    public class MainMenu : MonoBehaviour
    {
        [Header("场景名称设置")]
        public string gameScene = "OfficeScene";   // 开始游戏的目标场景
        public string creditsScene = "ThanksScene"; // 制作名单场景

        /// <summary>
        /// 点击「开始游戏」按钮
        /// </summary>
        public void StartGame()
        {
            Debug.Log("🎮 开始游戏 → 加载场景：" + gameScene);
            SceneManager.LoadScene(gameScene);
        }

        /// <summary>
        /// 点击「制作名单」按钮
        /// </summary>
        public void OpenCredits()
        {
            Debug.Log("🎬 打开制作名单 → 加载场景：" + creditsScene);
            SceneManager.LoadScene(creditsScene);
        }

        /// <summary>
        /// 点击「退出游戏」按钮
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("❌ 退出游戏");
#if UNITY_EDITOR
            // 编辑器模式下停止运行
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 构建后的游戏中退出程序
            Application.Quit();
#endif
        }
    }
}
