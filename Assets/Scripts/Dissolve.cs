using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    private Material _material;

    private float _dissolveAmount;

    public bool startDissolve;

    public float DissolveAmount { set => _dissolveAmount = value; }

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<SpriteRenderer>().material;
        _dissolveAmount = 0;


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
        _dissolveAmount = Mathf.Clamp01(_dissolveAmount + GameManagerScript.Instance.DissolveSpeed * Time.deltaTime);
        _material.SetFloat("_DissolveAmount", _dissolveAmount);
    }

    public void ResetDissolve()
    {
        _dissolveAmount = 0;
        _material.SetFloat("_DissolveAmount", _dissolveAmount);
        startDissolve = false;
    }

}
