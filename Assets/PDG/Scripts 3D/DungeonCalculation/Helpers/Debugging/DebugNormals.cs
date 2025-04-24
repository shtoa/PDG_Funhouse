using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using dungeonGenerator;




namespace dungeonGenerator
{
    [ExecuteInEditMode]
    public class DebugNormals : MonoBehaviour
{
    public Color normalColor = Color.green;
    public float normalLength = 0.2f;
    public Mesh mesh;

        private void Start()
        {
            
        }

        private void OnDrawGizmosSelected()
        {

            if (mesh != null)
            {
                //MeshFilter meshFilter = GetComponent<MeshFilter>();

                // return if no meshFiler or mesh found 

                //if (meshFilter != null || meshFilter.sharedMesh == null) return;



                // get mesh normals and vertices
                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;


                Gizmos.color = normalColor;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldPosition = transform.TransformPoint(vertices[i]); // get world position of vertices

                    Gizmos.DrawRay(worldPosition, normals[i]*normalLength);



                }
            }
        }
}
}
