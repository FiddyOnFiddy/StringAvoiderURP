using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringMovement : MonoBehaviour
{
    [SerializeField] int noOfSegments;
    public int NoOfSegments { get => noOfSegments; set => noOfSegments = value; }

    [SerializeField] float segmentLength = 0.025f;
    [SerializeField] float radius = 0.1f;

    [SerializeField] Vector2 mousePosition, previousMousePosition, mouseDelta;
    [SerializeField] float mouseSensitivity, mouseDeltaLimit = 0.25f; 

    float radians;

    [SerializeField] GameObject stringPointPrefab;
    [SerializeField] List<GameObject> stringPointsGO;
    [SerializeField] List<Rigidbody2D> stringPointsRB;
    [SerializeField] List<Vector2> stringPointsData;

	public List<GameObject> StringPointsGO { get => stringPointsGO; set => stringPointsGO = value; }
    public List<Rigidbody2D> StringPointsRB { get => stringPointsRB; set => stringPointsRB = value; }
    public List<Vector2> StringPointsData { get => stringPointsData; set => stringPointsData = value; }
    

    [SerializeField] Transform spawnPoint;

    void Awake()
    {
        InitialiseString();
    }

    void Update()
    {
        
        if(GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Idle)
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
       if(GameManagerScript.Instance.MoveRigidBodies)
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

    void CollectInput()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseDelta = mousePosition - previousMousePosition;
        mouseDelta.x = Mathf.Clamp(mouseDelta.x, -mouseDeltaLimit, mouseDeltaLimit);
        mouseDelta.y = Mathf.Clamp(mouseDelta.y, -mouseDeltaLimit, mouseDeltaLimit);

    }    

    void InitialiseString()
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
}
