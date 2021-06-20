using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeveloperTools : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {

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
