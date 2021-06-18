using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using Quaternion = UnityEngine.Quaternion;
using Slider = UnityEngine.UI.Slider;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class StringMovement : MonoBehaviour
{
    private float radians;
    public Vector2 mousePosition, previousMousePosition, mouseDelta;
    [SerializeField] private Vector2 tempPreviousPosition;
    private LineRenderer _lineRenderer;



    [Space(5)]
    [Header("String Initialisation Data:")]
    [SerializeField] private int noOfSegmentsForColliders;
    [SerializeField] private float segmentLengthForColliders;
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
    [SerializeField] private List<Vector3> lineRendererPoints;

    private bool _cachePreviousMousePosition;

    [Space(2)]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Slider _stringSensitivitySlider;
    public float speed;
    public Vector2 V;


    public List<GameObject> StringPointsGO { get => stringPointsGO; set => stringPointsGO = value; }
    public List<Rigidbody2D> StringPointsRB { get => stringPointsRB; set => stringPointsRB = value; }
    public List<Vector2> StringPointsData { get => stringPointsData; set => stringPointsData = value; }
    public int NoOfSegments { get => noOfSegmentsForColliders; set => noOfSegmentsForColliders = value; }
    public Transform SpawnPoint { get => spawnPoint; set => spawnPoint = value; }

    private void Awake()
    {
        _stringSensitivitySlider.value = stringSpeedLimit;


    }

    void Update()
    {
        CollectInput();
        if(GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Playing && !GameManagerScript.Instance.MouseOnUIObject)
        {
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
                foreach (var rb in stringPointsRB)
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }
        
    }

    private void FixedUpdate()
    {
        if (GameManagerScript.Instance.MoveRigidBodies)
        {
            //UpdateRigidBodies(Input.GetAxis("Mouse X"), Input.GetAxis(("Mouse Y")));
            UpdateRigidBodies(mouseDelta.x, mouseDelta.y);
        }
    }
    
    
    private void UpdateStringPointsData(float x, float y, bool hasCollided)
    {
        if (!hasCollided)
        {
            stringPointsData[0] = new Vector2(x + stringPointsData[0].x, y + stringPointsData[0].y);
            V = new Vector2(x, y) / Time.deltaTime;

        }
        else
        {
            stringPointsData[0] = new Vector2(x, y);
        }
        /*for (var i = 1; i < noOfSegmentsForColliders; i++)
        {
            var nodeAngle = Mathf.Atan2(stringPointsData[i].y - stringPointsData[i - 1].y, stringPointsData[i].x - stringPointsData[i - 1].x);

            stringPointsData[i] = new Vector2(stringPointsData[i - 1].x + segmentLengthForColliders * Mathf.Cos(nodeAngle), stringPointsData[i - 1].y + segmentLengthForColliders * Mathf.Sin(nodeAngle));
        }*/
    }


    private void UpdateRigidBodies(float x, float y)
    {
        /*for (var i = 0; i < noOfSegmentsForColliders; i++)
        {
            stringPointsRB[i].MovePosition(stringPointsData[i]);
        }*/

        stringPointsRB[0].velocity = V;
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
        /*mouseDelta.x = Mathf.Clamp(mouseDelta.x, -stringSpeedLimit, stringSpeedLimit);
        mouseDelta.y = Mathf.Clamp(mouseDelta.y, -stringSpeedLimit, stringSpeedLimit);*/
        
        


        stringSpeedLimit = _stringSensitivitySlider.value;
    }    

    public void InitialiseString()
    {
        stringPointsGO = new List<GameObject>();
        stringPointsRB = new List<Rigidbody2D>();
        stringPointsData = new List<Vector2>();
        lineRendererPoints = new List<Vector3>();

        for (var i = 0; i < noOfSegmentsForColliders; i++)
        {
            radians = 12 * Mathf.PI * i / noOfSegmentsForColliders + Mathf.PI / 4;

            stringPointsData.Add(new Vector2((spawnPoint.position.x + radius * Mathf.Cos(radians)), spawnPoint.position.y + radius * Mathf.Sin(radians)));

            stringPointsGO.Add(Instantiate(stringPointPrefab, stringPointsData[i], Quaternion.identity, transform));
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
        for (var i = 0; i < noOfSegmentsForColliders; i++)
        {
            radians = 12 * Mathf.PI * i / noOfSegmentsForColliders + Mathf.PI / 4;

            stringPointsData[i] = new Vector2((spawnPoint.position.x + radius * Mathf.Cos(radians)), spawnPoint.position.y + radius * Mathf.Sin(radians));

            stringPointsGO[i].transform.position = stringPointsData[i];
        }
    }

    public void DeleteString()
    {

        for (var i = 0; i < NoOfSegments; i++)
        {
            Destroy(stringPointsGO[i]);
        }

        stringPointsGO.Clear();
        stringPointsRB.Clear();
        StringPointsData.Clear();
    }
}
