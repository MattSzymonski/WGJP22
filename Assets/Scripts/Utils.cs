using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static string IDLESTRING = "isIDLE";
    public static string STOPSTRING = "isSTOP";
    public static Color[] colors = { Color.red, Color.blue, Color.green };
    public static string[] colorNames = { "Red", "Blue", "Green" };

    public static void ResetTimer(out Mighty.MightyTimer timer, string name, float timeMin, float timeMax)
    {
        float stopDelta = Random.Range(timeMin, timeMax);
        timer = Mighty.MightyTimersManager.Instance.CreateTimer(name, stopDelta, 1f, false, true); // Create new timer (Not looping, stopped on start)
        timer.RestartTimer();
        timer.PlayTimer();
    }

    public static void ResetTimer(Mighty.MightyTimer timer)
    {
        timer.RestartTimer();
        timer.PlayTimer();
    }

    public static Mighty.MightyTimer InitializeTimer(string name, float timeMin, float timeMax)
    {
        float stopDelta = Random.Range(timeMin, timeMax);
        Mighty.MightyTimer timer = Mighty.MightyTimersManager.Instance.CreateTimer(name, stopDelta, 1f, false, true); // Create new timer (Not looping, stopped on start)
        timer.RestartTimer();
        timer.PlayTimer();
        return timer;
    }
}
