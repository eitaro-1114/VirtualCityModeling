using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class CreateMeshes : MonoBehaviour
{

    [SerializeField]
    private Material _mat;

    // Use this for initialization
    void Start()
    {

        var mesh = new Mesh();
        float root3 = Mathf.Sqrt(3f);

        Vector3[] positions = new Vector3[] {
            new Vector3 (0f, 1f, 0f),
            new Vector3 (0f, 1f, 2f),
            new Vector3 (root3, 1f, 1f),
            new Vector3 (root3, 1f, -1f),
            new Vector3 (0f, 1f, -2f),
            new Vector3 (-root3, 1f, -1f),
            new Vector3 (-root3, 1f, 1f),

            new Vector3 (0f, -1f, 0f),
            new Vector3 (0f, -1f, 2f),
            new Vector3 (root3, -1f, 1f),
            new Vector3 (root3, -1f, -1f),
            new Vector3 (0f, -1f, -2f),
            new Vector3 (-root3, -1f, -1f),
            new Vector3 (-root3, -1f, 1f),
        };
        mesh.vertices = new Vector3[] {
            // 天板
            positions[0], positions[ 1], positions[ 2], positions[ 0], positions[ 2], positions[ 3], positions[ 0], positions[ 3], positions[ 4], positions[ 0], positions[ 4], positions[ 5], positions[ 0], positions[ 5], positions[ 6], positions[ 0], positions[ 6], positions[ 1], 
            // 底板
            positions[7], positions[ 9], positions[ 8], positions[ 7], positions[10], positions[ 9], positions[ 7], positions[11], positions[10], positions[ 7], positions[12], positions[11], positions[ 7], positions[13], positions[12], positions[ 7], positions[ 8], positions[13], 

            // 側面
            positions[1], positions[ 8], positions[ 2],
            positions[2], positions[ 9], positions[ 3],
            positions[3], positions[10], positions[ 4],
            positions[4], positions[11], positions[ 5],
            positions[5], positions[12], positions[ 6],
            positions[6], positions[13], positions[ 1], 
            // 側面2
            positions[1], positions[13], positions[ 8],
            positions[2], positions[ 8], positions[ 9],
            positions[3], positions[ 9], positions[10],
            positions[4], positions[10], positions[11],
            positions[5], positions[11], positions[12],
            positions[6], positions[12], positions[13]
        };

        int[] triangles = new int[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            triangles[i] = i;
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        var filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        var renderer = GetComponent<MeshRenderer>();
        renderer.material = _mat;

    }

}