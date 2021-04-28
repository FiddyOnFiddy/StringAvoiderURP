using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringCollisionScript : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "String Point" && GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Idle)
        {
            Debug.Log("Collided");
            GameManagerScript.Instance.StringPointIntersectedWith = Int16.Parse(collision.contacts[0].collider.name);
            GameManagerScript.Instance.MoveRigidBodies = false;
            GameManagerScript.Instance.CurrentState = GameManagerScript.GameState.InitialiseDeath;
        }
    }
}
