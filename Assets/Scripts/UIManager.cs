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
        StartCoroutine(LoadYourAsyncScene());        
    }

    private void Awake()
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

    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.
        

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
}
