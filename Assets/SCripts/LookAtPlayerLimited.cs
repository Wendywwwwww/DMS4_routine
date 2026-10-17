using UnityEngine;

public class LookAtPlayerLimited : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("需要注视的玩家 Transform。如果为空，脚本会自动通过 Tag 寻找 Player")]
    public Transform playerTarget;

    [Header("模型朝向修正（偏移度）")]
    [Tooltip("如果头骨面部没有精准对着玩家，可以在这里调整 X/Y/Z 的角度偏移（例如 Y 填 90 或 -90）")]
    public Vector3 rotationOffset = Vector3.zero;

    [Header("旋转限制与速度")]
    [Tooltip("旋转平滑速度")]
    public float rotationSpeed = 5f;

    [Tooltip("允许的最大左右旋转角度 (例如 80 度，表示左 80 度到右 80 度)")]
    public float maxRotationAngle = 80f;

    [Header("轴向锁定")]
    [Tooltip("勾选后，头骨只会水平旋转（Y轴），不会抬头或低头")]
    public bool lockYAxisOnly = true;

    // 记录初始的正前方朝向
    private Quaternion initialRotation;

    void Start()
    {
        // 保存物体生成时的初始朝向作为“正前方”基准
        initialRotation = transform.rotation;

        // 如果没有手动指定 Player，自动尝试查找带 "Player" Tag 的物体
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        // 1. 计算从头骨指向玩家的方向向量
        Vector3 targetDirection = playerTarget.position - transform.position;

        // 如果勾选了只在 Y 轴旋转（只水平看玩家，不上看下看）
        if (lockYAxisOnly)
        {
            targetDirection.y = 0;
        }

        if (targetDirection.sqrMagnitude < 0.001f) return;

        // 2. 计算朝向玩家的基础旋转 Quaternion，并叠加偏移度
        Quaternion rawTargetRotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(rotationOffset);

        // 3. 限制旋转角度（计算目标朝向与初始朝向之间的夹角）
        Quaternion clampedRotation = LimitRotation(initialRotation * Quaternion.Euler(rotationOffset), rawTargetRotation, maxRotationAngle);

        // 4. 平滑旋转过渡
        transform.rotation = Quaternion.Slerp(transform.rotation, clampedRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// 将旋转限制在基准朝向的最大角度内
    /// </summary>
    private Quaternion LimitRotation(Quaternion baseRotation, Quaternion targetRotation, float maxAngle)
    {
        // 计算目标朝向相对于初始朝向的角度差
        float angle = Quaternion.Angle(baseRotation, targetRotation);

        if (angle > maxAngle)
        {
            // 如果超出限制，进行角度截断
            float t = maxAngle / angle;
            return Quaternion.Slerp(baseRotation, targetRotation, t);
        }

        return targetRotation;
    }
}