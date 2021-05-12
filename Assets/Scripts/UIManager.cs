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

    [SerializeField] TMP_Text playButtonText;
    [SerializeField] public TMP_Text deathCounter;
    public TMP_Text timerText;
    [SerializeField] GameObject button;
    [SerializeField] GameObject content;
    [SerializeField] Sprite lockSymbol;


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

    void SetupLevelSelectScreen()
    {
        for (int i = 1; i <= GameManagerScript.Instance.maxLevelCount; i++)
        {
            GameObject clone = Instantiate(button, content.transform.position, Quaternion.identity, content.transform);
            clone.name = "Level" + i.ToString();
            clone.GetComponentInChildren<TMP_Text>().text = i.ToString();

            if(GameManagerScript.Instance.isLevelComplete.ContainsKey(i) == false)
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
