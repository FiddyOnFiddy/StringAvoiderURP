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

        }


        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            StartCoroutine(GameManagerScript.Instance.LoadNextLevel());

        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            StartCoroutine(GameManagerScript.Instance.LoadPreviousLevel());

        }
    }
}
