using UnityEngine;

public class SimpleTestTeleport : MonoBehaviour
{
    [Header("Target Destination")]
    public Transform targetDestination; // 把你的 Door B 拖到这里！

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Transform player = other.transform.root;
            Debug.Log("--- SIMPLE TELEPORT ACTIVATED ---");

            if (targetDestination != null)
            {
                // 1. 打印出 Door B 的世界坐标，看看它到底在哪！
                Debug.Log("Door B World Position is: " + targetDestination.position);

                // 2. 在 Door B 的位置生成一个红色的临时小球，用来肉眼确认位置
                GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.transform.position = targetDestination.position;
                debugSphere.GetComponent<Renderer>().material.color = Color.red;
                Destroy(debugSphere, 3.0f); // 3秒后自动消失

                // 3. 关闭控制器传过去
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // 强制赋值位置（在 Door B 的基础上往前往上稍微抬一点，防止卡进地板）
                player.position = targetDestination.position + Vector3.up * 1.0f + targetDestination.forward * 1.0f;
                player.rotation = targetDestination.rotation;

                if (cc != null) cc.enabled = true;
            }
            else
            {
                Debug.LogError("Target Destination is NOT assigned!");
            }
        }
    }
}