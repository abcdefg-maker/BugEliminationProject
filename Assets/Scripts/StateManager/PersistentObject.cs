using UnityEngine;
using System;

namespace BugElimination
{
    [DisallowMultipleComponent]
    public class PersistentObject : MonoBehaviour
    {
        [Tooltip("ÿ������Ψһ�ı�ʶ�������ڱ�����ָ�λ��/״̬��")]
        public string objectID;

        private void Reset()
        {
            if (string.IsNullOrEmpty(objectID))
                objectID = Guid.NewGuid().ToString();
        }
    }
}
