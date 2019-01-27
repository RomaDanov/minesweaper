using UnityEngine;
using UnityEngine.UI;

public class InformationView : MonoBehaviour
{
    [SerializeField] private Text totalBombText;
    [SerializeField] private Text timeText;

    public void UpdateTotalBombs(string totalBombs)
    {
        totalBombText.text = totalBombs;
    }

    public void UpdateTime(string time)
    {
        timeText.text = time;
    }
}
