using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

//Class that generates the board. Mastermind class of the game
public class Board : MonoBehaviour
{
    //Objectives counters
    private int counterBlue = 0;
    private int counterGreen = 0;
    private int counterRed = 0;
    private int counterYellow = 0;
    private int counterPurple = 0;
    public TMP_Text textBlueMatch;
    public TMP_Text textGreenMatch;
    public TMP_Text textRedMatch;
    public TMP_Text textYellowMatch;
    public TMP_Text textPurpleMatch;

    //Level we are trying to play
    public int level;

    //size of the board
    public int width = 8;
    public int height = 8;

    //spacing for the board
    public float spacingX;
    public float spacingY;

    //reference to our candies composing the board
    public GameObject[] candyPrefab;

    //create a 2 dimension array of tiles, our 8x8 board
    private Tile[,] candyBoard;

    //variables to calculate the score
    private int scoreCombo = 5;

    //game objects to destroy when we reload a new board
    List<GameObject> candiesToDestroy = new();

    //candies to destroy when we make matches
    List<Candy> candiesToRemove = new();

    //list of candy to swap when clicked
    List<Candy> candyToSwap = new();

    //keep count of the number of moves
    public int nbMoves = 15;
    public TMP_Text textNbMoves;

    //keep count of the score
    public TMP_Text textScore;
    public int score = 0;

    //keep count of the time
    public TMP_Text textTimer;
    private float remainingTime = 90f;

    //Kepp track if the player wins or loses
    private bool hasWon = false;
    public GameOver gameOver;
    public Win winMenu;

    //keep track of the audio related to our board
    private AudioSource[] myAudioSources;
    private bool hasLoweredPitch = false;

    //keeps track of the number of candies refilled
    private int nbCandiesRefilled = 0;

    // Start is called before the first frame update
    void Start()
    {
        InitializeBoard();
        myAudioSources = GetComponents<AudioSource>();

        //play the main music
        myAudioSources[0].Play();
    }

