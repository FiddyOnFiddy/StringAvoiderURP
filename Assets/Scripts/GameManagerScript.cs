using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;

public class GameManagerScript : MonoBehaviour
{

    private static GameManagerScript _instance;
    public static GameManagerScript Instance { get { return _instance; } }

    //Declaring new enum of type GameState to handle what state we're currently in
    public enum GameState
    {
        Idle,
        InitialiseDeath,
        CleanupLists,
        Dead,
        GameOver,
    }

    //Initiliase a new GameState variable called currentState to track and assign which state we are in.
    [SerializeField] private GameState currentState;

    [SerializeField] private List<Material> tempDissolveMaterials;
    [SerializeField] private List<float> dissolveAmounts;

    [SerializeField] private bool moveRigidBodies;

    [SerializeField] private int stringPointIntersectedWith;
    [SerializeField] private  int count2ndHalf, count1stHalf;

    [SerializeField] float dissolveAmount, dissolveSpeed, cascadeSpeed;


    private StringMovement sM;

    public GameState CurrentState { get => currentState; set => currentState = value; }
    public int Count2ndHalf { get => count2ndHalf; set => count2ndHalf = value;  }
    public int Count1stHalf { get => count1stHalf; set => count1stHalf = value;  }
    public int StringPointIntersectedWith { get => stringPointIntersectedWith; set => stringPointIntersectedWith = value; }
    public bool MoveRigidBodies {  get => moveRigidBodies; set => moveRigidBodies = value;  }


    
    struct TimeThreshold 
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


    /// <summary>
    /// List of variables on script per scene 
    /// get passed through to timethreshold struct which get passed into list or dictionary for player prefs
    /// </summary>
    List<TimeThreshold> timeThresholds = new List<TimeThreshold>();

    void Start()
    {
        //Default Game State should always be idle after string and other componenents have initialised
        currentState = GameState.Idle;

        //Grab reference to string movement script to handle death animation
        sM = FindObjectOfType<StringMovement>();

        //dissolveAmount = 1f;
        dissolveAmounts = new List<float>();
        for (int i = 0; i < sM.DissolveMaterials.Count; i++)
        {
            dissolveAmounts.Add(1f);
        }

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


    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Idle:
                break;
            case GameState.InitialiseDeath:
                InitiliaseDeath();
                break;
            case GameState.CleanupLists:
                CleanUpLists();
                break;
            case GameState.Dead:
                //See Fixed Update
                //DeathAnimation();
                break;
            case GameState.GameOver:
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


    void DeathAnimation()
    {
        dissolveAmount = dissolveAmount - dissolveSpeed * Time.deltaTime;


        for (int i = 0; i < sM.StringPointsGO.Count; i++)
        {
            float offset = i * cascadeSpeed;
            sM.DissolveMaterials[(sM.StringPointsGO.Count - 1) - i].SetFloat("_DissolveAmount", dissolveAmount + offset);
            count1stHalf++;
        }

        for (int i = 0; i < tempDissolveMaterials.Count; i++)
        {
            float offset = i * cascadeSpeed;
            tempDissolveMaterials[i].SetFloat("_DissolveAmount", dissolveAmount + offset);
            count2ndHalf++;
        }

    }

    void InitiliaseDeath()
    {
        tempDissolveMaterials = new List<Material>();

        for (int i = stringPointIntersectedWith; i < sM.StringPointsGO.Count; i++)
        {
            tempDissolveMaterials.Add(sM.DissolveMaterials[i]);
        }

        count1stHalf = 0;
        count2ndHalf = 0;
        currentState = GameState.CleanupLists;
    }

    void CleanUpLists()
    {
        int amountToRemove = sM.StringPointsData.Count - stringPointIntersectedWith;
        sM.StringPointsGO.RemoveRange(stringPointIntersectedWith, amountToRemove);
        currentState = GameState.Dead;
    }


}