using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringCollisionScript : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("String Point") && GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && this.gameObject.CompareTag("Wall"))
        {
            GameManagerScript.Instance.DeathCount++;
            GameManagerScript.Instance.Save();
            TriggerDeath(collision);
        }
        else if (collision.collider.CompareTag("String Point") && GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && this.gameObject.CompareTag("EndPoint"))
        {
            GameManagerScript.Instance.TriggerNextLevelMenu = true;
            TriggerDeath(collision);
        }
    }

    void TriggerDeath(Collision2D collision)
    {
        GameManagerScript.Instance.StringPointIntersectedWith = Int16.Parse(collision.contacts[0].collider.name);
        GameManagerScript.Instance.MoveRigidBodies = false;
        GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.InitialiseDeath;
    }
}
