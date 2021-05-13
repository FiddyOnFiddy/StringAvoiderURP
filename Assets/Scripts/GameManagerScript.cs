using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;


public class GameManagerScript : MonoBehaviour
{

    private static GameManagerScript _instance;
    public static GameManagerScript Instance { get { return _instance; } }

    //Declaring new enum of type GameState to handle what state we're currently in
    public enum GameState
    {
        Idle,               //Sub-state we sit in on main menu to see which button the user selects
        Setup,              //Initiliase everything that needs to be initiliased and set up game state for new level or respawn.
        Playing,            //For when the player is alive and handle the game loop
        InitialiseDeath,    //Initiliase the death by setting up the animation
        Dead,               //Run in fixed update for consistent animation
        GameOver,           //For when the player has died and death animation has finished
        NextLevelMenu,      //For when the player reaches the end point and triggers the next level screen with medal
    }

    [Header("Global References:")]
    //List of references all scripts and this script require. Store all references in Game Manager so there is a centralised location for references and all scripts pass through Game Manager for access.
    [SerializeField] private GameState currentState;                                                        //Tracks which state we are currently in which is passed to a switch statement for processing.
    [SerializeField] private StringMovement sM;                                                             //Holds reference to the string and all it's child components.
    [SerializeField] public Canvas mainMenuCanvas, levelSelectCanvas, gameCanvas, endScreenCanvas;          //Reference to all UI Canvas for various Game States.
    [SerializeField] private List<Material> dissolveMaterials;                                              //Reference to materials attached to each string point allowing us to access dissolve script plus material per string point.

    [Space(5)]
    [Header("Boolean States:")]
    [SerializeField] private bool moveRigidBodies;                                                          //Controls when to update physics ensuring it's after input + render update.
    [SerializeField] private bool dissolveDone;                                                             //Determines when all string points have finished animation to then transition to next state.
    [SerializeField] private bool triggerNextLevelMenu;                                                     //Used to determine if we collided with the end trigger instead of a wall.
    [SerializeField] private bool initString;                                                               //Used to initalise string upon first level load from main menu be it: New Game; Continue or Level Select. As we only spawn string upon game load otherwise we are resetting position+variables and not creating a new set of string points.

    [Space(5)]
    [Header("String Collision Info:")]
    [SerializeField] private int stringPointIntersectedWith;                                                //Lets us know which string point we have collided with to play the animation from that point looping outwards in either direction.
    [SerializeField] private int count2ndHalf, count1stHalf;                                               //Count variables that represent "i" in our if statement for looping through in each direction. Reason for count variables is we are using if statement and not for loop.

    [Space(5)]
    [Header("Persistant Data:")]

    [SerializeField] private int deathCount;                                                                //Death count variable that tracks total death count and gets written to and read from file.
    [Min(1)]
    [SerializeField] public int currentLevel = 1;
    [SerializeField] private float levelTime = 0f;                                                          //Time to complete level which is passed to the level text and which will be saved in file representing best time per level. To be added to total time variable for time across all levels.

    [Space(5)]
    [Header("Misc:")]
    [SerializeField] private float dissolveSpeed;                                                           //Determines how fast dissolve animation will be.

    [Space(8)]
    [Header("Medal Time Splits:")]
    public List<Vector2> medalSplits = new List<Vector2>();


    public Dictionary<int, Vector2> medalSplitsDict = new Dictionary<int, Vector2>();
    public Dictionary<int, bool> isLevelComplete = new Dictionary<int, bool>();
    public int maxLevelCount;

    public int animationSpeed;
    AsyncOperation asyncLoad;


