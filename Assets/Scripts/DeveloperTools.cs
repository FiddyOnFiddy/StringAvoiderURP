using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeveloperTools : MonoBehaviour
{
    private string currentScene;

    // Start is called before the first frame update
    void Awake()
    {
        currentScene = SceneManager.GetActiveScene().name;

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.R))
        {
            ReloadLevel();
        }

        else if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene(currentScene);
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.Setup;

    }
}
