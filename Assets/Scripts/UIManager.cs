 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{

    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    [SerializeField] public TMP_Text playButtonText, levelTime, fpsCounterLabel;
    [SerializeField] public TMP_Text deathCounter;
    public TMP_Text timerText;
    [SerializeField] GameObject button;
    [SerializeField] GameObject content;
    [SerializeField] Sprite lockSymbol;
    [SerializeField] public GameObject pauseMenuPanel;
    private GameObject[] _clones;
    [SerializeField] public TMP_Text lastLevelPanelText;
    [SerializeField] public GameObject endScreenPanel, lastLevelPanel;
    [SerializeField] public Color bronze, silver, gold;


    private void Awake()
    {
        
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        

        _clones = new GameObject[30];
        SetupLevelSelectScreen();
        deathCounter.text = "Deaths: " + GameManagerScript.Instance.Data.DeathCount;
        lastLevelPanel.SetActive(false);
        endScreenPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);


    }

    private void Update()
    {

    }

    private void Start()
    {
        if (GameManagerScript.Instance.Data.CurrentLevel > 1)
        {
            playButtonText.text = "Continue";
        }
        else
        {
            playButtonText.text = "Play";
        }
    }
    public void PlayGame()
    {
        StartCoroutine(GameManagerScript.Instance.PlayOrContinue());
    }

    public void NextLevel()
    {
        StartCoroutine(GameManagerScript.Instance.LoadNextLevel());
    }

    public void LevelSelect()
    {
        GameManagerScript.Instance.levelSelectCanvas.enabled = true;
        GameManagerScript.Instance.mainMenuCanvas.enabled = false;
        UpdateLevelSelect();
    }

    public void BackButton()
    {
        GameManagerScript.Instance.levelSelectCanvas.enabled = false;
        GameManagerScript.Instance.optionsCanvas.enabled = false;
        GameManagerScript.Instance.mainMenuCanvas.enabled = true;
        UpdateLevelSelect();

    }

    public void RestartButton()
    {
        GameManagerScript.Instance.ReloadLevel();
        UpdateLevelSelect();

    }

    public void PauseMenuButton()
    {
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.Setup;
        UpdateLevelSelect();

    }

    public void ResumeButton()
    {
        //Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    public void MainMenuButton()
    {
        GameManagerScript.Instance.SaveGame();
        pauseMenuPanel.SetActive(false);
        UpdateLevelSelect();
        GameManagerScript.Instance.LoadMainMenu();
    }

    public void OptionsButton()
    {
        GameManagerScript.Instance.mainMenuCanvas.enabled = false;
        GameManagerScript.Instance.optionsCanvas.enabled = true;
        UpdateLevelSelect();

    }
    
    public void QuitButton()
    {
        GameManagerScript.Instance.SaveGame();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    void SetupLevelSelectScreen()
    {
        for (int i = 1; i <= GameManagerScript.Instance.maxLevelCount; i++)
        {
            _clones[i - 1] = Instantiate(button, content.transform.position, Quaternion.identity, content.transform);
            _clones[i - 1].name = "Level" + i.ToString();
            _clones[i - 1].GetComponentInChildren<TMP_Text>().text = i.ToString();

            GameManagerScript.Instance.Data.CurrentMedalPerLevel.TryGetValue(i, out string value);

            if (GameManagerScript.Instance.Data.IsLevelComplete.ContainsKey(i) == false && i > 1)
            {
                Button button = _clones[i - 1].GetComponent<Button>();
                button.interactable = false;
                _clones[i - 1].GetComponent<Image>().sprite = lockSymbol;
            }
            else
            {
                _clones[i - 1].GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(GameManagerScript.Instance.LevelSelect()); });

                if (value == GameManagerScript.Instance.gold)
                {
                    _clones[i - 1].GetComponent<Image>().color = Color.yellow;
                }
                else if (value == GameManagerScript.Instance.silver)
                {
                    _clones[i - 1].GetComponent<Image>().color = Color.cyan;
                }
                else if(value == GameManagerScript.Instance.bronze)
                {
                    _clones[i - 1].GetComponent<Image>().color =Color.red;
                }
            }
        }
    }

    public void UpdateLevelSelect()
    {
        for (int i = 0; i < _clones.Length; i++)
        {
            Destroy(_clones[i]);
        }
        Array.Clear(_clones, 0, _clones.Length);

        if (GameManagerScript.Instance.Data.CurrentLevel > 1)
        {
            playButtonText.text = "Continue";
        }
        else
        {
            playButtonText.text = "Play";
        }


        SetupLevelSelectScreen();
    }

}
