using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 引导线（贝塞尔方法生成网格）
/// </summary>
public class PathLine : MonoBehaviour
{
    [Header("控制节点")] public Transform[] points;
    private List<Vector3> midList = new List<Vector3>();
    private List<Vector3> vertexList = new List<Vector3>();

    [Header("网格宽度")] public float width;
    [Header("骨骼数量")] public int boneCount = 5;

    public Mesh mesh;
    [Header("UV平铺Y")] public float tilingYRadio = 0.5f;
    [Header("UV动画速度")] public float uvSpeed = 0.5f;

    public GameObject prefab;

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private float length;
    private float offsetY;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh = new Mesh(); ;
        mesh.name = "Path";
        meshRenderer = GetComponent<MeshRenderer>();

        //DrawCurve();
        //GenMesh();

        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    var go = Instantiate(prefab);
        //    go.transform.localPosition = vertices[i];
        //    var text = go.GetComponent<TextMeshPro>();
        //    text.text = $"{i}({uv[i].x},{uv[i].y})";
        //}
    }

    private void Update()
    {
        DrawCurve();
        GenMesh();
        meshRenderer.sharedMaterial.SetTextureScale("_MainTex", new Vector2(1, length * tilingYRadio));
        offsetY -= uvSpeed;
        meshRenderer.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(0, offsetY * 0.001f));
    }

    private void GenMesh()
    {
        vertices = vertexList.ToArray();
        uv = new Vector2[vertices.Length];

        triangles = new int[(midList.Count - 1) * 6];

        CreateTrianglesUV();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    private void CreateTrianglesUV()
    {
        int index = 0;
        for (int i = 0; i < midList.Count - 1; i++)
        {
            index = SetTriangle(index, i * 2, i * 2 + 1, i * 2 + 2, i * 2 + 3);
            uv[i * 2] = new Vector2(0, i / (float)(midList.Count - 1));
            uv[i * 2 + 1] = new Vector2(1, i / (float)(midList.Count - 1));
        }

        uv[(midList.Count - 1) * 2] = new Vector2(0, 1);
        uv[(midList.Count - 1) * 2 + 1] = new Vector2(1, 1);
    }

    private int SetTriangle(int index, int left, int right, int upleft, int upright)
    {
        triangles[index] = left;
        triangles[index + 1] = triangles[index + 4] = upleft;
        triangles[index + 2] = triangles[index + 3] = right;
        triangles[index + 5] = upright;
        return index + 6;
    }

    void DrawCurve()
    {
        midList.Clear();
        vertexList.Clear();
        length = 0;
        for (int i = 0; i <= boneCount; i++)
        {
            float t = i / (float)boneCount;
            Vector3 p = CalculateBezierPoint(t, points[0].position, points[1].position, points[2].position, points[3].position);
            midList.Add(p);

            if (i > 0)
            {
                Vector3 to = p - midList[i - 1];
                //垂直向量
                Vector3 v = new Vector3(to.z, 0, -to.x).normalized;

                if (i == 1)
                {
                    vertexList.Add(midList[0] - v * width);
                    vertexList.Add(midList[0] + v * width);
                }
                vertexList.Add(p - v * width);
                vertexList.Add(p + v * width);

                length += to.magnitude;
            }
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < midList.Count; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(midList[i], 0.05f);
        }

        for (int i = 0; i < vertexList.Count; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertexList[i], 0.05f);
        }
    }

    /// <summary>
    /// 二阶公式
    /// </summary>
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 p0p1 = (1 - t) * p0 + t * p1;
        Vector3 p1p2 = (1 - t) * p1 + t * p2;

        Vector3 result = (1 - t) * p0p1 + t * p1p2;

        return result;
    }

    // 三阶公式
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 result;

        Vector3 p0p1 = (1 - t) * p0 + t * p1;
        Vector3 p1p2 = (1 - t) * p1 + t * p2;
        Vector3 p2p3 = (1 - t) * p2 + t * p3;

        Vector3 p0p1p2 = (1 - t) * p0p1 + t * p1p2;
        Vector3 p1p2p3 = (1 - t) * p1p2 + t * p2p3;

        result = (1 - t) * p0p1p2 + t * p1p2p3;
        return result;
    }
}
