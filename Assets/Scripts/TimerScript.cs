using UnityEngine;


public class TimerScript : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        UIManager.Instance.timerText.color = Color.white;

        if(GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing)
        {
            GameManagerScript.Instance.LevelTime += Time.deltaTime;

            if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.gold)
            {
                UIManager.Instance.timerText.color = UIManager.Instance.gold;
            }
            else if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.silver)
            {
                UIManager.Instance.timerText.color = UIManager.Instance.silver;
            }
            else
            {
                UIManager.Instance.timerText.color = UIManager.Instance.bronze;
            }
        }

        UIManager.Instance.timerText.SetText(FormatTime(GameManagerScript.Instance.LevelTime));
    }

    private static string FormatTime(float time)
    {
        var minutes = (int)time / 60;
        var seconds = (int)time - 60 * minutes;
        var milliseconds = (int)(100 * (time - minutes * 60 - seconds));
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
