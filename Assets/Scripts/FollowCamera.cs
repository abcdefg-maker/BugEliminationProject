using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FollowCamera : MonoBehaviour
{
    public Transform target;   // 玩家（跟随目标）
    public float smoothSpeed = 0.125f;  // 平滑跟随速度
    public Vector3 offset;     // 偏移量（相机与玩家的距离）

    void LateUpdate()
    {
        if (target == null) return;

        // 目标位置 = 玩家位置 + 偏移量
        Vector3 desiredPosition = target.position + offset;

        // 平滑插值
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 更新相机位置
        transform.position = smoothedPosition;
    }
}
