using UnityEngine;

public class DoorMarker : MonoBehaviour
{
    // 在 Unity 编辑器 Scene 视图中，为这个空物体绘制一个大箭头和名字，方便你一眼看到它在哪里
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.3f); // 在空物体位置画个蓝球
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.5f); // 画一根绿色箭头代表正前方
    }
}