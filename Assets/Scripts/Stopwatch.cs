using UnityEngine;
using UnityEngine.Events;

public class Stopwatch
{
    public UnityAction OnFinish;
    public UnityAction OnStart;
    public UnityAction OnPause;
    public UnityAction OnResume;
    public UnityAction<double> OnProgressChanged;

    private double previousTime = 0;
    private double currentTime = 0;
    private bool isPlay = false;
    private bool isPause = false;

    public void Play()
    {
        if (!isPlay)
        {
            isPlay = true;
            if (isPause)
            {
                isPause = false;
                if (OnResume != null)
                {
                    OnResume();
                }
            }
            else
            {
                currentTime = 0;
                if (OnStart != null)
                {
                    OnStart();
                }
            }
        }
    }

    public void Stop()
    {
        if (isPlay)
        {
            isPlay = false;
            if (OnFinish != null)
            {
                OnFinish();
            }
        }
    }

    public void Pause()
    {
        if (isPlay)
        {
            isPlay = false;
            isPause = true;
            if (OnPause != null)
            {
                OnPause();
            }
        }
    }

    public void UpdateStopwatch()
    {
        if (isPlay)
        {
            currentTime += Time.deltaTime;
            if (Mathf.Abs((float)currentTime - (float)previousTime) >= 1 && OnProgressChanged != null)
            {
                previousTime = currentTime;
                OnProgressChanged(currentTime);
            }
        }
    }
}
