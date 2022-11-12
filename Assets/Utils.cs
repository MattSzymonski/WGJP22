using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static string IDLESTRING = "isIDLE";
    public static string STOPSTRING = "isSTOP";
    public static void ResetTimer(out Mighty.MightyTimer timer, float timeMin, float timeMax)
    {
        float stopDelta = Random.Range(timeMin, timeMax);
        timer = Mighty.MightyTimersManager.Instance.CreateTimer("StopDurationTimer", stopDelta, 1f, false, true); // Create new timer (Not looping, stopped on start)
        timer.RestartTimer();
        timer.PlayTimer();
    }
}