    //All the expression body properties for getting and setting any relevant data and access outside of Game Manager.
    public GameState CurrentState { get => currentState; set => currentState = value; }
    public StringMovement SM { get => sM; set => sM = value; }
    public bool MoveRigidBodies { get => moveRigidBodies; set => moveRigidBodies = value; }
    public bool TriggerNextLevelMenu { get => triggerNextLevelMenu; set => triggerNextLevelMenu = value; }
    public bool InitString { get => initString; set => initString = value; }
    public int Count2ndHalf { get => count2ndHalf; set => count2ndHalf = value; }
    public int Count1stHalf { get => count1stHalf; set => count1stHalf = value; }
    public int StringPointIntersectedWith { get => stringPointIntersectedWith; set => stringPointIntersectedWith = value; }
    public float LevelTime { get => levelTime; set => levelTime = value; }
    public float DissolveSpeed { get => dissolveSpeed; set => dissolveSpeed = value; }
    public int DeathCount { get => deathCount; set => deathCount = value; }


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

        for (int i = 0; i < medalSplits.Count; i++)
        {
            medalSplitsDict[i + 1] = medalSplits[i];
        }

        mainMenuCanvas.enabled = true;
        gameCanvas.enabled = false;
        levelSelectCanvas.enabled = false;
        endScreenCanvas.enabled = false;

        Load();

        sM = FindObjectOfType<StringMovement>();

