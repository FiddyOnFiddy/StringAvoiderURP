using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using ProtoBuf;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class GameManagerScript : MonoBehaviour
{

    private static GameManagerScript _instance;
    public static GameManagerScript Instance { get { return _instance; } }
    private string _saveFile;
    [field: SerializeField] public SaveData Data { get; private set; }

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
        LastLevelMenu
    }

    [Header("Global References:")]
    //List of references all scripts and this script require. Store all references in Game Manager so there is a centralised location for references and all scripts pass through Game Manager for access.
    [SerializeField] private GameState currentState;                                                        //Tracks which state we are currently in which is passed to a switch statement for processing.
    [SerializeField] private StringMovement sM;                                                             //Holds reference to the string and all it's child components.
    [SerializeField] public Canvas mainMenuCanvas, levelSelectCanvas, gameCanvas, endScreenCanvas, optionsCanvas;          //Reference to all UI Canvas for various Game States.
    [SerializeField] private List<Material> dissolveMaterials;                                              //Reference to materials attached to each string point allowing us to access dissolve script plus material per string point.

    [Space(5)]
    [Header("Boolean States:")]
    [SerializeField] private bool moveRigidBodies;                                                          //Controls when to update physics ensuring it's after input + render update.
    [SerializeField] private bool dissolveDone;                                                             //Determines when all string points have finished animation to then transition to next state.
    [SerializeField] private bool triggerNextLevelMenu, triggerLastLevelMenu;                               //Used to determine if we collided with the end trigger instead of a wall.
    [SerializeField] private bool initString;                                                               //Used to initalise string upon first level load from main menu be it: New Game; Continue or Level Select. As we only spawn string upon game load otherwise we are resetting position+variables and not creating a new set of string points.
    [SerializeField] private bool mouseOnUIObject;

    [Space(5)]
    [Header("String Collision Info:")]
    [SerializeField] private int stringPointIntersectedWith;                                                //Lets us know which string point we have collided with to play the animation from that point looping outwards in either direction.
    [SerializeField] private int count2ndHalf, count1stHalf;                                               //Count variables that represent "i" in our if statement for looping through in each direction. Reason for count variables is we are using if statement and not for loop.

    [Space(5)]
    [Header("Persistant Data:")]

    [Min(1)]
    [SerializeField] private float levelTime = 0f;                                                          //Time to complete level which is passed to the level text and which will be saved in file representing best time per level. To be added to total time variable for time across all levels.

    [Space(5)]
    [Header("Dissolve Animation Variables:")]
    [SerializeField] private float dissolveSpeed;
    [SerializeField] private float dissolveMultiplier;
    private float defaultDissolveSpeed;                                                           //Determines how fast dissolve animation will be.

    [Space(8)]
    [Header("Medal Time Splits:")]
    public List<Vector2> medalSplits = new List<Vector2>();


    public Dictionary<int, Vector2> medalSplitsDict = new Dictionary<int, Vector2>();
    public int maxLevelCount;

    public int animationSpeed;
    private AsyncOperation _asyncLoad;

    public string bronze = "Bronze", silver = "Silver", gold = "Gold", endScreenText;

    [SerializeField] private float _hudRefreshRate;
 
    private float _timer;
    



    //All the expression body properties for getting and setting any relevant data and access outside of Game Manager.
    public GameState CurrentState { get => currentState; set => currentState = value; }
    public StringMovement SM { get => sM; set => sM = value; }
    public bool MoveRigidBodies { get => moveRigidBodies; set => moveRigidBodies = value; }
    public bool TriggerNextLevelMenu { get => triggerNextLevelMenu; set => triggerNextLevelMenu = value; }
    public bool TriggerLastLevelMenu { get => triggerLastLevelMenu; set => triggerLastLevelMenu = value; }
    public int StringPointIntersectedWith { get => stringPointIntersectedWith; set => stringPointIntersectedWith = value; }
    public float LevelTime { get => levelTime; set => levelTime = value; }
    public float DissolveSpeed { get => dissolveSpeed; set => dissolveSpeed = value; }
    public bool MouseOnUIObject { get => mouseOnUIObject; }


    void Awake()
    {
        Application.targetFrameRate = 60;
        _saveFile = Path.Combine(Application.persistentDataPath, "save001.dat");

        defaultDissolveSpeed = dissolveSpeed;

        /*
        Data.DeathCount = 0;
        Data.CurrentLevel = 1;
        Data.TotalTimeToComplete = 0;
        Data.IsLevelComplete = new Dictionary<int, bool>(maxLevelCount);
        Data.TimePerLevel = new Dictionary<int, float>(maxLevelCount);
        Data.CurrentMedalPerLevel = new Dictionary<int, string>(maxLevelCount);*/
        
        #region Initialisation Stuff
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
        optionsCanvas.enabled = false;

        LoadGame();


        sM = FindObjectOfType<StringMovement>();

        currentState = GameState.Idle;
        #endregion
        
    }


    void Update()
    {
        mouseOnUIObject = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        ShowFPS();
        

        #region State switch statement
        //Switch statement which takes our current state and decides what to do based on that state.
        switch (currentState)
        {
            case GameState.Setup:
                SetUp();
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
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
            case GameState.LastLevelMenu:
                LastLevelScreen();
                break;
        }
        #endregion
    }

    void ShowFPS()
    {
        if (Time.unscaledTime > _timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            UIManager.Instance.fpsCounterLabel.text = fps.ToString();
            _timer = Time.unscaledTime + _hudRefreshRate;
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
            levelTime = 0f;

            sM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
            sM.InitialiseString();

            //Grab reference to all material components attached to each string point
            for (int i = 0; i < sM.StringPointsGO.Count; i++)
            {
                dissolveMaterials.Add(sM.StringPointsGO[i].GetComponent<SpriteRenderer>().material);
            }
            initString = false;
        }

        WaitForInput();

    }

    void WaitForInput()
    {
        if (Input.GetMouseButtonDown(0) && !GameManagerScript.Instance.MouseOnUIObject && UIManager.Instance.pauseMenuPanel.activeSelf == false)
        {
            sM.previousMousePosition = sM.mousePosition;
            sM.mouseDelta = Vector2.zero;
            currentState = GameState.Playing;
        }
    }

    void InitialiseDeath()
    {
        UIManager.Instance.deathCounter.text = "Deaths: " + Data.DeathCount;
        count1stHalf = 0;
        count2ndHalf = stringPointIntersectedWith;

        currentState = GameState.Dead;
    }

    void DeathAnimation()
    {
        for (int i = 0; i < animationSpeed; i++)
        {
            //Loop 1st Half of string if intersected
            if (count1stHalf < stringPointIntersectedWith)
            {
                sM.StringPointsGO[(stringPointIntersectedWith - 1) - count1stHalf].GetComponent<Dissolve>().startDissolve = true;
                count1stHalf++;
            }
        }

        for (int i = 0; i < animationSpeed; i++)
        {
            //Loop 2nd half of string if intersected
            if (count2ndHalf < sM.StringPointsGO.Count)
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
                dissolveSpeed += dissolveMultiplier * Time.deltaTime;
                break;
            }
        }

        if (dissolveDone)
        {
            dissolveSpeed = defaultDissolveSpeed;
            if (triggerNextLevelMenu)
            {
                currentState = GameState.NextLevelMenu;
            }
            else if(triggerLastLevelMenu)
            {
                currentState = GameState.LastLevelMenu;
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

        levelTime = 0f;
        currentState = GameState.Setup;
    }

    void NextLevelScreen()
    {
        CalculateMedal();
        endScreenCanvas.enabled = true;
        UIManager.Instance.lastLevelPanel.SetActive(false);
        UIManager.Instance.endScreenPanel.SetActive(true);
        UIManager.Instance.levelTime.text = "Level Time: " + levelTime.ToString() + "       " + endScreenText;
        
    }

    void LastLevelScreen()
    {
        CalculateMedal();
        endScreenCanvas.enabled = true;
        UIManager.Instance.lastLevelPanel.SetActive(true);
        UIManager.Instance.endScreenPanel.SetActive(false);
        UIManager.Instance.lastLevelPanelText.text = "Time To Complete All Levels: " + Data.TotalTimeToComplete.ToString();
    }

    public string CalculateMedal()
    {
        string message = null;
        if (levelTime < medalSplitsDict[Data.CurrentLevel].x)
        {
            message = gold;      

        }
        else if (levelTime < medalSplitsDict[Data.CurrentLevel].y)
        {
            message = silver;
        }
        else
        {
            message = bronze;
        }
        endScreenText = "You Received A " + message + " " + "Medal!";
        return message;
    }

    public void CalculateTotalTimePerLevel()
    {
        Data.TotalTimeToComplete = 0f;
        for (int i = 1; i <= maxLevelCount; i++)
        {
            if (Data.TimePerLevel.ContainsKey(i))
            {

                Data.TotalTimeToComplete += Data.TimePerLevel[i];
            }
        }
    }

    public IEnumerator PlayOrContinue()
    {

        _asyncLoad = SceneManager.LoadSceneAsync("level" + Data.CurrentLevel.ToString(), LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!_asyncLoad.isDone)
        {
            yield return null;
        }

        currentState = GameState.Setup;
        initString = true;
    }

    public IEnumerator LoadNextLevel()
    {
        if(Data.CurrentLevel < maxLevelCount)
        {
            SceneManager.UnloadSceneAsync("level" + Data.CurrentLevel.ToString(), UnloadSceneOptions.None);

            Data.CurrentLevel++;
            _asyncLoad = SceneManager.LoadSceneAsync("level" + Data.CurrentLevel.ToString(), LoadSceneMode.Additive);

            while (!_asyncLoad.isDone)
            {
                yield return null;
            }

            sM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
            currentState = GameState.Setup;
            ResetString();
            SaveGame();
        }
        
    }

    public IEnumerator LoadPreviousLevel()
    {
        if(Data.CurrentLevel > 1)
        {
            SceneManager.UnloadSceneAsync("level" + Data.CurrentLevel.ToString(), UnloadSceneOptions.None);

            Data.CurrentLevel--;
            _asyncLoad = SceneManager.LoadSceneAsync("level" + Data.CurrentLevel.ToString(), LoadSceneMode.Additive);

            while (!_asyncLoad.isDone)
            {
                yield return null;
            }

            sM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
            currentState = GameState.Setup;
            ResetString();
            SaveGame();
        }
        
    }

    public void ReloadLevel()
    {
        SceneManager.UnloadSceneAsync("level" + Data.CurrentLevel.ToString(), UnloadSceneOptions.None);

        SceneManager.LoadSceneAsync("level" + Data.CurrentLevel.ToString(), LoadSceneMode.Additive);

        sM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
        currentState = GameState.Setup;
        ResetString();
        SaveGame();

    }

    public void LoadMainMenu()
    {
        SceneManager.UnloadSceneAsync("level" + Data.CurrentLevel.ToString(), UnloadSceneOptions.None);
        currentState = GameState.Idle;
        sM.DeleteString();
        dissolveMaterials.Clear();
        gameCanvas.enabled = false;
        endScreenCanvas.enabled = false;
        mainMenuCanvas.enabled = true;
    }


    public IEnumerator LevelSelect()
    {
        _asyncLoad = SceneManager.LoadSceneAsync(EventSystem.current.currentSelectedGameObject.name, LoadSceneMode.Additive);
        Data.CurrentLevel = Int16.Parse(System.Text.RegularExpressions.Regex.Match(EventSystem.current.currentSelectedGameObject.name, @"\d+").Value);

        while (!_asyncLoad.isDone)
        {
            yield return null;
        }

        currentState = GameState.Setup;
        initString = true;
    }
    
    public void LoadGame()
    {
        //Check if file exists or not, if not do nothing, if so then load save data into the Data object which holds all our persistant data
        if (!File.Exists(_saveFile))
            return;
 
        
        using var file = File.OpenRead(_saveFile);
        Data = Serializer.Deserialize<SaveData>(file);
    }

    public void SaveGame()
    {
        using var file = File.OpenWrite(_saveFile);
        Serializer.Serialize(file, Data);
    }
    
    public void ResetSaveFile()
    {
        if (File.Exists(_saveFile))
        {
            File.Delete(_saveFile);
        }
    }
    
}


/// <summary>
/// All data that is to be written to file for saving persistent data between sessions.
/// </summary>
[ProtoContract]
[Serializable]
public class SaveData
{
    [ProtoMember((1))] public int DeathCount { get; set; }
    [ProtoMember(2)] public int CurrentLevel { get; set; } = 1;
    [ProtoMember(3)] public float TotalTimeToComplete { get; set; }


    [ProtoMember(4)]public Dictionary<int, bool> IsLevelComplete { get; set; } = new Dictionary<int, bool>(30);
    [ProtoMember(5)]public Dictionary<int, float> TimePerLevel { get; set; }= new Dictionary<int, float>(30);
    [ProtoMember(6)]public Dictionary<int, string> CurrentMedalPerLevel { get; set; }= new Dictionary<int, string>(30);

    
    
    
}
