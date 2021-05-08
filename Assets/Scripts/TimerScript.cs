using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;


public class TimerScript : MonoBehaviour
{

    [SerializeField] TMP_Text timerText;

    [SerializeField] float levelTime = 0f;


    private void OnLevelWasLoaded(int level)
    {
        timerText = GameObject.Find("Timer").GetComponent<TMP_Text>();

    }


    // Update is called once per frame
    void Update()
    {
        if (GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Setup)
        {
            levelTime = 0f;

        }
        else if(GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing)
        {
            levelTime += Time.deltaTime;
        }

        timerText.SetText(FormatTime(levelTime));
    }

    public string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time - 60 * minutes;
        int milliseconds = (int)(100 * (time - minutes * 60 - seconds));
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}