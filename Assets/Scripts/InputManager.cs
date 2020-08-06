using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection
{
    Left, Right, Up, Down
}

public class InputManager : MonoBehaviour
{

    private GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(gameManager.DoMovement(MoveDirection.Left));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(gameManager.DoMovement(MoveDirection.Right));
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(gameManager.DoMovement(MoveDirection.Up));
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCoroutine(gameManager.DoMovement(MoveDirection.Down));
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(gameManager.Restart());
        }
    }
}
