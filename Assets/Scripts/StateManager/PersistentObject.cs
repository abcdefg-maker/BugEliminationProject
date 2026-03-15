using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PersistentObject : MonoBehaviour
{
    [Tooltip("每个物体唯一的标识符，用于保存与恢复位置/状态。")]
    public string objectID;

    private void Reset()
    {
        if (string.IsNullOrEmpty(objectID))
            objectID = Guid.NewGuid().ToString();
    }
}
