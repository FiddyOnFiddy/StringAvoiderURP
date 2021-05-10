using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [SerializeField] Material material;

    [SerializeField] float dissolveAmount, dissolveSpeed;

    [SerializeField] public bool startDissolve;

    public float DissolveSpeed { set => dissolveSpeed = value; }
    public float DissolveAmount { set => dissolveAmount = value; }

    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
        dissolveAmount = 0;


        Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        transform.rotation = randomRotation;



    }

    // Update is called once per frame
    void Update()
    {
        if(GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Dead && startDissolve)
        {
            DissolveStringPoint();
        }  
    }

    void DissolveStringPoint()
    {
        dissolveAmount = Mathf.Clamp01(dissolveAmount + dissolveSpeed * Time.deltaTime);
        material.SetFloat("_DissolveAmount", dissolveAmount);
    }


}
