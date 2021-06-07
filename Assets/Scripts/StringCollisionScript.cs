using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringCollisionScript : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("String Point") && GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && this.gameObject.CompareTag("Wall"))
        {
            GameManagerScript.Instance.Data.DeathCount++;
            GameManagerScript.Instance.SaveGame();
            TriggerDeath(collision);
        }
        else if (collision.collider.CompareTag("String Point") && GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && this.gameObject.CompareTag("EndPoint"))
        {
            GameManagerScript.Instance.CalculateTotalTimePerLevel();
            GameManagerScript.Instance.isLevelComplete[GameManagerScript.Instance.currentLevel + 1] = true;
            PopulateTimerPerLevelData();
            CalculateBestMedalPerLevel();
            GameManagerScript.Instance.SaveGame();
            if (GameManagerScript.Instance.currentLevel < GameManagerScript.Instance.maxLevelCount)
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

    void TriggerDeath(Collision2D collision)
    {
        GameManagerScript.Instance.StringPointIntersectedWith = Int16.Parse(collision.contacts[0].collider.name);
        GameManagerScript.Instance.MoveRigidBodies = false;
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.InitialiseDeath;
    }

    void CalculateBestMedalPerLevel()
    {
        GameManagerScript.Instance.currentMedalPerLevel.TryGetValue(GameManagerScript.Instance.currentLevel, out string value);

        if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.gold)
        {
            if (value != GameManagerScript.Instance.gold)
            {
                GameManagerScript.Instance.currentMedalPerLevel[GameManagerScript.Instance.currentLevel] = GameManagerScript.Instance.gold;
            }
            else
            {
                GameManagerScript.Instance.currentMedalPerLevel[GameManagerScript.Instance.currentLevel] = GameManagerScript.Instance.gold;
            }
        }
        else if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.silver)
        {
            if (value == GameManagerScript.Instance.bronze)
            {
                GameManagerScript.Instance.currentMedalPerLevel[GameManagerScript.Instance.currentLevel] = GameManagerScript.Instance.silver;
            }
            else if (value == GameManagerScript.Instance.gold)
            {

            }
            else
            {
                GameManagerScript.Instance.currentMedalPerLevel[GameManagerScript.Instance.currentLevel] = GameManagerScript.Instance.silver;
            }
        }
        else if (GameManagerScript.Instance.CalculateMedal() == GameManagerScript.Instance.bronze)
        {
            if (value == GameManagerScript.Instance.gold | value == GameManagerScript.Instance.silver)
            {

            }
            else
            {
                GameManagerScript.Instance.currentMedalPerLevel[GameManagerScript.Instance.currentLevel] = GameManagerScript.Instance.bronze;

            }

        }
    }


    void PopulateTimerPerLevelData()
    {
        if (GameManagerScript.Instance.timePerLevel.ContainsKey(GameManagerScript.Instance.currentLevel))
        {
            if (GameManagerScript.Instance.LevelTime < GameManagerScript.Instance.timePerLevel[GameManagerScript.Instance.currentLevel])
            {
                GameManagerScript.Instance.timePerLevel[GameManagerScript.Instance.currentLevel] = GameManagerScript.Instance.LevelTime;
            }
        }
        else
        {
            GameManagerScript.Instance.timePerLevel[GameManagerScript.Instance.currentLevel] = GameManagerScript.Instance.LevelTime;
        }
    }
}
