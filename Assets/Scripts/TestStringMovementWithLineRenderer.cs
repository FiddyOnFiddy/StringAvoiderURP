using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TestStringMovementWithLineRenderer : MonoBehaviour
{

    [SerializeField] private Transform spawnPoint;
    
    [SerializeField] private LineRenderer lineRenderer;
    private float _radians;
    
    public Vector2 mousePosition, previousMousePosition, mouseDelta;


    
    [SerializeField] private int noOfSegments;
    [SerializeField] private float segmentLength;
    [SerializeField] private float radius;
    
    
    [SerializeField] private Vector3[] stringPointsData; 
    private List<Vector2> _colliderPoints;
    private EdgeCollider2D _edgeCollider2D;


    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        _edgeCollider2D = GetComponent<EdgeCollider2D>();
        InitialiseString();
    }


    private void Update()
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
            previousMousePosition = mousePosition;
                

        }

    }


    private void InitialiseString()
    {
        stringPointsData = new Vector3[noOfSegments];
        lineRenderer.positionCount = noOfSegments;

        for (var i = 0; i < noOfSegments; i++)
        {
            _radians = 12 * Mathf.PI * i / noOfSegments + Mathf.PI / 4;

            var position = spawnPoint.position;
            stringPointsData[i] = (new Vector2((position.x + radius * Mathf.Cos(_radians)), position.y + radius * Mathf.Sin(_radians)));
        }

        lineRenderer.SetPositions(stringPointsData);

    }
    
    private void CollectInput()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseDelta = mousePosition - previousMousePosition;

    }

    private void UpdateStringPointsData(float x, float y)
    {
        stringPointsData[0] = new Vector2(x + stringPointsData[0].x, y + stringPointsData[0].y);


        for (var i = 1; i < noOfSegments; i++)
        {
            var nodeAngle = Mathf.Atan2(stringPointsData[i].y - stringPointsData[i - 1].y, stringPointsData[i].x - stringPointsData[i - 1].x);

            stringPointsData[i] = new Vector2(stringPointsData[i - 1].x + segmentLength * Mathf.Cos(nodeAngle), stringPointsData[i - 1].y + segmentLength * Mathf.Sin(nodeAngle));
        }
        
        lineRenderer.SetPositions(stringPointsData);

    }
    


}

public static class MyVector3Extension
{
    public static Vector2[] toVector2Array (this Vector3[] v3)
    {
        return System.Array.ConvertAll<Vector3, Vector2> (v3, getV3fromV2);
    }
         
    public static Vector2 getV3fromV2 (Vector3 v3)
    {
        return new Vector2 (v3.x, v3.y);
    }
}
