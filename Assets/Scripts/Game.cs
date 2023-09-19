using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static int gridWidth = 10;
    public static int gridHeight = 20;

    public static Transform[,] grid = new Transform[gridWidth, gridHeight];

    public int oneLineScore = 40;
    public int twoLineScore = 100;
    public int threeLineScore = 400;
    public int tetrisScore = 1200; // four lines at once

    private int rowsThisTurn = 0; // the number of lines cleared with the current piece
    private float comboMultiplier = 1; // the number of lines cleared in succession
    public static int currentScore;

    private int currentMinoID;
    private int nextMinoID;
    private AudioSource gameAudioSource;
    public AudioClip lineClearAudio;

    public Text hudScore;

    public Image previewTetrimino;
    private GameObject nextTetrimino;

    private bool gameStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        gameAudioSource = GetComponent<AudioSource>();
        SpawnNextTetrimino();
    }

    void Update()
    {
        UpdateScore();
        UpdateUI();
    }

    public void UpdateUI()
    {
        hudScore.text = currentScore.ToString();
    }

    public void UpdateScore()
    {
        if(rowsThisTurn > 0) // do nothing whilst no lines have been cleared
        {
            switch (rowsThisTurn)
            {
                case 1:
                    LineCleared(oneLineScore, 0);
                    break;
                case 2:
                    LineCleared(twoLineScore, 0.2f);
                    break;
                case 3:
                    LineCleared(threeLineScore, 0.5f);
                    break;
                case 4:
                    LineCleared(tetrisScore, 1.0f);
                    break;
            }
            rowsThisTurn = 0;
        }
    }

    public void LineCleared(int scoreToUpdate, float comboIncrement)
    {
        gameAudioSource.PlayOneShot(lineClearAudio);
        currentScore += Mathf.RoundToInt(scoreToUpdate * comboMultiplier);
        if(comboMultiplier < 2.5f)
        {
            comboMultiplier += comboIncrement;
            if(comboMultiplier > 2.5f)
            {
                comboMultiplier = 2.5f;
            }
        }  
    }

    // check to see if the piece that drops is out of the grid bounds
    public bool CheckOutOfBounds(Tetrimino tetrimino)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            foreach(Transform mino in tetrimino.transform)
            {
                Vector3 pos = RoundValues(mino.position);
                if(pos.y > gridHeight - 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // check to see if a row in the grid is full of blocks
    public bool IsRowFull (int row)
    {
        for(int i = 0; i < gridWidth; i++)
        {
            if(grid[i, row] == null)
            {
                return false;
            }
        }

        rowsThisTurn++; // row has been found, therefore increment this value
        return true;
    }

    // delete the blocks at grid position y
    public void DeleteMinoAt(int y)
    {
        for(int x = 0; x <gridWidth; x++)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    // shift the row down y, x time (where x is the number of rows cleared)
    public void ShiftRowDown(int y)
    {
        for(int x = 0; x < gridWidth; x++)
        {
            if(grid[x,y] != null)
            {
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;
                grid[x, y - 1].position = new Vector3(grid[x, y - 1].position.x, grid[x, y - 1].position.y-1, grid[x, y - 1].position.z);
            }
        }
    }

    public void ShiftAllRows(int y)
    {
        for(int i = y; i < gridHeight; i++)
        {
            ShiftRowDown(i);
        }
    }

    public void DeleteRow()
    {
        for(int y = 0; y < gridHeight; y++)
        {
            if(IsRowFull(y))
            {
                DeleteMinoAt(y);
                ShiftAllRows(y + 1);
                y--;
            }
        }
    }

    // ensure the current piece is within the grid
    public bool CheckIsInsideGrid(Vector3 position)
    {
        return ((int)position.x >= 0 && (int)position.x < gridWidth && (int)position.y >= 0);
    }

    // prevent values from having floating points
    public Vector3 RoundValues(Vector3 position)
    {
        return new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), 0);
    }

    // updates the grid when a new tetrimino lands
    public void UpdateGrid(Tetrimino tetrimino)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for(int x = 0; x < gridWidth; x++)
            {
                if(grid[x,y] != null)
                {
                    if(grid[x,y].parent == tetrimino.transform)
                    {
                        grid[x, y] = null;
                    }
                }
            }
        }

        // passes each value for each individual block in a tetrimino
        foreach(Transform mino in tetrimino.transform)
        {
            Vector3 pos = RoundValues(mino.position);
            if(pos.y < gridHeight)
            {
                grid[(int)pos.x, (int)pos.y] = mino;
            }
        }
    }

    public Transform GetTransformAtGridPosition(Vector3 pos)
    {
        if(pos.y > gridHeight -1)
        {
            return null;
        }
        else
        {
            return grid[(int)pos.x, (int)pos.y];
        }
    }

    int GetRandomNumber()
    {
        if(!gameStarted)
        {
            int randomPiece = Random.Range(1, 8); // number of unique pieces
            int randomNext = Random.Range(1, 8);
            currentMinoID = randomPiece;
            nextMinoID = randomNext;
            return currentMinoID;
        }
        else
        {
            int randomNext = Random.Range(1, 8);
            currentMinoID = nextMinoID;
            nextMinoID = randomNext;
            return currentMinoID;
        }
    }

    string GetRandomTetrimino()
    {
        
        string randomPieceName = "Prefabs/TMino"; // piece to get

        switch(currentMinoID)
        {
            case 1:
                randomPieceName = "Prefabs/TMino";
                break;
            case 2:
                randomPieceName = "Prefabs/CubeMino";
                break;
            case 3:
                randomPieceName = "Prefabs/LMino";
                break;
            case 4:
                randomPieceName = "Prefabs/SMino";
                break;
            case 5:
                randomPieceName = "Prefabs/LongMino";
                break;
            case 6:
                randomPieceName = "Prefabs/ZMino";
                break;
            case 7:
                randomPieceName = "Prefabs/JMino";
                break;
        }

        return randomPieceName;
    }

    string SetNextMinoSprite()
    {
        string nextSprite = "Sprites/T_Sprite";
        switch (nextMinoID)
        {
            case 1:
                nextSprite = "Sprites/T_Sprite";
                break;
            case 2:
                nextSprite = "Sprites/Cube_Sprite";
                break;
            case 3:
                nextSprite = "Sprites/L_Sprite";
                break;
            case 4:
                nextSprite = "Sprites/S_Sprite";
                break;
            case 5:
                nextSprite = "Sprites/Long_Sprite";
                break;
            case 6:
                nextSprite = "Sprites/Z_Sprite";
                break;
            case 7:
                nextSprite = "Sprites/J_Sprite";
                break;
        }

        return nextSprite;
    }

    public void SpawnNextTetrimino()
    {
        if(!gameStarted)
        {
            gameStarted = true;
        }
        GetRandomNumber();
        nextTetrimino = (GameObject)Instantiate(Resources.Load(GetRandomTetrimino(), typeof(GameObject)), new Vector3(5, 19, 0), Quaternion.identity);
        previewTetrimino.sprite = Resources.Load<Sprite>(SetNextMinoSprite());
        previewTetrimino.SetNativeSize();
    }

    public void GameOver()
    {
        Application.LoadLevel("GameOver");
    }
}
