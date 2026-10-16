using UnityEngine;
using System.Collections;

public class Clock : MonoBehaviour {

    //-- set start time 00:00
    public int minutes = 0;
    public int hour = 0;
    public int seconds = 0;
    public bool realTime = true;
    
    [Header("Time Flow Settings")]
    [Tooltip("Check this to make time flow backwards.")]
    public bool isBackwards = false;

    public GameObject pointerSeconds;
    public GameObject pointerMinutes;
    public GameObject pointerHours;
    
    //-- time speed factor
    public float clockSpeed = 1.0f;     // 1.0f = realtime, < 1.0f = slower, > 1.0f = faster

    //-- internal vars
    float msecs = 0;

    void Start() 
    {
        //-- set real time
        if (realTime)
        {
            hour = System.DateTime.Now.Hour;
            minutes = System.DateTime.Now.Minute;
            seconds = System.DateTime.Now.Second;
        }
    }

    void Update() 
    {
        //-- calculate time direction factor (1 for forward, -1 for backward)
        float direction = isBackwards ? -1.0f : 1.0f;

        //-- calculate time
        msecs += Time.deltaTime * clockSpeed * direction;

        if (msecs >= 1.0f)
        {
            msecs -= 1.0f;
            seconds++;
            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
                if (minutes >= 60)
                {
                    minutes = 0;
                    hour++;
                    if (hour >= 24)
                        hour = 0;
                }
            }
        }

        else if (msecs < 0.0f)
        {
            msecs += 1.0f;
            seconds--;
            if (seconds < 0)
            {
                seconds = 59;
                minutes--;
                if (minutes < 0)
                {
                    minutes = 59;
                    hour--;
                    if (hour < 0)
                        hour = 23;
                }
            }
        }

        //-- calculate pointer angles
        float rotationSeconds = (360.0f / 60.0f)  * seconds;
        float rotationMinutes = (360.0f / 60.0f)  * minutes;
        float rotationHours   = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);

        //-- draw pointers
        pointerSeconds.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationSeconds);
        pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
        pointerHours.transform.localEulerAngles   = new Vector3(0.0f, 0.0f, rotationHours);
    }
}