using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A zone trigger that makes specified objects rotate around their OWN pivot points 
/// while the player is inside the zone, and triggers a one-time fade out for clones.
/// </summary>
public class ZoneRotateAndFade : MonoBehaviour
{
    [Header("Zone Trigger Settings")]
    [Tooltip("The tag of the player entering the zone.")]
    public string playerTag = "Player";
    
    // Tracks if the player is currently inside the trigger zone
    private bool isPlayerInZone = false;
    // Ensures the fade-out event only triggers the first time the player enters
    private bool hasFadedClones = false;

    [Header("Local Rotation Settings")]
    [Tooltip("Drag the objects you want to rotate in place here.")]
    public List<Transform> objectsToRotate = new List<Transform>();
    
    [Tooltip("The local axis to rotate around (e.g., 0, 1, 0 for local Y-axis).")]
    public Vector3 rotationAxis = Vector3.up;
    
    [Tooltip("How fast the objects spin around their own pivots.")]
    public float rotationSpeed = 60f;

    [Header("Fade Out Settings")]
    [Tooltip("The Tag of the clones you want to fade out.")]
    public string cloneTag = "GuideClone";
    
    [Tooltip("How many seconds it takes for the clones to fade away.")]
    public float fadeDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = true; // 玩家进入区域，开启自转
            
            // 消失动画只需要触发一次，不需要每次进出都跑一遍
            if (!hasFadedClones)
            {
                hasFadedClones = true;
                StartCoroutine(FadeAndDestroyClones());
                Debug.Log("[Zone] Player entered zone. Fading clones and starting local rotation.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = false; // 玩家离开区域，停止自转
            Debug.Log("[Zone] Player left zone. Rotation paused.");
        }
    }

    private void Update()
    {
        // 只要玩家在这个区域内，每帧都会让物体原地自转
        if (isPlayerInZone)
        {
            foreach (Transform obj in objectsToRotate)
            {
                if (obj != null)
                {
                    // Space.Self 确保物体是围绕它自己的内部坐标轴(Pivot)转动
                    obj.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
                }
            }
        }
    }

    /// <summary>
    /// Smoothly fades out and destroys objects with the specified tag.
    /// </summary>
    private IEnumerator FadeAndDestroyClones()
    {
        GameObject[] clones = GameObject.FindGameObjectsWithTag(cloneTag);

        if (clones.Length == 0) yield break;

        List<Material> cloneMaterials = new List<Material>();
        foreach (GameObject clone in clones)
        {
            Renderer[] renderers = clone.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                cloneMaterials.Add(r.material);
            }
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            foreach (Material mat in cloneMaterials)
            {
                if (mat != null)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }
                    else if (mat.HasProperty("_BaseColor"))
                    {
                        Color c = mat.GetColor("_BaseColor");
                        c.a = alpha;
                        mat.SetColor("_BaseColor", c);
                    }
                }
            }
            yield return null;
        }

        foreach (GameObject clone in clones)
        {
            if (clone != null) Destroy(clone);
        }
    }
}