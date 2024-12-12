using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Tiles are what makes our board. They contains a candy game object
public class Tile : MonoBehaviour
{
    public GameObject candy;

    public Tile(GameObject myCandy)
    {
        candy = myCandy;
    }
        
}
