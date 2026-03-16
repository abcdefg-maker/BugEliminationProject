using UnityEngine;

namespace BugElimination
{
    /// <summary>
    /// ȫ�ֳ�ʼ������
    /// ȷ�� DayNightManager �����ⳡ��������
    /// �����ڵ�һ�����صĳ�����ִ�У�
    /// </summary>
    public class GlobalInitializer : MonoBehaviour
    {
        private void Awake()
        {
            // ��� DayNightManager �Ѿ����ڣ����ظ�����
            if (DayNightManager.Instance != null)
            {
                Debug.Log(" DayNightManager �Ѵ��ڣ�" + DayNightManager.Instance.name);
                return;
            }

            // �� Resources ����Ԥ����
            GameObject prefab = Resources.Load<GameObject>("DayNightManager");

            if (prefab == null)
            {
                Debug.LogError(" δ�� Resources ���ҵ� DayNightManager.prefab��");
                return;
            }

            // ʵ���������Ϊ������
            GameObject instance = Instantiate(prefab);
            instance.name = "DayNightManager (Auto)";

            // ��ֹ��������
            if (DayNightManager.Instance == null)
            {
                var manager = instance.GetComponent<DayNightManager>();
                if (manager != null)
                {
                    DontDestroyOnLoad(instance);
                    Debug.Log(" ���Զ�ʵ���������� DayNightManager ��פ��");
                }
                else
                {
                    Debug.LogError(" Ԥ������ȱ�� DayNightManager �ű���");
                }
            }
        }
    }
}
