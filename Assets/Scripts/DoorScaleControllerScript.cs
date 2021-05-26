using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScaleControllerScript : MonoBehaviour
{
    [SerializeField] List<GameObject> lockList;

    [SerializeField] Vector3 defaultScale;
    [SerializeField] Vector3 currentScale;
    [SerializeField] float speed;

    Vector3 zeroScale = new Vector3(0, 1, 1);

    // Start is called before the first frame update
    void Start()
    {
        defaultScale = transform.localScale;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //lerpTime = Time.deltaTime * speed;
        //transform.localScale = Vector3.Lerp(defaultScale, zeroScale, );

        currentScale = transform.localScale;
        foreach (GameObject gameObject in lockList)
        {
            GravityPull gravityPull = gameObject.GetComponent<GravityPull>();

            if (!gravityPull.hasKey)
            {
                DoorClose();
                return;
            }
        }
        DoorOpen();
    }

    void DoorOpen()
    {
        //transform.localScale = Vector3.Lerp(currentScale, zeroScale, Time.time * speed);

        transform.localScale = Vector3.MoveTowards(currentScale, zeroScale, speed / 100);
    }

    void DoorClose()
    {
        //transform.localScale = Vector3.Lerp(currentScale, defaultScale, Time.time * speed);
        transform.localScale = Vector3.MoveTowards(currentScale, defaultScale, speed / 100);


    }
}
