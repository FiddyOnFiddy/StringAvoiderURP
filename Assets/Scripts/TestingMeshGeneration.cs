using UnityEngine;

public class TestingMeshGeneration : MonoBehaviour
{
    
    [SerializeField] private Vector3[] newVertices;
    [SerializeField] private Vector2[] newUV;
    [SerializeField] private int[] newTriangles;


    private void Awake()
    {
        StringMovement sm = GameManagerScript.Instance.SM;
        for (int i = 0; i < sm.NoOfSegments; i++)
        {
            newVertices[i].x = sm.StringPointsData[i].x;
            newVertices[i].y = sm.StringPointsData[i].y;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
    }
}
