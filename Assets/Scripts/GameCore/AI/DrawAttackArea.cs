using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DrawAttackArea : MonoBehaviour
{

    public float anglefov = 30;
    public int quality = 2;
    public int[] triangles;
    public Vector2[] uvs;
    public Vector3[] vertices;
    public Vector3[] normals;

    public float dist_min = 0.0f;
    public float dist_max = 1f;

    public MeshFilter meshFilter;

    void Awake()
    {
        triangles = new int[quality * 2 * 3];
        vertices = new Vector3[quality * 2 + 2];
        uvs = new Vector2[vertices.Length];
        normals = new Vector3[vertices.Length];

        meshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {

        meshFilter.mesh.Clear();

        Vector3 pos = Vector3.zero;//transform.position;
        float angle_lookat = transform.eulerAngles.y;
        float angle_fov = anglefov;

        float angle_start = angle_lookat - angle_fov;
        float angle_end = angle_lookat + angle_fov;
        float angle_delta = (angle_end - angle_start) / quality;

        float angle_curr = angle_end;

        for (int i = 0; i < quality + 1; i++)
        {
            Vector3 sphere_curr = new Vector3();
            sphere_curr.z = Mathf.Cos(Mathf.Deg2Rad * angle_curr);
            sphere_curr.x = Mathf.Sin(Mathf.Deg2Rad * angle_curr);

            Vector3 pos_curr_min = pos + sphere_curr * dist_min;
            Vector3 pos_curr_max = pos + sphere_curr * dist_max;

            vertices[2 * i + 0] = pos_curr_min;
            vertices[2 * i + 1] = pos_curr_max;

            uvs[2 * i + 0] = new Vector2((float)(quality - i) / quality, 0);
            uvs[2 * i + 1] = new Vector2((float)(quality - i) / quality, 1);

            normals[2 * i + 0] = Vector3.up;
            normals[2 * i + 1] = Vector3.up;

            angle_curr -= angle_delta;
        }

        for (int i = 0; i < quality; i++)
        {
            //  5---3---1
            //  |  /|  /|
            //  | / | / |
            //  |/  |/  |
            //  4---2---0

            int index_min_cur = i * 2 + 0;
            int index_max_cur = i * 2 + 1;
            int index_min_next = i * 2 + 2;
            int index_max_next = i * 2 + 3;

            triangles[6 * i + 0] = index_min_cur;
            triangles[6 * i + 1] = index_min_next;
            triangles[6 * i + 2] = index_max_cur;
            triangles[6 * i + 3] = index_min_next;
            triangles[6 * i + 4] = index_max_next;
            triangles[6 * i + 5] = index_max_cur;

        }

        meshFilter.sharedMesh.vertices = vertices;
        meshFilter.sharedMesh.triangles = triangles;
        meshFilter.sharedMesh.uv = uvs;
    }
}
