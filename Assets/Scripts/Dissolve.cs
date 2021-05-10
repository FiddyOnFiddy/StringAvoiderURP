using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    private Material material;

    private float dissolveAmount;

    public bool startDissolve;

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
        dissolveAmount = Mathf.Clamp01(dissolveAmount + GameManagerScript.Instance.DissolveSpeed * Time.deltaTime);
        material.SetFloat("_DissolveAmount", dissolveAmount);
    }

    public void ResetDissolve()
    {
        dissolveAmount = 0;
        material.SetFloat("_DissolveAmount", dissolveAmount);
        startDissolve = false;
    }

}
