using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Slider = UnityEngine.UI.Slider;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class StringMovement : MonoBehaviour
{
    private float _radians;
    public Vector2 mousePosition, previousMousePosition, mouseDelta;
    private LineRenderer _lineRenderer;
    
    [Space(5)]
    [Header("String Initialisation Data:")]
    [SerializeField] private int noOfStringPoints;
    [SerializeField] private float distanceBetweenPoints;
    [SerializeField] private int noOfSringPointCollidersToSkip;
    [SerializeField] private float radius;

    [Space(5)]
    [Header("Speed Limit:")]
    [SerializeField] private float stringSpeedLimit;

    [Space(5)]
    [Header("Containers For All Data:")]
    [SerializeField] private GameObject stringPointPrefab;
    [SerializeField] private PhysicsMaterial2D defaultPhysicsMaterial2D;
    [SerializeField] private List<GameObject> stringPointsGO;
    [FormerlySerializedAs("stringPointsRB")] [SerializeField] private List<Rigidbody2D> stringPointsRb;
    [SerializeField] private List<Vector2> stringPointsData;

    private bool _cachePreviousMousePosition;

    [Space(2)]
    [SerializeField] private Transform spawnPoint;
    [FormerlySerializedAs("_stringSensitivitySlider")] [SerializeField] private Slider stringSensitivitySlider;



    public List<GameObject> StringPointsGO => stringPointsGO;
    public List<Rigidbody2D> StringPointsRb { get => stringPointsRb; set => stringPointsRb = value; }
    public List<Vector2> StringPointsData { get => stringPointsData; set => stringPointsData = value; }
    public Transform SpawnPoint { set => spawnPoint = value; }

    private void Awake()
    {
        stringSensitivitySlider.value = stringSpeedLimit;
    }

    void Update()
    {
        CollectInput();

        if (GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && !GameManagerScript.Instance.MouseOnUIObject)
        {
            UpdateDataFromRb();

            if (Input.GetMouseButtonDown(0))
            {
                previousMousePosition = mousePosition;
                mouseDelta = Vector2.zero;
            }

            if (Input.GetMouseButton(0))
            {
                CheckForCollisionsBetweenLastAndCurrentPositions();
                previousMousePosition = mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                GameManagerScript.Instance.MoveRigidBodies = false;
            }
        }

        if (GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Dead)
        {
            for (int i = 0; i < noOfStringPoints; i++)
            {
                stringPointsRb[i].transform.position = stringPointsData[i];

            }

        }
    }

    private void FixedUpdate()
    {
        if (GameManagerScript.Instance.MoveRigidBodies)
        {
            UpdateRigidBodies();
        }
    }


    private void UpdateStringPointsData(float x, float y, bool hasCollided)
    {
        if (!hasCollided)
        {
            stringPointsData[0] = new Vector2(x + stringPointsData[0].x, y + stringPointsData[0].y);
        }
        else
        {
            stringPointsData[0] = new Vector2(x, y);
        }

        for (var i = 1; i < noOfStringPoints; i++)
        {
            var nodeAngle = Mathf.Atan2(stringPointsData[i].y - stringPointsData[i - 1].y, stringPointsData[i].x - stringPointsData[i - 1].x);

            stringPointsData[i] = new Vector2(stringPointsData[i - 1].x + distanceBetweenPoints * Mathf.Cos(nodeAngle), stringPointsData[i - 1].y + distanceBetweenPoints * Mathf.Sin(nodeAngle));
        }
    }


    private void UpdateRigidBodies()
    {
        for (var i = 0; i < noOfStringPoints; i++)
        {
            stringPointsRb[i].MovePosition(stringPointsData[i]);
        }
    }

    private void UpdateDataFromRb()
    {
        for (var i = 0; i < noOfStringPoints; i++)
        {
            stringPointsData[i] = stringPointsRb[i].transform.position;
        }
    }

    private void CheckForCollisionsBetweenLastAndCurrentPositions()
    {
        Vector3 dir = mousePosition - previousMousePosition;

        RaycastHit2D hit = Physics2D.Raycast(stringPointsGO[0].transform.position, dir, Vector3.Distance(mousePosition, previousMousePosition), LayerMask.GetMask("Walls"));

        if (hit.collider != null)
        {
            UpdateStringPointsData(hit.point.x + hit.normal.x * 0.1f, hit.point.y + hit.normal.y * 0.1f, true);
            GameManagerScript.Instance.MoveRigidBodies = true;
            GameManagerScript.Instance.RayHasCollidedWithWall = true;
        }

        if (!GameManagerScript.Instance.RayHasCollidedWithWall)
        {
            UpdateStringPointsData(mouseDelta.x, mouseDelta.y, false);
            GameManagerScript.Instance.MoveRigidBodies = true;
        }
    }


    private void CollectInput()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseDelta = mousePosition - previousMousePosition;
        //mouseDelta.x = Mathf.Clamp(mouseDelta.x, -stringSpeedLimit, stringSpeedLimit);
        //mouseDelta.y = Mathf.Clamp(mouseDelta.y, -stringSpeedLimit, stringSpeedLimit);

        stringSpeedLimit = stringSensitivitySlider.value;
    }

    public void InitialiseString()
    {
        stringPointsGO = new List<GameObject>();
        stringPointsRb = new List<Rigidbody2D>();
        stringPointsData = new List<Vector2>();

        for (var i = 0; i < noOfStringPoints; i++)
        {
            _radians = 12 * Mathf.PI * i / noOfStringPoints + Mathf.PI / 4;

            stringPointsData.Add(new Vector2((spawnPoint.position.x + radius * Mathf.Cos(_radians)), spawnPoint.position.y + radius * Mathf.Sin(_radians)));

            stringPointsGO.Add(Instantiate(stringPointPrefab, stringPointsData[i], Quaternion.identity, transform));
            stringPointsGO[i].name = i.ToString();
            stringPointsRb.Add(stringPointsGO[i].GetComponent<Rigidbody2D>());
        }

        for (int i = 0; i < noOfStringPoints; i += noOfSringPointCollidersToSkip)
        {
            CircleCollider2D circleCollider2D = stringPointsGO[i].AddComponent<CircleCollider2D>();
            circleCollider2D.radius = 0.9f;
            //\circleCollider2D.sharedMaterial = defaultPhysicsMaterial2D;
        }

        Destroy(stringPointsGO[0].GetComponent<CircleCollider2D>());
        BoxCollider2D boxCollider = stringPointsGO[0].AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(1.3f, 1.3f);
        boxCollider.edgeRadius = 0.015f;
    }

    public void ResetString()
    {
        for (var i = 0; i < noOfStringPoints; i++)
        {
            _radians = 12 * Mathf.PI * i / noOfStringPoints + Mathf.PI / 4;

            stringPointsData[i] = new Vector2((spawnPoint.position.x + radius * Mathf.Cos(_radians)), spawnPoint.position.y + radius * Mathf.Sin(_radians));

            stringPointsGO[i].transform.position = stringPointsData[i];
        }
    }

    public void DeleteString()
    {
        for (var i = 0; i < noOfStringPoints; i++)
        {
            Destroy(stringPointsGO[i]);
        }

        stringPointsGO.Clear();
        stringPointsRb.Clear();
        stringPointsData.Clear();
    }
}
