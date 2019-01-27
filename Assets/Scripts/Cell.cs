using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour 
{
    public Action<Cell> OnMarkAdded;
    public Action<Cell> OnMarkRemoved;

    [SerializeField] private Image cellImage, bombImage;
	[SerializeField] private Text bombCountText;
	[SerializeField] private Color firstColor,secondColor;
    [SerializeField] private GameObject selectedImage, markImage;
	private Cell[] neighbors;

	public bool HasBomb { get; private set;}
    public bool HasMark { get; private set; }
    public bool isRevealed { get; private set;}
	public int BombCount { get; private set;}

    public void SetDefault()
    {
        HasBomb = false;
        HasMark = false;
        isRevealed = false;
        BombCount = 0;
        bombCountText.enabled = false;
        bombImage.enabled = false;
        cellImage.color = firstColor;
        GetComponent<Button>().enabled = true;
        selectedImage.SetActive(false);
        markImage.SetActive(false);
    }

	public void SetNeighbors(Cell[] neighbors)
	{
        if (neighbors != null && neighbors.Length > 0)
		{
            this.neighbors = neighbors;
            UpdateBombsInfo();
		}
	}

    public void UpdateBombsInfo()
    {
        if (neighbors != null)
        {
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (neighbors[i] != null && neighbors[i].HasBomb)
                {
                    BombCount++;
                }
            }
        }
    }

	public void AddBomb()
	{
        HasBomb = true;
	}

	public void OnRevealClick()
	{
        if ((GameManager.instance.IsBombMode || HasMark) && !GameManager.instance.IsGameOver)
        {
            HasMark = !HasMark;
            markImage.SetActive(HasMark);

            if (HasMark && OnMarkAdded != null)
            {
                OnMarkAdded(this);
            }
            else if(!HasMark && OnMarkRemoved != null)
            {
                OnMarkRemoved(this);
            }
        }
        else
        {
            isRevealed = true;
            OffInteractable();
            cellImage.color = secondColor;
            bombCountText.enabled = !HasBomb && BombCount > 0;
            bombCountText.text = BombCount.ToString();
            bombImage.enabled = HasBomb;

            if (!HasBomb && BombCount <= 0)
            {
                RevealEmptyNeighbors();
            }

            if (HasBomb)
            {
                if (!GameManager.instance.IsGameOver)
                {
                    selectedImage.SetActive(true);
                }
                GameManager.instance.GameOver();
            }
        }

        if (!GameManager.instance.IsGameOver)
        {
            GameManager.instance.CheckStateOfGame();
        }
    }

    public void OffInteractable()
    {
        GetComponent<Button>().enabled = false;
    }

	private void RevealEmptyNeighbors()
	{
		if(neighbors != null)
		{
			for (int i = 0; i < neighbors.Length; i++)
			{
				if(!neighbors[i].HasBomb && !neighbors[i].isRevealed)
				{
                    if (neighbors[i].HasMark)
                    {
                        if (OnMarkRemoved != null)
                        {
                            OnMarkRemoved(neighbors[i]);
                        }
                    }
					neighbors[i].OnRevealClick();
				}
			}
		}
	}
}
