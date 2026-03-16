using UnityEngine;

namespace BugElimination
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LayerSortController : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        public float offset = 0f;

        // ֻ������� Layer �ϵĶ���
        public string targetSortingLayer = "Transform";

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            // ֻ��Ŀ�� Layer �������������
            if (spriteRenderer.sortingLayerName == targetSortingLayer)
            {
                int order = Mathf.RoundToInt(-(transform.position.y - offset) * 100);
                spriteRenderer.sortingOrder = order;

                //Debug �����Ϣ
                //Debug.Log($"[YSort] {gameObject.name} - Y: {transform.position.y:F2}, Order: {order}");
            }
        }
    }
}
