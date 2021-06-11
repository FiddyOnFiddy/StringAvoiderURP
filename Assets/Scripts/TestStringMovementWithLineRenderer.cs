using System;
using System.Collections.Generic;
using UnityEngine;

public class TestStringMovementWithLineRenderer : MonoBehaviour
{

    [SerializeField] private Transform _spawnPoint;
    
    [SerializeField] private LineRenderer _lineRenderer;
    private float radians;

    
    [SerializeField] private int noOfSegments;
    [SerializeField] private float segmentLength;
    [SerializeField] private float radius;
    
    
    [SerializeField] private List<Vector3> stringPointsData;


    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }



    public void InitialiseString()
    {
        stringPointsData = new List<Vector3>();

        
        for (var i = 0; i < noOfSegments; i++)
        {
            radians = 12 * Mathf.PI * i / noOfSegments + Mathf.PI / 4;

            stringPointsData.Add(new Vector3((_spawnPoint.position.x + radius * Mathf.Cos(radians)), _spawnPoint.position.y + radius * Mathf.Sin(radians)));
            
            _lineRenderer.positionCount = noOfSegments;
            _lineRenderer.SetPositions(stringPointsData.ToArray());
            


        }
        
    }

    
}
