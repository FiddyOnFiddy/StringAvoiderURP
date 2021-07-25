using System;
using UnityEngine;

public class StringCollisionScript : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("String Point") && GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && gameObject.CompareTag("Wall"))
        {
            GameManagerScript.Instance.Data.DeathCount++;
            GameManagerScript.Instance.SaveGame();   
            
            for (int i = 0; i < GameManagerScript.Instance.Sm.StringPointsRb.Count; i++)
            {
                GameManagerScript.Instance.Sm.StringPointsRb[i].transform.position = GameManagerScript.Instance.Sm.StringPointsData[i];

            }
            
            TriggerDeath(collision);
        }
        else if (collision.collider.CompareTag("String Point") && GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && gameObject.CompareTag("EndPoint"))
        {
            GameManagerScript.Instance.CalculateTotalTimePerLevel();
            GameManagerScript.Instance.Data.IsLevelComplete[GameManagerScript.Instance.Data.CurrentLevel + 1] = true;
            PopulateTimerPerLevelData();
            CalculateBestMedalPerLevel();
            GameManagerScript.Instance.SaveGame();            
            if (GameManagerScript.Instance.Data.CurrentLevel < GameManagerScript.Instance.maxLevelCount)
            {
                GameManagerScript.Instance.TriggerNextLevelMenu = true;
            }
            else
            {
                GameManagerScript.Instance.TriggerLastLevelMenu = true;
            }
            TriggerDeath(collision);
        }
    }

    private static void TriggerDeath(Collision2D collision)
    {
        GameManagerScript.Instance.StringPointIntersectedWith = Int16.Parse(collision.contacts[0].collider.name);
        GameManagerScript.Instance.MoveRigidBodies = false;
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.InitialiseDeath;
    }

    private static void CalculateBestMedalPerLevel()
    {
        GameManagerScript.Instance.Data.CurrentMedalPerLevel.TryGetValue(GameManagerScript.Instance.Data.CurrentLevel, out string value);

        if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.gold)
        {
            if (value != GameManagerScript.Instance.gold)
            {
                GameManagerScript.Instance.Data.CurrentMedalPerLevel[GameManagerScript.Instance.Data.CurrentLevel] = GameManagerScript.Instance.gold;
            }
            else
            {
                GameManagerScript.Instance.Data.CurrentMedalPerLevel[GameManagerScript.Instance.Data.CurrentLevel] = GameManagerScript.Instance.gold;
            }
        }
        else if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.silver)
        {
            if (value == GameManagerScript.Instance.bronze)
            {
                GameManagerScript.Instance.Data.CurrentMedalPerLevel[GameManagerScript.Instance.Data.CurrentLevel] = GameManagerScript.Instance.silver;
            }
            else if (value == GameManagerScript.Instance.gold)
            {

            }
            else
            {
                GameManagerScript.Instance.Data.CurrentMedalPerLevel[GameManagerScript.Instance.Data.CurrentLevel] = GameManagerScript.Instance.silver;
            }
        }
        else if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.bronze)
        {
            if (value == GameManagerScript.Instance.gold | value == GameManagerScript.Instance.silver)
            {
                
            }
            else
            {
                GameManagerScript.Instance.Data.CurrentMedalPerLevel[GameManagerScript.Instance.Data.CurrentLevel] = GameManagerScript.Instance.bronze;

            }

        }
    }


    private static void PopulateTimerPerLevelData()
    {
        if (GameManagerScript.Instance.Data.TimePerLevel.ContainsKey(GameManagerScript.Instance.Data.CurrentLevel))
        {
            if (GameManagerScript.Instance.LevelTime < GameManagerScript.Instance.Data.TimePerLevel[GameManagerScript.Instance.Data.CurrentLevel])
            {
                GameManagerScript.Instance.Data.TimePerLevel[GameManagerScript.Instance.Data.CurrentLevel] = GameManagerScript.Instance.LevelTime;
            }
        }
        else
        {
            GameManagerScript.Instance.Data.TimePerLevel[GameManagerScript.Instance.Data.CurrentLevel] = GameManagerScript.Instance.LevelTime;
        }
    }
}
