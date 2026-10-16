using UnityEngine;

/// <summary>
/// Registers the persistent prompt UI globally so managers can find it across scenes.
/// </summary>
public class PromptReference : MonoBehaviour
{
    public static PromptReference Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}