        currentState = GameState.Idle;
    }

    void Update()
    {
        //Switch statement which takes our current state and decides what to do based on that state.
        switch (currentState)
        {
            case GameState.Setup:
                SetUp();
                break;
            case GameState.Playing:
                break;
            case GameState.InitialiseDeath:
                InitialiseDeath();
                break;
            case GameState.Dead:
                //See Fixed Update
                break;
            case GameState.GameOver:
                ReloadLevel();
                break;
            case GameState.NextLevelMenu:
                NextLevelScreen();
                break;
        }
    }

    private void FixedUpdate()
    {
        //Checks for Dead state to play the death animation. Placed in fixed update for consistent frame rate.
        if (currentState == GameState.Dead)
        {
            DeathAnimation();
        }
    }

    void SetUp()
    {
        mainMenuCanvas.enabled = false;
        gameCanvas.enabled = true;
        levelSelectCanvas.enabled = false;
        endScreenCanvas.enabled = false;


        //Bool check so that if we die and need to reset string we can call setup to reset and reinitialise everthing and not have it try and spawn a new string or enable disable canvasses that shouldn't be.  *** SUBJECT TO CHANGE ***
        if (initString)
        {
            sM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
            sM.InitialiseString();

            //Grab reference to all material components attached to each string point
            for (int i = 0; i < sM.StringPointsGO.Count; i++)
            {
                dissolveMaterials.Add(sM.StringPointsGO[i].GetComponent<SpriteRenderer>().material);
            }
            initString = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            sM.previousMousePosition = sM.mousePosition;
            sM.mouseDelta = Vector2.zero;
            currentState = GameState.Playing;
        }

    }

    void InitialiseDeath()
    {
        UIManager.Instance.deathCounter.text = "Deaths: " + deathCount;
        count1stHalf = 0;
        count2ndHalf = stringPointIntersectedWith;

        currentState = GameState.Dead;
    }

    void DeathAnimation()
    {
        //Loop 1st Half of string if intersected
        if (count1stHalf < stringPointIntersectedWith)
        {
            for (int i = 0; i < animationSpeed; i++)
            {
                sM.StringPointsGO[(stringPointIntersectedWith - 1) - count1stHalf].GetComponent<Dissolve>().startDissolve = true;
                count1stHalf++;
            }
        }

        //Loop 2nd half of string if intersected
        if (count2ndHalf < sM.StringPointsGO.Count)
        {
            for (int i = 0; i < animationSpeed; i++)
            {
                sM.StringPointsGO[count2ndHalf].GetComponent<Dissolve>().startDissolve = true;
                count2ndHalf++;
            }
        }


        //Check if each elements dissolve amount has reached 1 meaning animation is done and set dissolveDone to true. If are not done we set to false until the final one has changed
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

        if (dissolveDone)
        {
            if (triggerNextLevelMenu)
            {
                currentState = GameState.NextLevelMenu;
            }
            else
            {
                //As of now the player automatically respawns after they die and the animation has finished but perhaps we spawn a death ui for retry/main menu/level select or quit? I'm more drawn to insta respawn
                currentState = GameState.GameOver;
            }
        }
    }

    public void ResetString()
    {
        sM.ResetString();
        dissolveDone = false;
        triggerNextLevelMenu = false;


        for (int i = 0; i < sM.StringPointsGO.Count; i++)
        {
            sM.StringPointsGO[i].GetComponent<Dissolve>().ResetDissolve();

        }

        count1stHalf = 0;
        count2ndHalf = 0;
        stringPointIntersectedWith = 0;

        currentState = GameState.Setup;
    }

    void NextLevelScreen()
    {
        endScreenCanvas.enabled = true;
        UIManager.Instance.levelTime.text = "Level Time: " + levelTime.ToString() + "       " + CalculateMedal();
    }

    string CalculateMedal()
    {
        string message = null;
        if (levelTime < medalSplitsDict[currentLevel].x)
        {
            message = "You received a gold medal";

        }
        else if (levelTime < medalSplitsDict[currentLevel].y)
        {
            message = "You received a silver medal";

        }
        else
        {
            message = "You received a bronze medal";

        }
        return message;
    }

    public IEnumerator PlayOrContinue()
    {

        asyncLoad = SceneManager.LoadSceneAsync("level" + currentLevel.ToString(), LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        currentState = GameState.Setup;
        initString = true;
    }

    public IEnumerator LoadNextLevel()
    {
        SceneManager.UnloadSceneAsync("level" + currentLevel.ToString(), UnloadSceneOptions.None);

        currentLevel++;
        asyncLoad = SceneManager.LoadSceneAsync("level" + currentLevel.ToString(), LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        sM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
        currentState = GameState.Setup;
        ResetString();
    }

    public void ReloadLevel()
    {
        SceneManager.UnloadSceneAsync("level" + currentLevel.ToString(), UnloadSceneOptions.None);

        SceneManager.LoadSceneAsync("level" + currentLevel.ToString(), LoadSceneMode.Additive);

        sM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
        currentState = GameState.Setup;
        ResetString();
         
    }


    public IEnumerator LevelSelect()
    {
        asyncLoad = SceneManager.LoadSceneAsync(EventSystem.current.currentSelectedGameObject.name, LoadSceneMode.Additive);
        currentLevel = Int16.Parse(System.Text.RegularExpressions.Regex.Match(EventSystem.current.currentSelectedGameObject.name, @"\d+").Value);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        currentState = GameState.Setup;
        initString = true;
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);

        //We write our ingame variables to this data object when the game closes that then gets written to a file to be loaded next session.
        PlayerData data = new PlayerData();
        data.deathCount = deathCount;
        data.currentLevel = currentLevel;
        data.isLevelComplete = isLevelComplete;


        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/playerInfo.dat");
            PlayerData data = (PlayerData)bf.Deserialize(file);

            file.Close();

            //On load of game/main menu load all data from file into game and store in variable
            deathCount = data.deathCount;

            if (data.currentLevel == 0)
            {
                data.currentLevel = 1;
            }
            currentLevel = data.currentLevel;
            isLevelComplete = data.isLevelComplete;
        }
    }

    public void ResetSaveFile()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);
        PlayerData data = new PlayerData();

        bf.Serialize(file, data);
        file.Close();
    }

}


/// <summary>
/// All data that is to be written to file for saving persistent data between sessions.
/// </summary>
[Serializable]
class PlayerData
{
    public int deathCount;
    //Maybe make this the last level played and not the most recent completed level. As someone may have replayed a level but haven't completed the game so continue should just take them to the level they were last on or the level after that level if they completed it last session (determined by the end UI)
    public int currentLevel;

    public Dictionary<int, bool> isLevelComplete = new Dictionary<int, bool>();
}

