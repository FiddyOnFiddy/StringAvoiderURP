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

    [SerializeField] public TMP_Text playButtonText, levelTime;
    [SerializeField] public TMP_Text deathCounter;
    public TMP_Text timerText;
    [SerializeField] GameObject button;
    [SerializeField] GameObject content;
    [SerializeField] Sprite lockSymbol;
    [SerializeField] GameObject pauseMenuPanel;
    private GameObject[] clones;

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
        clones = new GameObject[30];
        SetupLevelSelectScreen();
        deathCounter.text = "Deaths: " + GameManagerScript.Instance.DeathCount;
        pauseMenuPanel.SetActive(false);

    }

    private void Start()
    {
        if (GameManagerScript.Instance.currentLevel > 1)
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
        GameManagerScript.Instance.Save();
    }

    public void LevelSelect()
    {
        GameManagerScript.Instance.levelSelectCanvas.enabled = true;
        GameManagerScript.Instance.mainMenuCanvas.enabled = false;
    }

    public void BackButton()
    {
        GameManagerScript.Instance.levelSelectCanvas.enabled = false;
        GameManagerScript.Instance.mainMenuCanvas.enabled = true;
    }

    public void RestartButton()
    {
        GameManagerScript.Instance.ReloadLevel();
    }

    public void PauseMenuButton()
    {
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.Setup;
    }

    public void ResumeButton()
    {
        //Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    public void MainMenuButton()
    {
        GameManagerScript.Instance.Save();
        pauseMenuPanel.SetActive(false);
        UpdateLevelSelect();
        GameManagerScript.Instance.LoadMainMenu();
    }
    
    public void QuitButton()
    {
        GameManagerScript.Instance.Save();

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
            clones[i - 1] = Instantiate(button, content.transform.position, Quaternion.identity, content.transform);
            clones[i - 1].name = "Level" + i.ToString();
            clones[i - 1].GetComponentInChildren<TMP_Text>().text = i.ToString();

            if(GameManagerScript.Instance.isLevelComplete.ContainsKey(i) == false && i > 1)
            {
                Button button = clones[i - 1].GetComponent<Button>();
                button.interactable = false;
                clones[i - 1].GetComponent<Image>().sprite = lockSymbol;
            }
            else
            {
                clones[i - 1].GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(GameManagerScript.Instance.LevelSelect()); });
            }
        }     
    }

    void UpdateLevelSelect()
    {
        for (int i = 0; i < clones.Length; i++)
        {
            Destroy(clones[i]);
        }
        Array.Clear(clones, 0, clones.Length);

        if (GameManagerScript.Instance.currentLevel > 1)
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
