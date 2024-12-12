using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class that manages the candies and their positions
public class Candy : MonoBehaviour
{
    public CandyType candyType;

    public int xPos;
    public int yPos;

    //to keep track if the candy is part of a match
    public bool isMatched;

    private Vector2 currentPos;
    private Vector2 targetPos;

    //to keep track if the candy is moving
    public bool isMoving;

    //constructor
    public Candy(int myX, int myY)
    {
        xPos = myX;
        yPos = myY;
    }

    //method that sets the position of a candy
    public void setPosition(int myX, int myY)
    {
        xPos = myX;
        yPos = myY;
    }

    //method that moves a candy to a target position
    public void MoveToTarget(Vector2 endPos)
    {
        StartCoroutine(SmoothSwapCandy(endPos));
    }

    //coroutine that create a smooth animation when two candies are swap
    private IEnumerator SmoothSwapCandy(Vector2 endPos)
    {
        float duration = 0.2f;
        float elapsedTime = 0f;

        //current position of the candy
        Vector2 startPos = transform.position;

        while(elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            //goes towards the end position (little bt little)
            transform.position = Vector2.Lerp(startPos, endPos, t);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        //make sure the final position is at the exact destination
        transform.position = endPos;
    }
    
}

public enum CandyType
{
    Blue,
    Green,
    Purple,
    Red,
    Yellow
}
