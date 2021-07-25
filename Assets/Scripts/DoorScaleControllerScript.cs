using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorScaleControllerScript : MonoBehaviour
{
    [SerializeField] List<GameObject> lockList;

    [SerializeField] private Vector3 _defaultScale;
    [SerializeField] private Vector3 _currentScale;
    [SerializeField] private float _speed;

    readonly Vector3 _zeroScale = new Vector3(0, 1, 1);

    private void Start()
    {
        _defaultScale = transform.localScale;
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        _currentScale = transform.localScale;
        if (lockList.Select(go => go.GetComponent<GravityPull>()).Any(gravityPull => !gravityPull.hasKey))
        {
            DoorClose();
            return;
        }
        DoorOpen();
    }

    private void DoorOpen()
    {

        transform.localScale = Vector3.MoveTowards(_currentScale, _zeroScale, _speed / 100);
    }

    private void DoorClose()
    {
        transform.localScale = Vector3.MoveTowards(_currentScale, _defaultScale, _speed / 100);
    }
}
