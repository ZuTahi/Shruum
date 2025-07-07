using UnityEngine;

public class ComboTimer
{
    private float lastAttackTime;
    private float timeoutDuration;

    public ComboTimer(float timeout)
    {
        timeoutDuration = timeout;
        lastAttackTime = -timeout;
    }

    public void ResetTimer()
    {
        lastAttackTime = Time.time;
    }

    public bool HasTimedOut()
    {
        return Time.time - lastAttackTime > timeoutDuration;
    }
}
