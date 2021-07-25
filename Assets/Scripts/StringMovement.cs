using System;
using System.Collections.Generic;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;


public class StringMovement : MonoBehaviour
{
    //Private Variables
    private float _radians;
    private bool _cachePreviousMousePosition; 
    [HideInInspector] public Vector2 mousePosition, mouseDelta, previousMousePosition;


    //Class to hold all initialisation data for the string
    [Serializable]
    public class InitialisationData
    {
        public int noOfStringPoints;
        public float distanceBetweenPoints;
        public int noOfStringPointCollidersToSkip;
        public float radius;
    }
    
    //Class to hold all the references the string script has
    [Serializable]
    public class References
    {
        public GameObject stringPointPrefab;
        public Transform spawnPoint;
        public Slider sensitivitySlider;
    }

    //Class to hold all the data pertaining to the string
    [Serializable]
    public class StringData
    {
        public List<Vector2> stringPointsData;
        public List<GameObject> stringPointsGO;
        public List<Rigidbody2D> stringPointsRb;
    }
    
    
    //Each of these is the initialisation of each class
    [Space(5)]
    [SerializeField] private InitialisationData initialisationData;
    
    [Space(5)]
    [SerializeField] private References references;
    
    [Space(5)]
    [SerializeField] private StringData stringData;
    
    [Space(10)]
    [SerializeField] private float stringSpeedLimit;
    

    //Public getter and/or setter for use in another script
    public List<GameObject> StringPointsGO => stringData.stringPointsGO;
    public List<Rigidbody2D> StringPointsRb { get => stringData.stringPointsRb; set => stringData.stringPointsRb = value; }
    public List<Vector2> StringPointsData { get => stringData.stringPointsData; set => stringData.stringPointsData = value; }
    public Transform SpawnPoint { set => references.spawnPoint = value; }

    private void Awake()
    {
        references.sensitivitySlider.value = stringSpeedLimit;
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
                //Update string points data then tell rigidbodies to update afterwards
                UpdateStringPointsData(mouseDelta.x, mouseDelta.y);
                GameManagerScript.Instance.MoveRigidBodies = true;
                previousMousePosition = mousePosition;
                
                //Toggle Kinematic back to false while player is interacting so we have dynamic physics bodies
                ToggleKinematic(false);
            }

            if (Input.GetMouseButtonUp(0))
            {
                GameManagerScript.Instance.MoveRigidBodies = false;
                ToggleKinematic(true);
            }
        }

        if (GameManagerScript.Instance.CurrentState == GameManagerScript.GameState.Dead)
        {
            for (int i = 0; i < initialisationData.noOfStringPoints; i++)
            {
                stringData.stringPointsRb[i].transform.position = stringData.stringPointsData[i];

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


    private void UpdateStringPointsData(float x, float y)
    {
        stringData.stringPointsData[0] = new Vector2( x + stringData.stringPointsData[0].x, y + stringData.stringPointsData[0].y);
        
        for (var i = 1; i < initialisationData.noOfStringPoints; i++)
        {
            var nodeAngle = Mathf.Atan2(stringData.stringPointsData[i].y - stringData.stringPointsData[i - 1].y, stringData.stringPointsData[i].x - stringData.stringPointsData[i - 1].x);

            stringData.stringPointsData[i] = new Vector2(stringData.stringPointsData[i - 1].x + initialisationData.distanceBetweenPoints * Mathf.Cos(nodeAngle), stringData.stringPointsData[i - 1].y + initialisationData.distanceBetweenPoints * Mathf.Sin(nodeAngle));
        }
    }


    private void UpdateRigidBodies()
    {
        for (var i = 0; i < initialisationData.noOfStringPoints; i++)
        {
            stringData.stringPointsRb[i].MovePosition(stringData.stringPointsData[i]);
        }
    }
    

    private void UpdateDataFromRb()
    {
        for (var i = 0; i < initialisationData.noOfStringPoints; i++)
        {
            stringData.stringPointsData[i] = stringData.stringPointsRb[i].transform.position;
        }
    }

    private void ToggleKinematic(bool toggleKinematic)
    {

        for (var i = 0; i < initialisationData.noOfStringPoints; i++)
        {
            stringData.stringPointsRb[i].bodyType = toggleKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        }
    }
    
    private void CollectInput()
    {
        if (Camera.main is { }) mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        mouseDelta = mousePosition - previousMousePosition;
    }

    public void InitialiseString()
    {
        stringData.stringPointsGO = new List<GameObject>();
        stringData.stringPointsRb = new List<Rigidbody2D>();
        stringData.stringPointsData = new List<Vector2>();

        for (var i = 0; i < initialisationData.noOfStringPoints; i++)
        {
            _radians = 12 * Mathf.PI * i / initialisationData.noOfStringPoints + Mathf.PI / 4;

            stringData.stringPointsData.Add(new Vector2((references.spawnPoint.position.x + initialisationData.radius * Mathf.Cos(_radians)), references.spawnPoint.position.y + initialisationData.radius * Mathf.Sin(_radians)));

            stringData.stringPointsGO.Add(Instantiate(references.stringPointPrefab, stringData.stringPointsData[i], Quaternion.identity, transform));
            stringData.stringPointsGO[i].name = i.ToString();
            stringData.stringPointsRb.Add(stringData.stringPointsGO[i].GetComponent<Rigidbody2D>());
        }

        for (int i = 0; i < initialisationData.noOfStringPoints; i += initialisationData.noOfStringPointCollidersToSkip)
        {
            CircleCollider2D circleCollider2D = stringData.stringPointsGO[i].AddComponent<CircleCollider2D>();
            circleCollider2D.radius = 0.9f;
        }

        Destroy(stringData.stringPointsGO[0].GetComponent<CircleCollider2D>());
        BoxCollider2D boxCollider = stringData.stringPointsGO[0].AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(1.3f, 1.3f);
        boxCollider.edgeRadius = 0.015f;
    }

    public void ResetString()
    {
        for (var i = 0; i < initialisationData.noOfStringPoints; i++)
        {
            _radians = 12 * Mathf.PI * i / initialisationData.noOfStringPoints + Mathf.PI / 4;

            stringData.stringPointsData[i] = new Vector2((references.spawnPoint.position.x + initialisationData.radius * Mathf.Cos(_radians)), references.spawnPoint.position.y + initialisationData.radius * Mathf.Sin(_radians));

            stringData.stringPointsGO[i].transform.position = stringData.stringPointsData[i];
        }
    }

    public void DeleteString()
    {
        for (var i = 0; i < initialisationData.noOfStringPoints; i++)
        {
            Destroy(stringData.stringPointsGO[i]);
        }

        stringData.stringPointsGO.Clear();
        stringData.stringPointsRb.Clear();
        stringData.stringPointsData.Clear();
    }
}
