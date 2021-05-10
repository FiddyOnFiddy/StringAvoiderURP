using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text playButtonText;

    public void PlayGame()
    {
        StartCoroutine(PlayorContinue());        
    }

    public void NextLevel()
    {
        StartCoroutine(LoadNextLevel());
        GameManagerScript.Instance.Save();
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

    IEnumerator PlayorContinue()
    {

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("level" + GameManagerScript.Instance.currentLevel.ToString(), LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.Setup;
        GameManagerScript.Instance.isPlayContinue = true;
        GameManagerScript.Instance.InitString = true;
    }

    IEnumerator LoadNextLevel()
    {
        SceneManager.UnloadSceneAsync("level" + GameManagerScript.Instance.currentLevel.ToString(), UnloadSceneOptions.None);

        GameManagerScript.Instance.currentLevel++;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("level" + GameManagerScript.Instance.currentLevel.ToString(), LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        GameManagerScript.Instance.SM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.Setup;
        GameManagerScript.Instance.ResetString();
    }

}
