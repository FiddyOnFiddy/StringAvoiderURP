using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeveloperTools : MonoBehaviour
{
    AsyncOperation asyncLoad;
    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            GameManagerScript.Instance.ResetSaveFile();
        }

        if(Input.GetKeyUp(KeyCode.P))
        {
            GameManagerScript.Instance.CalculateTotalTimePerLevel();
        }


        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            StartCoroutine(ChangeLevel(true));

        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            StartCoroutine(ChangeLevel(false));

        }
    }

    IEnumerator ChangeLevel(bool isNextLevel)
    {

        SceneManager.UnloadSceneAsync("level" + GameManagerScript.Instance.currentLevel.ToString(), UnloadSceneOptions.None);

        if (isNextLevel)
        {
            GameManagerScript.Instance.currentLevel++;
        }
        else if(!isNextLevel)
        {
            GameManagerScript.Instance.currentLevel--;
        }
        asyncLoad = SceneManager.LoadSceneAsync("level" + GameManagerScript.Instance.currentLevel.ToString(), LoadSceneMode.Additive);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        GameManagerScript.Instance.SM.SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.Setup;
        GameManagerScript.Instance.ResetString();
        GameManagerScript.Instance.Save();
    }

}
