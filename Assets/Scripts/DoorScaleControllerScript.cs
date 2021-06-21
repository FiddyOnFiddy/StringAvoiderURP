using System.Collections.Generic;
using UnityEngine;

public class DoorScaleControllerScript : MonoBehaviour
{
    [SerializeField] List<GameObject> lockList;

    [SerializeField] Vector3 defaultScale;
    [SerializeField] Vector3 currentScale;
    [SerializeField] float speed;

    Vector3 _zeroScale = new Vector3(0, 1, 1);

    void Start()
    {
        defaultScale = transform.localScale;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        currentScale = transform.localScale;
        foreach (GameObject go in lockList)
        {
            GravityPull gravityPull = go.GetComponent<GravityPull>();

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

        transform.localScale = Vector3.MoveTowards(currentScale, _zeroScale, speed / 100);
    }

    void DoorClose()
    {
        transform.localScale = Vector3.MoveTowards(currentScale, defaultScale, speed / 100);
    }
}
