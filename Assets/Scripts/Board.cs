using System.Collections;
using UnityEngine;

public enum GameState
{
    wait,
    move
}
public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offset;
    public GameObject tilePrefabs;
    public GameObject[] dots;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    public int countAllPossibleMatch = 0;
    public int score;
    private HintManager hintManager;

    public ParticleSystem blowEffect;
    private GameManager gameManager;


    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        hintManager = FindObjectOfType<HintManager>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();

        hintManager.FindAllMatches();

    }


    public void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //Create the board
                Vector2 tempPos = new Vector2(i, j + offset);
                GameObject backgroundTile = Instantiate(tilePrefabs, tempPos, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";


                //Create dots 
                int randomDotIndex = Random.Range(0, dots.Length);
                // Variable for limit loop cirle
                int maxInteration = 0;
                while (MatchesAt(i, j, dots[randomDotIndex]) && maxInteration < 100)
                {
                    randomDotIndex = Random.Range(0, dots.Length);
                    maxInteration++;
                }
                maxInteration = 0;


                GameObject dot = Instantiate(dots[randomDotIndex], tempPos, Quaternion.identity);
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().column = i;
                dot.transform.parent = this.transform;
                dot.name = "(" + i + "," + j + ")";
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row].gameObject.CompareTag(piece.gameObject.tag) && allDots[column - 2, row].gameObject.CompareTag(piece.gameObject.tag))
            {
                return true;
            }
            if (allDots[column, row - 1].gameObject.CompareTag(piece.gameObject.tag) && allDots[column, row - 2].gameObject.CompareTag(piece.gameObject.tag))
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].gameObject.CompareTag(piece.gameObject.tag) && allDots[column, row - 2].gameObject.CompareTag(piece.gameObject.tag))
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row].gameObject.CompareTag(piece.gameObject.tag) && allDots[column - 2, row].gameObject.CompareTag(piece.gameObject.tag))
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Destroy when match 3 or more
    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatch)
        {
            Destroy(allDots[column, row]);
            //Update score
            score++;
            //Play particle
            blowEffect.transform.position = allDots[column, row].transform.position;
            Instantiate(blowEffect, allDots[column, row].transform.position, Quaternion.identity);

            allDots[column, row] = null;
        }
    }
    public void DestroyMatch()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCoroutine());
    }
    private IEnumerator DecreaseRowCoroutine()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCoroutine());
    }

    //Fill the enmpty space after blocks were destroyed
    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPos = new Vector2(i, j + offset);
                    int randomDotIndex = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[randomDotIndex], tempPos, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    //Tracking if block match with each other
    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatch)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IEnumerator FillBoardCoroutine()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatch();
        }
        yield return new WaitForSeconds(.5f);

        //If there is no block left to blow up, display restart button
        if (IsDeadLock())
        {
            Debug.Log("DEADLOCK!!!");
            gameManager.DeadLock();

        }

        currentState = GameState.move;
    }



    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        //Take the second block and save it in a holdlder
        GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        //Switching the first block to be the second possition
        allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
        //Set the first block to be the second block
        allDots[column, row] = holder;
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    //Make sure that one and two to the rght are in the board

                    if (i < width - 2)
                    {
                        //Check if the dots to the right and two to the right exist
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].CompareTag(allDots[i, j].tag)
                                && allDots[i + 2, j].CompareTag(allDots[i, j].tag))
                            {

                                return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        //Check if the dots above exist
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].CompareTag(allDots[i, j].tag)
                                && allDots[i, j + 2].CompareTag(allDots[i, j].tag))
                            {

                                return true;
                            }
                        }
                    }
                }


            }

        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadLock()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.right))
                        {
                            //Check if there any block to explode
                            hintManager.FindAllMatches();
                            return false;
                        }
                    }
                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            //Check if there any block to explode
                            hintManager.FindAllMatches();
                            return false;
                        }
                    }
                }

            }
        }
        return true;
    }

}
