using System.Collections;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variable")]
    public int column;
    public int row;
    private int previousColumn;
    private int previousRow;
    private int targetX;
    private int targetY;
    public bool isMatch = false;


    private Board board;
    private GameObject otherDots;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPos;
    public float swipeAngle = 0;
    private float swipeResist = 1f;
    private GameManager gameManager;
    private HintManager hintManager;
    void Start()
    {
        hintManager = FindObjectOfType<HintManager>();
        gameManager = FindObjectOfType<GameManager>();
        board = FindObjectOfType<Board>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //column = targetX;
        //row = targetY;
        //previousColumn = column;
        //previousRow = row;
    }

    void Update()
    {
        FindMatchDots();
        if (isMatch)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(0f, 0f, 0f, .2f);
        }
        targetX = column;
        targetY = row;

        //Swipe in X axis
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Move torward the target
            tempPos = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPos, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }

        }
        else
        {
            //Directly set the position
            tempPos = new Vector2(targetX, transform.position.y);
            transform.position = tempPos;

        }


        //Swipe in Y axis
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Move torward the target
            tempPos = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPos, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
        }
        else
        {
            //Directly set the position
            tempPos = new Vector2(transform.position.x, targetY);
            transform.position = tempPos;

        }
    }

    IEnumerator CheckMoveCoroutine()
    {
        yield return new WaitForSeconds(.5f);
        if (otherDots != null)
        {
            if (!isMatch && !otherDots.GetComponent<Dot>().isMatch)
            {
                //Redirect block when it isn't match 
                otherDots.GetComponent<Dot>().row = row;
                otherDots.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentState = GameState.move;
            }
            else
            {
                //If it match then destroy
                board.DestroyMatch();
            }
            otherDots = null;

        }
    }

    void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            //Tracking user first touch on block
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            //Tracking user final touch on block
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Calculate angle between first and final touch position
            CalculateAngle();

        }

    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist ||
            Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;
            //Debug.Log(swipeAngle);
            MovePieces();
            board.currentState = GameState.wait;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            //Swipe right
            otherDots = board.allDots[column + 1, row];
            previousColumn = column;
            previousRow = row;
            otherDots.GetComponent<Dot>().column -= 1;
            column += 1;

        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            //Swipe up
            otherDots = board.allDots[column, row + 1];
            previousColumn = column;
            previousRow = row;
            otherDots.GetComponent<Dot>().row -= 1;
            row += 1;

        }
        else if (swipeAngle > 135 || swipeAngle <= -135 && column > 0 )
        {
            //Swipe left
            otherDots = board.allDots[column - 1, row];
            previousColumn = column;
            previousRow = row;
            otherDots.GetComponent<Dot>().column += 1;
            column -= 1;

        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0 )
        {
            //Swipe down
            otherDots = board.allDots[column, row - 1];
            previousColumn = column;
            previousRow = row;
            otherDots.GetComponent<Dot>().row += 1;
            row -= 1;

        }
        StartCoroutine(CheckMoveCoroutine());

    }

    void FindMatchDots()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.gameObject.CompareTag(this.gameObject.tag) && rightDot1.gameObject.CompareTag(this.gameObject.tag))
                {
                    leftDot1.GetComponent<Dot>().isMatch = true;
                    rightDot1.GetComponent<Dot>().isMatch = true;
                    isMatch = true;
                }

            }

        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.gameObject.CompareTag(this.gameObject.tag) && downDot1.gameObject.CompareTag(this.gameObject.tag))
                {
                    upDot1.GetComponent<Dot>().isMatch = true;
                    downDot1.GetComponent<Dot>().isMatch = true;
                    isMatch = true;
                }

            }

        }
    }



}
