using UnityEngine;

/// <summary>
/// Makes pool balls float on a purely visual animated water surface using a matching sine wave formula.
/// </summary>
public class WaterFloatable : MonoBehaviour
{
    [Header("Water Visual Settings")]
    [Tooltip("The base world Y position of your water plane (e.g., 0).")]
    public float waterBaseY = 0f;

    [Tooltip("Must match your water Shader's wave height/amplitude.")]
    public float waveAmplitude = 0.3f;

    [Tooltip("Must match your water Shader's wave speed/frequency.")]
    public float waveFrequency = 3f;

    [Header("Buoyancy Settings")]
    [Tooltip("Upward push force when the ball is below the virtual wave height.")]
    public float floatStrength = 30f;

    [Tooltip("Damping to stabilize the ball and prevent bouncing into the sky.")]
    public float waterDrag = 8f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // If waterBaseY is left at 0, you can automatically capture the water's actual Y position on start
        GameObject waterObj = GameObject.Find("Water"); // 你的水面物体名字
        if (waterObj != null)
        {
            waterBaseY = waterObj.transform.position.y;
        }
    }

    void FixedUpdate()
    {
        // 1. Calculate the virtual wave height at this ball's exact X and Z position
        // This mirrors standard vertex shader wave formulas
        float virtualWaveY = waterBaseY + Mathf.Sin(Time.time * waveFrequency + transform.position.x * 2f + transform.position.z * 2f) * waveAmplitude;

        // 2. Check if the ball has fallen below the virtual wave surface
        float ballY = transform.position.y;
        float depth = virtualWaveY - ballY;

        if (depth > 0f)
        {
            // Apply upward buoyancy force proportional to the submerge depth
            Vector3 buoyancyForce = Vector3.up * (depth * floatStrength);
            rb.AddForce(buoyancyForce, ForceMode.Acceleration);

            // Apply water drag/damping to make it float softly
            Vector3 vel = rb.linearVelocity;
            vel.y *= (1f - waterDrag * Time.fixedDeltaTime);
            rb.linearVelocity = vel;
        }
    }
}