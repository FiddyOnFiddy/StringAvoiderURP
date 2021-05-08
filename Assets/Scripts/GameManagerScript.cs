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
        Setup,
        Playing,
        InitialiseDeath,
        Dead,
        GameOver,
        NextLevelMenu,
    }

    //Initiliase a new GameState variable called currentState to track and assign which state we are in.
    [SerializeField] private GameState currentState;

    [SerializeField] private bool moveRigidBodies;
    [SerializeField] private bool dissolveDone;
    [SerializeField] private bool triggerNextLevelMenu;

    [SerializeField] private int stringPointIntersectedWith;
    [SerializeField] private  int count2ndHalf, count1stHalf;

    [SerializeField] private List<Material> dissolveMaterials;

    [SerializeField] List<TimeThreshold> medalSplits = new List<TimeThreshold>();


    [SerializeField] float dissolveSpeed;



    [SerializeField] private StringMovement sM;
    [SerializeField] Canvas nextLevelCanvas;


    public GameState CurrentState { get => currentState; set => currentState = value; }
    public int Count2ndHalf { get => count2ndHalf; set => count2ndHalf = value;  }
    public int Count1stHalf { get => count1stHalf; set => count1stHalf = value;  }
    public int StringPointIntersectedWith { get => stringPointIntersectedWith; set => stringPointIntersectedWith = value; }
    public bool MoveRigidBodies {  get => moveRigidBodies; set => moveRigidBodies = value;  }
    public bool TriggerNextLevelMenu { get => triggerNextLevelMenu; set => triggerNextLevelMenu = value; }


    private void OnLevelWasLoaded(int level)
    {
        sM = FindObjectOfType<StringMovement>();
    }
    void Awake()
    {
        //Checks to see if any Game Managers are present in the scene and either delete or assign this script to them. Necessary for singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }


        //Load all data upon start of game and pass into local variable for use in game

        currentState = GameState.Setup;

        sM = FindObjectOfType<StringMovement>();        

    }
    
    void Update()
    {
        switch (currentState)
        {
            case GameState.Setup:
                if(Input.GetMouseButtonDown(0))
                {
                    currentState = GameState.Playing;
                }
                break;
            case GameState.Playing:
                break;
            case GameState.InitialiseDeath:
                InitiliaseDeath();
                break;
            case GameState.Dead:
                //See Fixed Update
                break;
            case GameState.GameOver:
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.Setup;
                break;
            case GameState.NextLevelMenu:
                NextLevelScreen();
                break;
        }
    }

    private void FixedUpdate()
    {
        if(currentState == GameState.Dead)
        {
            DeathAnimation();
        }
    }

    void InitiliaseDeath()
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
            sM.StringPointsGO[(stringPointIntersectedWith - 1) - count1stHalf].GetComponent<Dissolve>().DissolveSpeed = dissolveSpeed;
            sM.StringPointsGO[(stringPointIntersectedWith - 1) - count1stHalf].GetComponent<Dissolve>().startDissolve = true;

            count1stHalf++;
        }

        //Loop 2nd half of string if intersected
        if (count2ndHalf < sM.StringPointsGO.Count)
        {
            sM.StringPointsGO[count2ndHalf].GetComponent<Dissolve>().DissolveSpeed = dissolveSpeed;
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

    void NextLevelScreen()
    {
        nextLevelCanvas.enabled = true;
    }

    //Move this into a UI controller script that gets passed into all buttons to handle what they do
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }


    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);

        //We write our ingame variables to this data object when the game closes that then gets written to a file to be loaded next session
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


[Serializable]
class PlayerData
{

}

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


