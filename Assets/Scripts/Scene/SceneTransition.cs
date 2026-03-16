using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugElimination
{
    public class SceneTransition : MonoBehaviour
    {
        [Header("要跳转的场景名")]
        public string targetScene = "OfficeScene";

        [Header("玩家标签名")]
        public string playerTag = "Player";


        private void OnTriggerStay2D(Collider2D other)
        {
            // 检测进入的是否是玩家
            if (other.CompareTag(playerTag) && GameStateManager.Instance.CheckFlag(GameConstants.Flags.CanLeave))
            {
                Debug.Log($"🎯 玩家进入触发区，切换到场景：{targetScene}");
                GameStateManager.Instance.SetFlag(GameConstants.Flags.Day4);
                GameStateManager.Instance.currentDay++;
                SceneManager.LoadScene(targetScene);
            }
        }
    }
}
