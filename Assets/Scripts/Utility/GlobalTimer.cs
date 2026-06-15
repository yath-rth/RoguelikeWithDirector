using System;
using System.Collections.Generic;
using UnityEngine;
public class TimerRequest
{
    public float triggerTime;
    public Action callback;

    public TimerRequest(float delay, Action callback)
    {
        this.triggerTime = GlobalTimer.instance.Timer + delay;
        this.callback = callback;
    }
}

public class GlobalTimer : Singleton<GlobalTimer>
{
    private List<TimerRequest> timers;
    public float Timer { get; private set; }

    new void Awake()
    {
        base.Awake();
        timers = new List<TimerRequest>();
    }

    public void AddTimer(TimerRequest req)
    {
        timers.Add(req);
    }

    void Update()
    {
        if (timers.Count == 0)
        {
            return;
        }

        Timer += Time.deltaTime;

        for (int i = timers.Count - 1; i >= 0; i--)
        {
            if (Timer >= timers[i].triggerTime)
            {
                timers[i].callback?.Invoke();
                timers.RemoveAt(i);
            }
        }
    }
}