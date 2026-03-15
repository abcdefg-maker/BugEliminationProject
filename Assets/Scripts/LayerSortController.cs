using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LayerSortController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public float offset = 0f;

    // 只排序这个 Layer 上的对象
    public string targetSortingLayer = "Transform";

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // 只对目标 Layer 的物体进行排序
        if (spriteRenderer.sortingLayerName == targetSortingLayer)
        {
            int order = Mathf.RoundToInt(-(transform.position.y - offset) * 100);
            spriteRenderer.sortingOrder = order;

            //Debug 监控信息
            //Debug.Log($"[YSort] {gameObject.name} - Y: {transform.position.y:F2}, Order: {order}");
        }
    }
}


