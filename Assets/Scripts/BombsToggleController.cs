using UnityEngine;
using UnityEngine.UI;

public class BombsToggleController : MonoBehaviour
{
    [SerializeField] private Image checkmark;
    [SerializeField] private Color onColor, offColor;
    public bool IsBombMode { get; private set;}

    public void SetCheckmarkColor(bool isOn)
    {
        IsBombMode = isOn;
        checkmark.color = IsBombMode ? onColor : offColor;
    }
}
