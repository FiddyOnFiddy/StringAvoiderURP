using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{

    private static GameManagerScript _instance;
    public static GameManagerScript Instance { get { return _instance; } }

    //Declaring new enum of type GameState to handle what state we're currently in
    public enum GameState
    {
        Idle,               //Substate we sit in on main menu to see which button the user selects
        Setup,              //Initiliase everything that needs to be initiliased and set up game state for new level or respawn.
        Playing,            //For when the player is alive and handle the game loop
        InitialiseDeath,    //Initiliase the death by setting up the animation
        Dead,               //Run in fixed update for consistent animation
        GameOver,           //For when the player has died and death animation has finished
        NextLevelMenu,      //For when the player reaches the end point and triggers the next level screen with medal
    }

    [Header("Global References:")]
    //List of references all scripts and this script require. Store all references in Game Manager so there is a centralised location for references and all scripts pass through Game Manager for access.
    [SerializeField] private GameState currentState;                                    //Tracks which state we are currently in which is passed to a switch statement for processing.
    [SerializeField] private StringMovement sM;                                         //Holds reference to the string and all it's child components.
    [SerializeField] private Canvas endLevelCanvas, mainMenuCanvas, levelCanvas;        //Reference to all UI Canvas for various Game States.
    [SerializeField] private List<Material> dissolveMaterials;                          //Reference to materials attached to each string point allowing us to access dissolve script plus material per string point.

    [Space(5)]
    [Header("Boolean States:")]
    [SerializeField] private bool moveRigidBodies;                                      //Controls when to update physics ensuring it's after input + render update.
    [SerializeField] private bool dissolveDone;                                         //Determines when all string points have finished animation to then transition to next state.
    [SerializeField] private bool triggerNextLevelMenu;                                 //Used to determine if we collided with the end trigger instead of a wall.
    [SerializeField] private bool initString;                                           //Used to initalise string upon first level load from main menu be it: New Game; Continue or Level Select. As we only spawn string upon game load otherwise we are resetting position+variables and not creating a new set of string points.

    [Space(5)]
    [Header("Numerical Values:")]
    [SerializeField] private int stringPointIntersectedWith;                            //Lets us know which string point we have collided with to play the animation from that point looping outwards in either direction.
    [SerializeField] private  int count2ndHalf, count1stHalf;                           //Count variables that represent "i" in our if statement for looping through in each direction. Reason for count variables is we are using if statement and not for loop.
    [SerializeField] private int deathCount;                                            //Death count variable that tracks total death count and gets written to and read from file.
    [SerializeField] private float levelTime = 0f;                                      //Time to complete level which is passed to the level text and which will be saved in file representing best time per level. To be added to total time variable for time across all levels.
    [SerializeField] private float dissolveSpeed;                                       //Determines how fast dissolve animation will be.


    [Space(8)]
    [Header("Medal Time Splits:")]
    [SerializeField] private List<TimeThreshold> medalSplits = new List<TimeThreshold>();       //List of custom struct TimeThreshold which holds the medal time splits for each level.


    //All the expression body properties for getting and setting any relevant data and access outside of Game Manager.
    public GameState CurrentState { get => currentState; set => currentState = value; }
    public StringMovement SM { get => sM; set => sM = value; }
    public bool MoveRigidBodies {  get => moveRigidBodies; set => moveRigidBodies = value;  }
    public bool TriggerNextLevelMenu { get => triggerNextLevelMenu; set => triggerNextLevelMenu = value; }
    public bool InitString { get => initString; set => initString = value; }
    public int Count2ndHalf { get => count2ndHalf; set => count2ndHalf = value;  }
    public int Count1stHalf { get => count1stHalf; set => count1stHalf = value;  }
    public int StringPointIntersectedWith { get => stringPointIntersectedWith; set => stringPointIntersectedWith = value; }
    public float LevelTime { get => levelTime; set => levelTime = value; }
    public float DissolveSpeed { get => dissolveSpeed; set => dissolveSpeed = value; }


    void Awake()
    {
        //Checks to see if any Game Managers are present in the scene and either delete or assign this script to them. Necessary for singleton pattern.
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        sM = FindObjectOfType<StringMovement>();

        currentState = GameState.Idle;

    }
    
    void Update()
    {
        //Switch statement which takes our current state and decides what to do based on that state.
        switch (currentState)
        {
            case GameState.Setup:
                //SetUp();
                break;
            case GameState.Playing:
                break;
            case GameState.InitialiseDeath:
                //InitialiseDeath();
                break;
            case GameState.Dead:
                //See Fixed Update
                break;
            case GameState.GameOver:
                //ResetString();
                break;
            case GameState.NextLevelMenu:
                break;
        }
    }

    private void FixedUpdate()
    {
        //Checks for Dead state to play the death animation. Placed in fixed update for consistent frame rate.
        if(currentState == GameState.Dead)
        {
            //DeathAnimation();
        }
    }

    void SetUp()
    {
        //Initialise the level and all relevant variables.
        //What we do depends on if player hit Play/Continue or Level Select or Speedrun mode. Will be boolean check which decides what to do for each use case.
        mainMenuCanvas.enabled = false;
        levelCanvas.enabled = true;


        if(initString)
        {
            sM.InitialiseString();
            initString = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            currentState = GameState.Playing;
        }
    }

    void InitialiseDeath()
    {
        dissolveMaterials.Clear();
        count1stHalf = 0;
        count2ndHalf = stringPointIntersectedWith;

        //Cache the dissolve material in a variable to then loop through and check that the dissovle animation has finished so that we can transition to gameover state.
        for (int i = 0; i < sM.StringPointsGO.Count; i++)
        {
            dissolveMaterials.Add(sM.StringPointsGO[i].GetComponent<SpriteRenderer>().material);
        }

        currentState = GameState.Dead;
    }

    void DeathAnimation()
    {
        //Loop 1st Half of string if intersected
        if (count1stHalf < stringPointIntersectedWith)
        {
            sM.StringPointsGO[(stringPointIntersectedWith - 1) - count1stHalf].GetComponent<Dissolve>().startDissolve = true;

            count1stHalf++;
        }

        //Loop 2nd half of string if intersected
        if (count2ndHalf < sM.StringPointsGO.Count)
        {
            sM.StringPointsGO[count2ndHalf].GetComponent<Dissolve>().startDissolve = true;


            count2ndHalf++;
        }


        //Check if each elements dissolve amount has reached 1 meaning animatoin is done and set dissolveDone to true. if not all are done we set to false until the final one has changed
        dissolveDone = false;
        foreach (Material material in dissolveMaterials)
        {
            if (material.GetFloat("_DissolveAmount") == 1)
            {
                dissolveDone = true;
            }
            else
            {
                dissolveDone = false;
                break;
            }
        }

        if(dissolveDone)
        {

            if(triggerNextLevelMenu)
            {
                currentState = GameState.NextLevelMenu;
            }
            else
            {
                currentState = GameState.GameOver;
            }
        }
    }

    public void ResetString()
    {
        sM.ResetString();
        dissolveDone = false;

        foreach (Material material in dissolveMaterials)
        {
            material.SetFloat("_DissolveAmount", 0);
        }

        for (int i = 0; i < sM.StringPointsGO.Count; i++)
        {
            sM.StringPointsGO[i].GetComponent<Dissolve>().startDissolve = false;
            sM.StringPointsGO[i].GetComponent<Dissolve>().DissolveAmount = 0;

        }

        count1stHalf = 0;
        count2ndHalf = 0;
        stringPointIntersectedWith = 0;

        currentState = GameState.Setup;
    }


    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);

        //We write our ingame variables to this data object when the game closes that then gets written to a file to be loaded next session.
        PlayerData data = new PlayerData();

        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if(File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/playerInfo.dat");
            PlayerData data = (PlayerData)bf.Deserialize(file);

            file.Close();

            //On load of game/main menu load all data from file into game and store in variable
        }
    }


}


/// <summary>
/// All data that is to be written to file for saving persistent data between sessions.
/// </summary>
[Serializable]
class PlayerData
{
    public int deathCount;
}

/// <summary>
/// Medal time splits for each level. Level is the level we're on, rest are splits for that level.
/// </summary>
[Serializable]
public struct TimeThreshold
{
    public int Level;


    public int Bronze;
    public int Silver;
    public int Gold;

    public TimeThreshold(int level, int bronze, int silver, int gold)
    {
        this.Level = level;

        this.Bronze = bronze;
        this.Silver = silver;
        this.Gold = gold;
    }
}


