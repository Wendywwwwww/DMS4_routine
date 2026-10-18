using UnityEngine;
using UnityEngine.XR;
using StarterAssets; // 引用 Starter Assets

public class SimpleVRBridge : MonoBehaviour
{
    [Header("PC Player 核心组件")]
    [Tooltip("把你原先 Player 上的 StarterAssetsInputs 脚本拖进这里")]
    public StarterAssetsInputs playerInputs;
    
    [Header("VR 交互设置")]
    [Tooltip("把你层级里的 Right Controller 拖进这里")]
    public Transform rightController; 
    public float interactDistance = 5f;

    private InputDevice leftControllerDevice;
    private InputDevice rightControllerDevice;

    private bool wasLeftTriggerPressed;
    private bool wasRightTriggerPressed;

    [Header("右手指引激光")]
    public bool showLaser = true;
    private LineRenderer laserLine;

    void Start()
    {
        // 自动给右手柄添加一条红色的激光束，方便你瞄准可交互物品
        if (showLaser && rightController != null)
        {
            laserLine = rightController.gameObject.AddComponent<LineRenderer>();
            laserLine.startWidth = 0.005f;
            laserLine.endWidth = 0.005f;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = Color.red;
            laserLine.endColor = Color.red;
        }
    }

    void Update()
    {
        // 1. 获取 VR 物理手柄设备 (绕过 Input Actions UI 的配置)
        if (!leftControllerDevice.isValid) 
            leftControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (!rightControllerDevice.isValid) 
            rightControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // 2. 左手：移动 (摇杆) 和 跳跃 (扳机)
        if (playerInputs != null && leftControllerDevice.isValid)
        {
            // 摇杆控制移动
            if (leftControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 moveDir))
            {
                playerInputs.MoveInput(moveDir);
            }

            // 扳机控制跳跃
            if (leftControllerDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTrigger))
            {
                if (leftTrigger && !wasLeftTriggerPressed)
                {
                    playerInputs.JumpInput(true);
                }
                else if (!leftTrigger)
                {
                    playerInputs.JumpInput(false);
                }
                wasLeftTriggerPressed = leftTrigger;
            }
        }

        // 3. 右手：激光射线瞄准 与 扳机交互
        if (rightController != null && rightControllerDevice.isValid)
        {
            // 实时更新激光射线的终点
            if (showLaser && laserLine != null)
            {
                laserLine.SetPosition(0, rightController.position);
                Ray laserRay = new Ray(rightController.position, rightController.forward);
                if (Physics.Raycast(laserRay, out RaycastHit hitInfo, interactDistance))
                {
                    laserLine.SetPosition(1, hitInfo.point);
                }
                else
                {
                    laserLine.SetPosition(1, rightController.position + rightController.forward * interactDistance);
                }
            }

            // 右侧扳机按下时进行交互
            if (rightControllerDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTrigger))
            {
                if (rightTrigger && !wasRightTriggerPressed)
                {
                    PerformRaycastInteract();
                }
                wasRightTriggerPressed = rightTrigger;
            }
        }
    }

    private void PerformRaycastInteract()
    {
        Ray ray = new Ray(rightController.position, rightController.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // 终极魔法：不需要去每个物品上写 VR 逻辑！
            // SendMessage 会自动寻找目标物体上所有名叫 "Interact" 的方法并执行。
            // 只要你的 PC 脚本里写了 public void Interact()，这里一枪打过去就能直接触发！
            hit.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
            
            // 为了防止脚本挂在父/子物体层级不匹配，顺便向上广播一下
            hit.collider.SendMessageUpwards("Interact", SendMessageOptions.DontRequireReceiver);
            
            Debug.Log($"[VR 交互] 右手射线命中了: {hit.collider.name}");
        }
    }
}