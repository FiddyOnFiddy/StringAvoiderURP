using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
            GameObject clone = Instantiate(button, content.transform.position, Quaternion.identity, content.transform);
            clone.name = "Level" + i.ToString();
            clone.GetComponentInChildren<TMP_Text>().text = i.ToString();

            if(GameManagerScript.Instance.isLevelComplete.ContainsKey(i) == false && i > 1)
            {
                Button button = clone.GetComponent<Button>();
                button.interactable = false;
                clone.GetComponent<Image>().sprite = lockSymbol;
            }
            else
            {
                clone.GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(GameManagerScript.Instance.LevelSelect()); });
            }
        }     
    }
}
