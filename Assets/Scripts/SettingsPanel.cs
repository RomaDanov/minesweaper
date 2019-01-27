using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private InputField minesField;
    [SerializeField] private Text minesDescriptionText;
    [SerializeField, Range(1,30)] int rangeMinMines = 10;
    [SerializeField, Range(30, 100)] int rangeMaxMines = 30;
    [SerializeField] private SizeStruct[] gridSizes;
    private int selectedSizeIndex = 2;
    private int minMines = 10;
    private int maxMines = 50;

    public void OnMinesFieldValueEnd(string text)
    {
        int mines = Int32.Parse(text);
        if (mines == 0)
        {
            minesField.text = minMines.ToString();
        }
        else if (mines > maxMines)
        {
            minesField.text = maxMines.ToString();
        }
    }

    public void OnSizeSelect(int sizeIndex) // 1 = small, 2 = middle, 3 = big
    {
        selectedSizeIndex = sizeIndex;
        minMines = Mathf.FloorToInt((gridSizes[selectedSizeIndex].columns * gridSizes[selectedSizeIndex].rows) * (rangeMinMines * 0.01f));
        maxMines = Mathf.FloorToInt((gridSizes[selectedSizeIndex].columns * gridSizes[selectedSizeIndex].rows) * (rangeMaxMines * 0.01f));
        UpdateMinesDescriptionText();
        minesField.text = Mathf.FloorToInt(((minMines + maxMines) / 2)).ToString();
    }

    private void UpdateMinesDescriptionText()
    {
        minesDescriptionText.text = minMines + " - " + maxMines;
    }

    public void OnApplyClick()
    {
        GridStats stats = new GridStats();
        stats.size = gridSizes[selectedSizeIndex];
        stats.bombs = Int32.Parse(minesField.text);
        GameManager.instance.GenerateNewGame(stats);
        gameObject.SetActive(false);
    }
}