    private void Update()
    {
        //set and update the timer
        SetTimer();

        //update the progress display
        UpdateProgressDisplay();

        //When the player loses
        if((nbMoves == 0 || remainingTime <= 0.01) && !hasLoweredPitch)
        {
            //to make sure we only lower the pitch once and not every frame
            hasLoweredPitch = true;

            //lower the pitch of the audio
            myAudioSources[0].pitch = 0.8f;

            //display the game over overlay
            gameOver.GameOverDisplay(score); 

            //stop the passing of time
            Time.timeScale = 0;
        }

        //if the player wins
        if((counterBlue >=3 && counterGreen >= 3 && counterPurple >= 3 && counterRed >= 3 && counterYellow >= 3) && hasWon == false)
        {
            //so that this is executed once and not every frame
            hasWon = true;

            //display the win overlay
            winMenu.WinDisplay(score);

            //disable the monobehaviour to be able to have animation in our overlay
            this.enabled = false;
        }

        //Detects when the player clicks on a candy then process matches if any
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            //if we hit the collider of a candy
            if(hit.collider != null && hit.collider.gameObject.GetComponent<Candy>())
            {
                //once the list has reached 2 elements, it clears out if click on another element
                if(candyToSwap.Count == 2)
                {
                    candyToSwap.Clear();
                }

                //get the clicked candy and add it to list of candy to swap
                Candy clickedCandy = hit.collider.gameObject.GetComponent<Candy>();
                candyToSwap.Add(clickedCandy);

                //check if the 2 candies in the list are a valid selection
                if(CheckIfValidSelection())
                {
                    //swap the 2 candies
                    swap();

                    //process the matches 
                    StartCoroutine(ProcessMatches());
                }

            }
        }
    }

    //methods that set up the timer
    private void SetTimer()
    {
        //reduce the time of our timer
        remainingTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        //set up the format of the timer
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    //method that update the TMP in the UI overlay
    private void UpdateProgressDisplay()
    {
        //update the objectives
        textBlueMatch.text = counterBlue.ToString() + "/3";
        textGreenMatch.text = counterGreen.ToString() + "/3";
        textRedMatch.text = counterRed.ToString() + "/3";
        textYellowMatch.text = counterYellow.ToString() + "/3";
        textPurpleMatch.text = counterPurple.ToString() + "/3";

        //update the nbMoves and score
        textNbMoves.text = nbMoves.ToString();
        textScore.text = score.ToString();
    }
  
    //method that hides or show all the candies in the board (for the pause menu)
    public void HideCandy(bool hides)
    {
        //hides all the candies
        if(hides)
        {
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    candyBoard[x, y].candy.SetActive(false);
                }
            }
        }
        //show all the candies
        if(!hides)
        {
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    candyBoard[x, y].candy.SetActive(true);
                }
            }
        }
        
     }
    
    //method that swaps the 2 candies in the candy to swap list
    void swap()
    {
        //swap the position of the candies
        int tempX = candyToSwap[0].xPos;
        int tempY = candyToSwap[0].yPos;

        candyToSwap[0].xPos = candyToSwap[1].xPos;
        candyToSwap[0].yPos = candyToSwap[1].yPos;

        candyToSwap[1].xPos = tempX;
        candyToSwap[1].yPos = tempY;

        //swap the game objects and update their position
        GameObject tempCandy = candyBoard[candyToSwap[0].xPos, candyToSwap[0].yPos].candy;
        candyBoard[candyToSwap[0].xPos, candyToSwap[0].yPos].candy = candyBoard[candyToSwap[1].xPos, candyToSwap[1].yPos].candy;
        candyBoard[candyToSwap[1].xPos, candyToSwap[1].yPos].candy = tempCandy;

        //update the position of the candies in the world
        Vector2 newPosCandy0 = new Vector2(candyToSwap[0].xPos - spacingX, candyToSwap[0].yPos - spacingY);
        Vector2 newPosCandy1 = new Vector2(candyToSwap[1].xPos - spacingX, candyToSwap[1].yPos - spacingY);

        //move the candies to their new positions
        candyToSwap[0].MoveToTarget(newPosCandy0);
        candyToSwap[1].MoveToTarget(newPosCandy1);
    }
    
    //coroutine that processes match
    private IEnumerator ProcessMatches()
    {
        yield return new WaitForSeconds(0.2f);
        
        //if a match has been found
        if(checkBoard())
        {
            //set the score combo to 5
            scoreCombo = 5;

            //starts a coroutine that is going to process our matches
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        //if no match have been found
        else
        {
            //play an indicative sound and decrement the number of moves
            myAudioSources[2].Play();
            nbMoves--;

            //swap back the candies
            swap();
        }
    }

    //check if the selection of candies are valid
    private bool CheckIfValidSelection()
    {
        //if only two candies are selected
        if(candyToSwap.Count == 2)
        {
            //check if the 2 candies are horizontal neighbours
            bool horizontalNeighbour = (candyToSwap[0].xPos == candyToSwap[1].xPos + 1 || candyToSwap[0].xPos == candyToSwap[1].xPos - 1) && candyToSwap[0].yPos == candyToSwap[1].yPos;
            
            //check if the 2 candies are vertical neighbours
            bool verticalNeighbour = (candyToSwap[0].yPos == candyToSwap[1].yPos + 1 || candyToSwap[0].yPos == candyToSwap[1].yPos - 1) && candyToSwap[0].xPos == candyToSwap[1].xPos;
           
            //check if the 2 candies are the same 
            bool sameCandy = candyToSwap[0].xPos == candyToSwap[1].xPos && candyToSwap[0].yPos == candyToSwap[1].yPos;

            //if the 2 candies are not verical or horizontal candies or if they are the same candy, they are invalid
            if (!horizontalNeighbour && !verticalNeighbour || sameCandy)
            {
                //clear the list of candies
                candyToSwap.Clear();

                //play wrong selection sound
                myAudioSources[2].Play();
               
                //decrement the number of candies
                nbMoves--;
                
                return false;
            }
            //the selection is valid
            else
            {
                return true;
            }
        }
        return false;
    }

    //method that initialize the board
    private void InitializeBoard()
    {
        //initialize a 2D array to create the board
        candyBoard = new Tile[width, height];

        //set the spacing of the tiles so that the candies are properly centered in the screen
        spacingX = ((float)(width - 1) / 2) - 2;
        spacingY = ((float)(height - 1) / 2) + 0.5f;

        //fill the board
        //goes through each row and fill them in the inner loop starting 
        //bottom left and finishing top right of the board
        for(int x = 0; x < height; x++)
        {
            for(int y = 0; y < width; y++)
            {
                //calculate the position of where the candy will be spawned
                Vector2 position = new Vector2(x - spacingX, y - spacingY);

                //generate a random candy index
                int randomIndex = Random.Range(0, candyPrefab.Length);

                //Instanciate a random candyPrefab at the position calculated
                GameObject candy = Instantiate(candyPrefab[randomIndex], position, Quaternion.identity);

                //set the correct position of the candy
                candy.GetComponent<Candy>().setPosition(x, y); //why do this?

                //create a new tile containing the random candy generated 
                candyBoard[x, y] = new Tile(candy); // why do this? 

                //list will have all the candies in the board generated
                candiesToDestroy.Add(candy);
            }
        }

        //check the board for matches
        if(checkBoard())
        {
            //destroy candies from previous board generated
            foreach(GameObject candy in candiesToDestroy)
            {
                Destroy(candy);
            }

            candiesToDestroy.Clear();    

            //generate a new board
            InitializeBoard();
            score = 0;
            counterBlue = 0;
            counterGreen = 0;
            counterPurple = 0;
            counterRed = 0;
            counterYellow = 0;
           
        }
    }
   
    //check the board to see if it contains matches
    private bool checkBoard()
    {
        //make sure the list is empty
        candiesToRemove.Clear();

        //true if we have at least one match 
        bool hasMatched = false;

        //make sure all the candies are set to not matched
        foreach(Tile candyTile in candyBoard)
        {
            if(candyTile.candy != null)
            {
                candyTile.candy.GetComponent<Candy>().isMatched = false;
            }
        }

        //Loop through each tiles 
        for(int x = 0; x < height; x++)
        {
            for(int y = 0; y < width; y++)
            {
                //get the candy in the tile
                Candy currentCandy = candyBoard[x,y].candy.GetComponent<Candy>();

                //make sure the candy is not matched already
                if(!currentCandy.isMatched)
                {
                    //get the match result
                    MatchResult matchedCandies = IsConnected(currentCandy);

                    //if we have long enough matches
                    if(matchedCandies.connectedCandies.Count >= 3)
                    {
                        //get the match result
                        MatchResult superMatchCandy = SuperMatch(matchedCandies);
                        
                        //add the candies to the list of candies to remove
                        candiesToRemove.AddRange(superMatchCandy.connectedCandies);

                        //set the candies of the match result to matched
                        foreach(Candy candy in superMatchCandy.connectedCandies)
                        {
                            candy.isMatched = true;
                        }
                        
                        hasMatched = true;
                    }
                }
            }
        }

        return hasMatched;
    }

    //called when matches are found in a board
    private IEnumerator ProcessTurnOnMatchedBoard(bool substractMove)
    {
        //play a sound because there is a match
        myAudioSources[3].Play();

        //set the candies to remove to not matched
        foreach (Candy candy in candiesToRemove)
        {
            candy.isMatched = false;
        }

        //remove and refill the spaces of the candies
        RemoveAndRefill(candiesToRemove);
        if(substractMove)
        {
            //substract a move and increment the score
            nbMoves--;
            score += 5;
        }

        yield return new WaitForSeconds(0.4f);

        //if theres is new created matches
        if(checkBoard())
        {
            //keep on processing the matches (but do not decrement the nb of moves)
            StartCoroutine(ProcessTurnOnMatchedBoard(false));

            //use a 2 multiplication factor when link trigger
            scoreCombo += 2 * scoreCombo;
            score += scoreCombo;
        }
    }

    //update the color objective counters
    private void UpdateColorCounter(Candy myCandy)
    {
        if(myCandy.candyType == CandyType.Blue)
        {
            counterBlue++;
        }
        else if(myCandy.candyType == CandyType.Red)
        {
            counterRed++;
        }
        else if(myCandy.candyType == CandyType.Green)
        {
            counterGreen++;
        }
        else if(myCandy.candyType == CandyType.Yellow)
        {
            counterYellow++;
        }
        else if(myCandy.candyType == CandyType.Purple)
        {
            counterPurple++;
        }
    }

    //function that tells us if the candy we are currently looking at is connected to other candies
    private MatchResult IsConnected(Candy myCandy)
    {
        //list that will contains the connected candies
        List<Candy> connectedCandies = new();

        //get the type of the candy passed
        CandyType myCandyType = myCandy.candyType;

        connectedCandies.Add(myCandy);

        //check Right 
        CheckDirection(myCandy, new Vector2Int(1, 0), connectedCandies);

        //check left
        CheckDirection(myCandy, new Vector2Int(-1, 0), connectedCandies);

        //3 match? (horizontal match)
        if(connectedCandies.Count == 3)
        {
            //Debug.Log("Normal horizontal match. Color: " + connectedCandies[0].candyType);
            UpdateColorCounter(connectedCandies[0]);

            //return the match found
            return new MatchResult(connectedCandies, MatchDirection.Horizontal);
        }

        //more than 3 match? (long Horizontal match)
        else if (connectedCandies.Count > 3)
        {
            //Debug.Log("Long horizontal match. Color: " + connectedCandies[0].candyType);
            UpdateColorCounter(connectedCandies[0]);

            //return the match found
            return new MatchResult(connectedCandies, MatchDirection.LongHorizontal);
        }

        //clear out the list of connected candies et add back our initial candy
        connectedCandies.Clear();
        connectedCandies.Add(myCandy);

        //check up
        CheckDirection(myCandy, new Vector2Int(0, 1), connectedCandies);

        //check down
        CheckDirection(myCandy, new Vector2Int(0, -1), connectedCandies);

        //3 match? (vertical match)
        if (connectedCandies.Count == 3)
        {
            //Debug.Log("Normal vertical match. Color: " + connectedCandies[0].candyType);
            UpdateColorCounter(connectedCandies[0]);

            //return the match found
            return new MatchResult(connectedCandies, MatchDirection.Vertical);

        }

        //more than 3 match? (long vertical match)
        else if (connectedCandies.Count > 3)
        {
            //Debug.Log("Long vertical match. Color: " + connectedCandies[0].candyType);
            UpdateColorCounter(connectedCandies[0]);

            //return the match found
            return new MatchResult(connectedCandies, MatchDirection.LongVertical);
        }
        
        //if no match were found
        else
        {
            return new MatchResult(connectedCandies, MatchDirection.None);
        }
    }

    //method that either checks vertically or horizontally for matches and update the connected candies list (list passed by reference)
    private void CheckDirection(Candy aCandy, Vector2Int direction, List<Candy> connectedCandies)
    {
        CandyType myCandyType = aCandy.candyType;

        //x and y of the neighbouring candy
        int x = aCandy.xPos + direction.x;
        int y = aCandy.yPos + direction.y;

        //check that we are in the boundaries of the board
        while(x >= 0 && x < width && y >=0 && y < height)
        {
            Candy neighbourCandy = candyBoard[x, y].candy.GetComponent<Candy>();

            //check if it is matching our candy passed in parameter and is not already matched
            if(!neighbourCandy.isMatched && neighbourCandy.candyType == aCandy.candyType)
            {
                //add that candy to the list of connected candies
                connectedCandies.Add(neighbourCandy);

                //increment x and y to keep looking for matches to add to the list
                x += direction.x;
                y += direction.y;
            }
            else
            {
                break;
            }
        }
    }

    //method that checks for super matches
    private MatchResult SuperMatch(MatchResult myMatchResult)
    {
        //looking at horizontal matches
        if(myMatchResult.direction == MatchDirection.Horizontal || myMatchResult.direction == MatchDirection.LongHorizontal)
        {
            foreach(Candy aCandy in myMatchResult.connectedCandies)
            {
                List<Candy> extraConnectedCandies = new();

                //check if there is extra connected candies
                CheckDirection(aCandy, new Vector2Int(0, 1), extraConnectedCandies);
                CheckDirection(aCandy, new Vector2Int(0, -1), extraConnectedCandies);

                //if we have a supermatch
                if(extraConnectedCandies.Count >= 2)
                {
                    //Debug.Log("Super horizontal match");
                    //add an extra 5 points because it is a special match
                    score += 5;

                    //update the color counter (consider as another match)
                    UpdateColorCounter(extraConnectedCandies[0]);
                    extraConnectedCandies.AddRange(myMatchResult.connectedCandies);

                    MatchResult newSuperMatchResult = new MatchResult(extraConnectedCandies, MatchDirection.Super);

                    return newSuperMatchResult;
                }
            }
            return myMatchResult;
            
        }

        //looking at vertical matches
        else if (myMatchResult.direction == MatchDirection.Vertical || myMatchResult.direction == MatchDirection.LongVertical)
        {
            foreach (Candy aCandy in myMatchResult.connectedCandies)
            {
                List<Candy> extraConnectedCandies = new();

                //check if theres extra connected candies
                CheckDirection(aCandy, new Vector2Int(1, 0), extraConnectedCandies);
                CheckDirection(aCandy, new Vector2Int(-1, 0), extraConnectedCandies);

                //if we have a supermatch
                if (extraConnectedCandies.Count >= 2)
                {
                    //Debug.Log("Super Vertical match");
                    //add an extra 5 points becuase it is a special match
                    score += 5;

                    //update the color counter (consider as another match)
                    UpdateColorCounter(extraConnectedCandies[0]);
                    extraConnectedCandies.AddRange(myMatchResult.connectedCandies);

                    MatchResult newSuperMatchResult = new MatchResult(extraConnectedCandies, MatchDirection.Super);

                    return newSuperMatchResult;
                }
            }
            return myMatchResult;
        }
        return null;
    }
    
    //Method that removes candies and refill the space
    private void RemoveAndRefill(List<Candy> candiesToRemove)
    {
        //get the y of the candy to remove
        int yPos = candiesToRemove[0].yPos;

        //keep track if the candies to remove are horizontal
        bool isHorizontal = true;

        //removing the candy and clearing the board
        foreach (Candy aCandy in candiesToRemove)
        {
            int myX = aCandy.xPos;
            int myY = aCandy.yPos;

            //if the y of the candies to remove change, it is not a horizontal match
            if(yPos != myY)
            {
                isHorizontal = false;
            }

            //destroy the candy
            Destroy(aCandy.gameObject);

            //create a blank tile on the board
            candyBoard[myX, myY] = new Tile(null);
        }

        //clear the list of candies to remove
        candiesToRemove.Clear();

        //if we find an empty space on the board, refill it
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if (candyBoard[x, y].candy == null)
                {
                    RefillCandy(x, y, isHorizontal);

                    //increment the nb of candies refilled
                    nbCandiesRefilled++;
                }
            }
        }
        nbCandiesRefilled = 0;
    }

    //method that drops candy down and refills empty space
    private void RefillCandy(int myX, int myY, bool isHorizontal)
    {
        int yOffset = 1;

        //until we reach the top of the board or we hit the first non null candy above
        while(myY + yOffset < height && candyBoard[myX, myY + yOffset].candy == null)
        {
            yOffset++;
        }

        //we hit the top of the board or the candy above is not null. we move that candy down
        if(myY + yOffset < height && candyBoard[myX, myY + yOffset].candy != null)
        {
            //candy above
            Candy candyAbove = candyBoard[myX, myY + yOffset].candy.GetComponent<Candy>();

            //move to the correct location
            Vector3 targetPos = new Vector3(myX - spacingX, myY - spacingY, candyAbove.transform.position.z);

            candyAbove.MoveToTarget(targetPos);

            //update position
            candyAbove.setPosition(myX, myY);

            //update the board
            candyBoard[myX, myY] = candyBoard[myX, myY + yOffset];

            //set the location the candy came from to null
            candyBoard[myX, myY + yOffset] = new Tile(null);
        }

        //when there is no candies above
        if(myY + yOffset == height)
        {
            //logic to generate candies for level 1
            if(level == 1)
            {
                SpawnCandyAtTopLvl1(myX, isHorizontal);
            }
            //logic to generate candies for level 2
            else if(level == 2)
            {
                SpawnCandyAtTopLvl2(myX);
            }
            else
            {
                Debug.Log("Unrecognized level, cannot process matches");
            }
            
        }
    }

    //spawning logic for level 1
    private void SpawnCandyAtTopLvl1(int xPos, bool isHorizontal)
    {
        //get the lowest empty tile
        int index = FindIndexOfLowestNull(xPos);

        //where to move the spawned candy
        int locationToMoveTo = height - index;

        //new candy being spaned
        GameObject newCandy;

        //look for the candy under
        Candy candyUnder;

        //if the candy we want to spawn have a candy underneath
        if(index - 1 >= 0)
        {
            //get the candy underneath
            candyUnder = candyBoard[xPos, index - 1].candy.GetComponent<Candy>();

            //get the color of the candy under
            int colorNb = ColorNumber(candyUnder);

            //if we are refilling the first candy of a vertical match
            if (nbCandiesRefilled == 0 && isHorizontal == false)
            {
                //40% change of matching the tile below
                int randomIndex = Random.Range(0, 100);

                //spawn a candy of the same color of the underneath one
                if (randomIndex < 40)
                {
                    newCandy = Instantiate(candyPrefab[colorNb], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
                }
                //spawn a candy that is NOT of the same color of the candy underneath
                else
                {
                    int newRandom;
                    do
                    {
                        newRandom = Random.Range(0, candyPrefab.Length);
                    }
                    //find another random color if we get the same as the candy underneath
                    while (newRandom == colorNb);

                    //instantiate the new candy
                    newCandy = Instantiate(candyPrefab[newRandom], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
                }
            }
            //if we are refilling subsequent candies OR just horizontal candies
            else if (isHorizontal || nbCandiesRefilled > 0)
            {
                //60%chance of matching the candy bellow
                int randomIndex = Random.Range(0, 100);

                //spawn a candy the same color as the candy underneath
                if (randomIndex < 60)
                {
                    newCandy = Instantiate(candyPrefab[colorNb], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
                }
                //spawn a candy of a different color
                else
                {
                    int newRandom;
                    do
                    {
                        newRandom = Random.Range(0, candyPrefab.Length);
                    }
                    //find another random color if we get the same as the candy underneath
                    while (newRandom == colorNb);

                    newCandy = Instantiate(candyPrefab[newRandom], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
                }
            }
            else
            {
                newCandy = null;
            }
        }
        //if there is no candy underneath (we are placing the first candy)
        else
        {
            //20% change of being any candy
            int randomIndex = Random.Range(0, candyPrefab.Length);
            newCandy = Instantiate(candyPrefab[randomIndex], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
        }

        //set the position of the new candy
        newCandy.GetComponent<Candy>().setPosition(xPos, index);

        //put the candy in the tile
        candyBoard[xPos, index] = new Tile(newCandy);

        //move the candy to its final position
        Vector3 targetPos = new Vector3(newCandy.transform.position.x, newCandy.transform.position.y - locationToMoveTo, newCandy.transform.position.z);
        newCandy.GetComponent<Candy>().MoveToTarget(targetPos);
    }

    //spawning logic for level 2
    private void SpawnCandyAtTopLvl2(int xPos)
    {
        //find the lowest null y position
        int index = FindIndexOfLowestNull(xPos);

        //get the location we want to move the candt to
        int locationToMoveTo = height - index;

        //surrounding candies
        Candy top = null, bottom = null, right = null, left = null, topRight = null, topLeft = null, bottomRight = null, bottomLeft = null;

        //surrounding counters
        int blue = 0, red = 0, green = 0, yellow = 0, purple = 0;

        //check the right side
        if (xPos + 1 < width)
        {
            //bottom right
            if(index - 1 >= 0 && candyBoard[xPos + 1, index - 1] != null && candyBoard[xPos + 1, index - 1].candy != null)
            {
                bottomRight = candyBoard[xPos + 1, index - 1].candy.GetComponent<Candy>();
            }
            else
            {
                bottomRight = null;
            }
            //top right
            if(index + 1 < height && candyBoard[xPos + 1, index + 1]!= null && candyBoard[xPos + 1, index + 1].candy != null)
            {
                topRight = candyBoard[xPos + 1, index + 1].candy.GetComponent<Candy>();
            }
            else
            {
                topRight = null;
            }
            //right
            if (candyBoard[xPos + 1, index] != null && candyBoard[xPos + 1, index].candy != null)
            {
                right = candyBoard[xPos + 1, index].candy.GetComponent<Candy>();
            }
            else
            {
                right = null;
            }
        }

        //check the left side
        if (xPos - 1 >= 0)
        {
            //bottom left
            if (index - 1 >= 0 && candyBoard[xPos - 1, index - 1]!= null && candyBoard[xPos - 1, index - 1].candy != null)
            {
                bottomLeft = candyBoard[xPos - 1, index - 1].candy.GetComponent<Candy>();
            }
            else
            {
                bottomLeft = null;
            }
            //top left
            if (index + 1 < height && candyBoard[xPos - 1, index + 1] != null && candyBoard[xPos - 1, index + 1].candy != null)
            {
                topLeft = candyBoard[xPos - 1, index + 1].candy.GetComponent<Candy>();
            }
            else
            {
                topLeft = null;
            }
            //left
            if(candyBoard[xPos - 1, index] != null && candyBoard[xPos - 1, index].candy !=null)
            {
                left = candyBoard[xPos - 1, index].candy.GetComponent<Candy>();
            }
            else
            {
                left = null;
            }
        }

        //check bottom
        if(index-1 >=0)
        {
            bottom = candyBoard[xPos, index-1].candy.GetComponent<Candy>();
        }
        else
        {
            bottom = null;
        }

        //check top
        if(index + 1 < height)
        {
            top = null;
        }
        else
        {
            top = null;
        }

        //update the counters of the surrounding candies (passing the counters by reference)
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref purple, top);
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref purple, bottom);
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref purple, topRight);
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref purple, topLeft);
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref purple, bottomRight);
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref purple, bottomLeft);
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref  purple, right);
        surroundingColor(ref blue, ref red, ref yellow, ref green, ref purple, left);

        //probabilities weight
        float weightRed, weightGreen, weightBlue, weightYellow, weightPurple;

        //total probability weight
        float totalWeight;

        //probabilities
        float redProb, greenProb, blueProb, yellowProb, purpleProb;

        //spawned candy
        GameObject newCandy = null;

        //calculate the weight of each probabilities
        weightRed = red + 1;
        weightGreen = green + 1;
        weightBlue = blue + 1;
        weightYellow = yellow + 1;
        weightPurple = purple + 1;
        totalWeight = weightRed + weightGreen + weightYellow + weightPurple + weightBlue;

        //calculate the probabilities
        redProb = 100 / totalWeight * weightRed;
        greenProb = 100 / totalWeight * weightGreen;
        blueProb = 100 / totalWeight * weightBlue;
        yellowProb = 100 / totalWeight * weightYellow;
        purpleProb = 100 / totalWeight * weightPurple;

        //generate a random float between 0 and 100
        float randomVal = Random.Range(0.0f, 100.0f);

        //spawning the right candy depending on the probabilities
        
        //red
        if(0<=randomVal && randomVal< redProb)
        {
            newCandy = Instantiate(candyPrefab[3], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
        }
        //blue
        if(redProb <= randomVal && randomVal < redProb + blueProb)
        {
            newCandy = Instantiate(candyPrefab[0], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
        }
        //green
        if (redProb + blueProb <= randomVal && randomVal < redProb + blueProb + greenProb)
        {
            newCandy = Instantiate(candyPrefab[1], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
        }
        //yellow
        if (redProb + blueProb + greenProb <= randomVal && randomVal < redProb + blueProb + greenProb + yellowProb)
        {
            newCandy = Instantiate(candyPrefab[4], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
        }
        //purple
        if (redProb  + blueProb + greenProb + yellowProb <= randomVal && randomVal < redProb + blueProb + greenProb + yellowProb + purpleProb)
        {
            newCandy = Instantiate(candyPrefab[2], new Vector2(xPos - spacingX, height - spacingY), Quaternion.identity);
        }

        //Debug.Log("SURROUNDING: Blue: " + blue + " /Red: " + red + " /Yellow: " + yellow + " /Green: " + green + " /Purple: " + purple);
        //Debug.Log("PROB: Blue: " + blueProb + " /Red: " + redProb + " /Yellow: " + yellowProb + " /Green: " + greenProb + " /Purple: " + purpleProb);
        
        //set the position of the new candy
        newCandy.GetComponent<Candy>().setPosition(xPos, index);
        candyBoard[xPos, index] = new Tile(newCandy);

        //move the candy to the correct position
        Vector3 targetPos = new Vector3(newCandy.transform.position.x, newCandy.transform.position.y - locationToMoveTo, newCandy.transform.position.z);
        newCandy.GetComponent<Candy>().MoveToTarget(targetPos);
    }

    //find the y of the lowest empty tile
    private int FindIndexOfLowestNull(int xPos)
    {
        int lowestNull = 99;
        for(int y = 7; y >=0; y--)
        {
            if (candyBoard[xPos, y].candy == null)
            {
                lowestNull = y; 
            }
        }
        return lowestNull;
    }

    //update the counter of the surrounding colors (portrait of the surrounding colors)
    public void surroundingColor(ref int blue, ref int red, ref int yellow, ref int green, ref int purple, Candy myCandy)
    {
        //usede to generate the color if the candy passed is null
        int randomCandy = -1;

        //dealing with null candies
        if(myCandy == null)
        {
            //generate a random candy color
             randomCandy = Random.Range(0, candyPrefab.Length);

            if(randomCandy == 0)
            {
                blue++;
            }
            if(randomCandy == 3)
            {
                red++;
            }
            if(randomCandy == 1)
            {
                green++;
            }
            if(randomCandy == 4)
            {
                yellow++;
            }
            if(randomCandy == 2)
            {
                purple++;
            }
        }
        //upgrade the right counter
        else if (myCandy.candyType == CandyType.Blue)
        {
            blue++;
        }
        else if (myCandy.candyType == CandyType.Red)
        {
            red++;
        }
        else if (myCandy.candyType == CandyType.Green)
        {
            green++;
        }
        else if (myCandy.candyType == CandyType.Yellow)
        {
            yellow++;
        }
        else if (myCandy.candyType == CandyType.Purple)
        {
            purple++;
        }
    }
    
    //method that return the prefab number of a candy object
    public int ColorNumber(Candy myCandy)
    {
        if (myCandy.candyType == CandyType.Blue)
        {
            return 0;
        }
        else if (myCandy.candyType == CandyType.Red)
        {
            return 3;
        }
        else if (myCandy.candyType == CandyType.Green)
        {
            return 1;
        }
        else if (myCandy.candyType == CandyType.Yellow)
        {
            return 4;
        }
        else if (myCandy.candyType == CandyType.Purple)
        {
            return 2;
        }

        return 10;
    }
}



//class used to organize match results
public class MatchResult
{
    //candies that are connected 
    public List<Candy> connectedCandies;

    //direction of the matches
    public MatchDirection direction;

    //constructor
    public MatchResult(List<Candy> connectedCandies, MatchDirection direction)
    {
        this.connectedCandies = connectedCandies;
        this.direction = direction;
    }
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
