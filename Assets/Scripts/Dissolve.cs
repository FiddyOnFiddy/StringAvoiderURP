using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [SerializeField] Material material;

    [SerializeField] float dissolveAmount, dissolveSpeed;
    [SerializeField] Vector2 randomSeed;

    [SerializeField] public bool startDissolve;

    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
        dissolveAmount = 0;
        dissolveSpeed = 0.8f;

        randomSeed = new Vector2(Random.Range(0f, 4f), Random.Range(0f, 3f));
        material.SetVector("_RandomSeed", randomSeed);
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
        material.SetFloat("_DoDisortion", 1.0f);
    }


}
