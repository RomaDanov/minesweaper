using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct SizeStruct
{
    public int columns;
    public int rows;
}

public struct GridStats
{
    public SizeStruct size;
    public int bombs;
}

public class GameManager : MonoBehaviour 
{
	public static GameManager instance;

    public Action<GridStats> OnGameCreated;
    public Action OnGameOver;
    public Action<int> OnMarkValueChanged;

    private GridStats gridStats;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private BombsToggleController bombsToggleController;
    private List<Cell> cellsList = new List<Cell>();
    private Dictionary<int, bool> addedMarks = new Dictionary<int, bool>();
    private int totalMarks = 0;
    private bool isFirstGenerated = true;
    public bool IsGameOver { get; private set; }
    public bool IsBombMode
    {
        get
        {
            return bombsToggleController.IsBombMode;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
	{
        gridStats.bombs = 20;
        gridStats.size.columns = 11;
        gridStats.size.rows = 11;
        isFirstGenerated = true;
        GenerateNewGame(gridStats);
    }

    public void GenerateNewGame(GridStats stats)
    {
        IsGameOver = false;
        gridStats = stats;

        //Определяем нужное кол-во ячеек
        float gridHeight = gridLayout.gameObject.GetComponent<RectTransform>().rect.height;
        float gridWidth = gridLayout.gameObject.GetComponent<RectTransform>().rect.width;
        int cellsCount = gridLayout.transform.childCount - (gridStats.size.columns * gridStats.size.rows);

        //Задаем размер ячейки 
        gridLayout.cellSize = new Vector2(gridWidth / gridStats.size.columns, gridHeight / gridStats.size.rows);

        //Удаляем лишние ячейки
        if (cellsCount > 0)
        {
            GameObject[] cellsToDestroy = new GameObject[cellsCount];
            for (int i = 0; i < cellsToDestroy.Length; i++)
            {
                cellsToDestroy[i] = gridLayout.transform.GetChild(i).gameObject;
                cellsList.Remove(cellsToDestroy[i].GetComponent<Cell>());
            }

            foreach (GameObject cellToDestroy in cellsToDestroy)
            {
                Destroy(cellToDestroy);
            }
        }

        //Либо добавляем необходимое кол-во ячеек
        else if(cellsCount < 0)
        {
            int count = Mathf.Abs(cellsCount);
            for (int i = 0; i < count; i++)
            {
                Cell newCell = Instantiate(cellPrefab, gridLayout.transform);
                newCell.gameObject.name = newCell.transform.GetSiblingIndex().ToString();
                newCell.OnMarkAdded += AddMark;
                newCell.OnMarkRemoved += RemoveMark;
                cellsList.Add(newCell);
            }
        }

        //Сбрасываем все ячейки к начальному состоянию
        foreach (Cell cell in cellsList)
        {
            cell.SetDefault();
        }

        //Задаем бомбу случайным ячейкам
        List<int> previousIndexes = new List<int>();
        if (gridStats.bombs < cellsList.Count)
        {
            for (int i = 0; i < gridStats.bombs; i++)
            {
                int random = UnityEngine.Random.Range(0, cellsList.Count);
                if (gridStats.bombs > 1)
                {
                    while (Array.Find(previousIndexes.ToArray(), cellIndex => cellIndex == random) == random)
                    {
                        random = UnityEngine.Random.Range(0, cellsList.Count);
                    }
                }
                previousIndexes.Add(random);
                cellsList[random].AddBomb();
                Debug.Log(random + " has a bomb!");
            }
        }

        //Сбрасываем список всех флажков
        addedMarks = new Dictionary<int, bool>();
        totalMarks = 0;

        //Задаём ячейкам соседей, если первая генерация сетки, либо если размер сетки поменялся
        if (isFirstGenerated || cellsCount != 0)
        {
            foreach (Cell cell in cellsList)
            {
                StartCoroutine(SetNeighborsWithDelay(cell));
            }
        }

        //Обновляем кол-во соседей с бомбами, если размер сетки не поменялся 
        else if (cellsCount == 0)
        {
            foreach (Cell cell in cellsList)
            {
                cell.UpdateBombsInfo();
            }
        }

        //Вызываем событие о том, что сетка сгенерировалась
        if (OnGameCreated != null)
        {
            OnGameCreated(gridStats);
        }

        //Первая генерация прошла
        isFirstGenerated = false;
    }

    IEnumerator SetNeighborsWithDelay(Cell forCell)
    {
        yield return new WaitForEndOfFrame();
        forCell.SetNeighbors(GetNeighbors(forCell));
    }

	private Cell[] GetNeighbors(Cell forCell)
	{
		List<Cell> temp = new List<Cell>();
		float off = 0.01f;
		for (int i = 0; i < cellsList.Count; i++)
		{
			if(cellsList[i] != forCell)
			{
				Vector2 delta = forCell.GetComponent<RectTransform>().anchoredPosition - cellsList[i].GetComponent<RectTransform>().anchoredPosition;
				if(Mathf.Abs(delta.x) <= gridLayout.cellSize.x + off && Mathf.Abs(delta.y) <= gridLayout.cellSize.y + off)
				{
					temp.Add(cellsList[i]);
				}

                if (temp.Count >= 8)
                {
                    break;
                }
			}
		}

		Cell[] neighbors = temp.ToArray();
		return neighbors;
	}

	public void GameOver()
	{
        if (!IsGameOver)
		{
            IsGameOver = true;
			for (int i = 0; i < cellsList.Count; i++)
			{
                if (cellsList[i].HasBomb && !cellsList[i].HasMark)
                {
                    cellsList[i].OnRevealClick();
                }
                else
                {
                    cellsList[i].OffInteractable();
                }
            }

            if (OnGameOver != null)
            {
                OnGameOver();
            }
        }
	}

	public void CheckStateOfGame()
	{
        int bombCount = 0;
		for (int i = 0; i < cellsList.Count; i++)
		{
			if(!cellsList[i].HasBomb && cellsList[i].isRevealed)
			{
				bombCount++;
			}
		}

		if(bombCount == cellsList.Count - gridStats.bombs || (bombCount == cellsList.Count - gridStats.bombs && CheckCorrectMarks()))
		{
            IsGameOver = true;

            for (int i = 0; i < cellsList.Count; i++)
            {
                if (!cellsList[i].isRevealed)
                {
                    cellsList[i].OnRevealClick();
                }
            }

            if (OnGameOver != null)
            {
                OnGameOver();
            }
        }
	}

    private void AddMark(Cell toCell)
    {
        addedMarks.Add(Array.FindIndex(cellsList.ToArray(), cell => cell == toCell), toCell.HasBomb);
        totalMarks++;
        if (totalMarks >= 0 && totalMarks <= gridStats.bombs)
        {
            if (OnMarkValueChanged != null)
            {
                OnMarkValueChanged(-1);
            }
        }
    }

    private void RemoveMark(Cell fromCell)
    {
        addedMarks.Remove((Array.FindIndex(cellsList.ToArray(), cell => cell == fromCell)));
        if (totalMarks >= 0 && totalMarks <= gridStats.bombs)
        {
            if (OnMarkValueChanged != null)
            {
                OnMarkValueChanged(1);
            }
        }
        totalMarks--;
    }

    private bool CheckCorrectMarks()
    {
        if (addedMarks.Count > 0)
        {
            int correctMarks = 0;
            foreach (KeyValuePair<int, bool> item in addedMarks)
            {
                if (item.Value == true)
                {
                    correctMarks++;
                }
            }

            if (correctMarks == gridStats.bombs)
            {
                return true;
            }
        }

        return false;
    }

    public void OnRestartGameClick()
	{
        GenerateNewGame(gridStats);
	}
}
