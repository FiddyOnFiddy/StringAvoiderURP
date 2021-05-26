using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class StringMovement : MonoBehaviour
{
    private float radians;
    public Vector2 mousePosition, previousMousePosition, mouseDelta;


    [Space(5)]
    [Header("String Initialisation Data:")]
    [SerializeField] private int noOfSegments;

    [SerializeField] private float segmentLength;
    [SerializeField] private float radius;

    [Space(5)]
    [Header("Speed Limit:")]
    [SerializeField] private float stringSpeedLimit;

    [Space(5)]
    [Header("Containers For All Data:")]
    [SerializeField] private GameObject stringPointPrefab;
    [SerializeField] private List<GameObject> stringPointsGO;
    [SerializeField] private List<Rigidbody2D> stringPointsRB;
    [SerializeField] private List<Vector2> stringPointsData;


    [Space(2)]
    [SerializeField] private Transform spawnPoint;

    public List<GameObject> StringPointsGO { get => stringPointsGO; set => stringPointsGO = value; }
    public List<Rigidbody2D> StringPointsRB { get => stringPointsRB; set => stringPointsRB = value; }
    public List<Vector2> StringPointsData { get => stringPointsData; set => stringPointsData = value; }
    public int NoOfSegments { get => noOfSegments; set => noOfSegments = value; }
    public Transform SpawnPoint { get => spawnPoint; set => spawnPoint = value; }

    void Update()
    {
        if(GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && !GameManagerScript.Instance.MouseOnUIObject)
        {
            CollectInput();
            if (Input.GetMouseButtonDown(0))
            {
                
                previousMousePosition = mousePosition;
                mouseDelta = Vector2.zero;
            }

            if (Input.GetMouseButton(0))
            {
                UpdateStringPointsData(mouseDelta.x, mouseDelta.y);
                GameManagerScript.Instance.MoveRigidBodies = true;
                previousMousePosition = mousePosition;
                

            }

            if (Input.GetMouseButtonUp(0))
            {
                GameManagerScript.Instance.MoveRigidBodies = false;
            }
        }
        
    }

    void FixedUpdate()
    {
        if (GameManagerScript.Instance.MoveRigidBodies)
        {
            UpdateRigidBodies();
        }
    }
    
    
    public void UpdateStringPointsData(float x, float y)
    {
        stringPointsData[0] = new Vector2(x + stringPointsData[0].x, y + stringPointsData[0].y);

        for (int i = 1; i < noOfSegments; i++)
        {
            float nodeAngle = Mathf.Atan2(stringPointsData[i].y - stringPointsData[i - 1].y, stringPointsData[i].x - stringPointsData[i - 1].x);

            stringPointsData[i] = new Vector2(stringPointsData[i - 1].x + segmentLength * Mathf.Cos(nodeAngle), stringPointsData[i - 1].y + segmentLength * Mathf.Sin(nodeAngle));
        }
    }

    public void UpdateRigidBodies()
    {

        for (int i = 0; i < noOfSegments; i++)
        {
            stringPointsRB[i].MovePosition(stringPointsData[i]);
        }
    }

    public void CollectInput()
    {
        mouseDelta.x = Mathf.Clamp(mouseDelta.x, -stringSpeedLimit, stringSpeedLimit);
        mouseDelta.y = Mathf.Clamp(mouseDelta.y, -stringSpeedLimit, stringSpeedLimit);
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseDelta = mousePosition - previousMousePosition;
        

    }    

    public void InitialiseString()
    {
        stringPointsGO = new List<GameObject>();
        stringPointsRB = new List<Rigidbody2D>();
        stringPointsData = new List<Vector2>();

        for (int i = 0; i < noOfSegments; i++)
        {
            radians = 12 * Mathf.PI * i / noOfSegments + Mathf.PI / 4;

            stringPointsData.Add(new Vector2((spawnPoint.position.x + radius * Mathf.Cos(radians)), spawnPoint.position.y + radius * Mathf.Sin(radians)));

            stringPointsGO.Add(Instantiate(stringPointPrefab, stringPointsData[i], Quaternion.identity, this.transform));
            stringPointsGO[i].name = i.ToString();
            stringPointsRB.Add(stringPointsGO[i].GetComponent<Rigidbody2D>());
        }

        stringPointsGO[0].GetComponent<CircleCollider2D>().enabled = false;
        BoxCollider2D boxCollider = stringPointsGO[0].AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(1.3f, 1.3f);
        boxCollider.edgeRadius = 0.015f;
    }

    public void ResetString()
    {
        for (int i = 0; i < noOfSegments; i++)
        {
            radians = 12 * Mathf.PI * i / noOfSegments + Mathf.PI / 4;

            stringPointsData[i] = new Vector2((spawnPoint.position.x + radius * Mathf.Cos(radians)), spawnPoint.position.y + radius * Mathf.Sin(radians));

            stringPointsGO[i].transform.position = stringPointsData[i];
        }
    }

    public void DeleteString()
    {

        for (int i = 0; i < NoOfSegments; i++)
        {
            Destroy(stringPointsGO[i]);
        }

        stringPointsGO.Clear();
        stringPointsRB.Clear();
        StringPointsData.Clear();
    }
}
