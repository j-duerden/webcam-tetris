using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrimino : MonoBehaviour
{
    float fallTimer = 0;                                // timer before piece falls automatically
    public float fallSpeed = 1;                         // default falling speed (in seconds)

    public bool allowRotation;                          // whether a piece can rotate
    public bool limitRotation;                          // whether a piece only has two rotation states

    public int individualScore = 100;                   // a piece's individual score for placement
    private float individualScoreTime;                  // the time the piece has been active

    private float continuousVerticalSpeed = 0.05f;      // movement speed for piece whilst down input held
    private float continuousHorizontalSpeed = 0.1f;     // movement speed for piece whilst left/right input held
    private float buttonDownDelayMax = 0.2f;            
    private float buttonDownDelayTimer = 0;

    private bool moveNowHorizontal = false;
    private bool moveNowVertical = false;

    private float verticalTimer = 0;
    private float horizontalTimer = 0;

    public AudioClip moveSound;
    public AudioClip rotateSound;
    public AudioClip landSound;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForUserInput();
        UpdateIndividualScore();
    }

    // update the score over time for placing the piece
    void UpdateIndividualScore()
    {
        if(individualScoreTime < 1)
        {
            individualScoreTime += Time.deltaTime;
        }
        else
        {
            individualScoreTime = 0;
            individualScore = Mathf.Max(individualScore - 10, 0);
        }
    }

    // check for button presses by the player
    void CheckForUserInput()
    {
        // reset the timers when a key is released
        if(Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            moveNowHorizontal = false;
            moveNowVertical = false;
            horizontalTimer = 0;
            verticalTimer = 0;
            buttonDownDelayTimer = 0;
        }

        if(Input.GetKey(KeyCode.RightArrow))
        {
            if(moveNowHorizontal)
            {
                if (buttonDownDelayTimer < buttonDownDelayMax)
                {
                    buttonDownDelayTimer += Time.deltaTime;
                    return;
                }
                if (horizontalTimer < continuousHorizontalSpeed)
                {
                    horizontalTimer += Time.deltaTime;
                    return;
                }
            }

            if(!moveNowHorizontal)
            {
                moveNowHorizontal = true;
            }

            horizontalTimer = 0;

            transform.position += new Vector3(1, 0, 0);
            if(CheckIsValidPosition())
            {
                PlayGivenAudio(moveSound);
                FindObjectOfType<Game>().UpdateGrid(this);
            }
            else
            {
                transform.position += new Vector3(-1, 0, 0);
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (moveNowHorizontal)
            {
                if (buttonDownDelayTimer < buttonDownDelayMax)
                {
                    buttonDownDelayTimer += Time.deltaTime;
                    return;
                }
                if (horizontalTimer < continuousHorizontalSpeed)
                {
                    horizontalTimer += Time.deltaTime;
                    return;
                }
            }

            if (!moveNowHorizontal)
            {
                moveNowHorizontal = true;
            }
            horizontalTimer = 0;

            transform.position += new Vector3(-1, 0, 0);
            if (CheckIsValidPosition())
            {
                PlayGivenAudio(moveSound);
                FindObjectOfType<Game>().UpdateGrid(this);
            }
            else
            {
                transform.position += new Vector3(1, 0, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(allowRotation)
            { 
                transform.Rotate(0, 0, 90);
                if(CheckIsValidPosition())
                {
                    PlayGivenAudio(rotateSound);
                    FindObjectOfType<Game>().UpdateGrid(this);
                }
                else
                {
                    if(limitRotation)
                    {
                        if(transform.rotation.eulerAngles.z >= 90)
                        {
                            transform.Rotate(0, 0, -90);
                        }
                        else
                        {
                            transform.Rotate(0, 0, 90);
                        }
                    }
                    else
                    {
                        transform.Rotate(0, 0, -90);
                    }
                }
                
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Time.time - fallTimer >= fallSpeed)
        {
            if(moveNowVertical)
            {
                if (buttonDownDelayTimer < buttonDownDelayMax)
                {
                    buttonDownDelayTimer += Time.deltaTime;
                    return;
                }
                if (verticalTimer < continuousVerticalSpeed)
                {
                    verticalTimer += Time.deltaTime;
                    return;
                }
            }
            if(!moveNowVertical)
            {
                moveNowVertical = true;
            }

            verticalTimer = 0;

            transform.position += new Vector3(0, -1, 0);
            if (CheckIsValidPosition())
            {
                FindObjectOfType<Game>().UpdateGrid(this);
                if(Input.GetKeyDown(KeyCode.DownArrow))
                {
                    PlayGivenAudio(moveSound);
                }
            }
            else
            {
                transform.position += new Vector3(0, 1, 0);
                FindObjectOfType<Game>().DeleteRow();
                if(FindObjectOfType<Game>().CheckOutOfBounds(this))
                {
                    FindObjectOfType<Game>().GameOver();
                }
                enabled = false;
                PlayGivenAudio(landSound);
                FindObjectOfType<Game>().SpawnNextTetrimino();
                Game.currentScore += individualScore;
            }
            fallTimer = Time.time;
        }
    }

    // plays specified audio clip once
    void PlayGivenAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    // checks whether each individual cube is moving to a valid position
    bool CheckIsValidPosition()
    {
        foreach(Transform block in transform)
        {
            Vector3 position = FindObjectOfType<Game>().RoundValues(block.position);
            if(FindObjectOfType<Game>().CheckIsInsideGrid(position)==false)
            {
                return false;
            }
            if(FindObjectOfType<Game>().GetTransformAtGridPosition(position) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(position).parent != transform)
            {
                return false;
            }
        }

        return true;
    }
}
