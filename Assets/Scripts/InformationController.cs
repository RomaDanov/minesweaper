using System;
using System.Collections;
using UnityEngine;

public class InformationController : MonoBehaviour
{
    [SerializeField] private InformationView view;
    private Stopwatch stopwatch;
    private int totalBombs = 0;
    private int currentTotalBombs = 0;

    private void Awake()
    {
        GameManager.instance.OnGameCreated += (gridStats) =>
        {
            stopwatch = new Stopwatch();
            stopwatch.OnProgressChanged += UpdateTime;
            stopwatch.OnStart += () => { UpdateTime(0); };
            stopwatch.Play();
            totalBombs = gridStats.bombs;
            currentTotalBombs = totalBombs;
            UpdateTotalBombs();
        };

        GameManager.instance.OnGameOver += () =>
        {
            stopwatch.Pause();
        };

        GameManager.instance.OnMarkValueChanged += (i) =>
        {
            currentTotalBombs += i;
            UpdateTotalBombs();
        };
    }

    private void Update()
    {
        if (stopwatch != null)
        {
            stopwatch.UpdateStopwatch();
        }
    }

    private void UpdateTime(double time)
    {
        TimeSpan ts = TimeSpan.FromSeconds(time);
        string currentTime = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        view.UpdateTime(currentTime);
    }

    private void UpdateTotalBombs()
    {
        view.UpdateTotalBombs(currentTotalBombs.ToString());
    }
}
