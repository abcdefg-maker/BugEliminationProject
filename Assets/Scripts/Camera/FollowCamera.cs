using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugElimination
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;   // ��ң�����Ŀ�꣩
        public float smoothSpeed = 0.125f;  // ƽ�������ٶ�
        public Vector3 offset;     // ƫ�������������ҵľ��룩

        void LateUpdate()
        {
            if (target == null) return;

            // Ŀ��λ�� = ���λ�� + ƫ����
            Vector3 desiredPosition = target.position + offset;

            // ƽ����ֵ
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // �������λ��
            transform.position = smoothedPosition;
        }
    }
}
