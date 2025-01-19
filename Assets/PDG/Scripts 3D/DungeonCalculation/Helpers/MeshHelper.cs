using System;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace dungeonGenerator
{
    public static class MeshHelper
    {
        public static GameObject CreatePlane(Vector3Int size, int uvUnit, bool isInverted = false)
        {

            // from code monkey

            Vector3 bottomLeftV = new Vector3(-size.x / 2f, 0, -size.z / 2f);
            Vector3 bottomRightV = new Vector3(size.x / 2f, 0, -size.z / 2f);
            Vector3 topLeftV = new Vector3(-size.x/2f, 0, size.z/2f);
            Vector3 topRightV = new Vector3(size.x/2f, 0, size.z/2f);

            Vector3[] vertices = new Vector3[]
            {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
            };

            // likely wrong double check
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z) * 0.5f;
            }

            int[] triangles;

            if (!isInverted)
            {
                triangles = new int[]
                {
                    0,1,2,
                    2,1,3
                };
            } else
            {
                triangles = new int[]
             {
                    0,2,1,
                    1,2,3
             };
            }

            Mesh plane = new Mesh();
            plane.vertices = vertices;
            plane.triangles = triangles;
            plane.uv = uvs;
            plane.RecalculateNormals();
            plane.RecalculateBounds();

            GameObject planeObject = new GameObject("Floor", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            planeObject.GetComponent<MeshFilter>().mesh = plane;
            planeObject.GetComponent<MeshCollider>().sharedMesh = plane;

            return planeObject;




        }

        public static Vector3 normalizeLocation(Vector3 location, Vector3 min)
        {

            var alignedLoc = location - min;
            return alignedLoc;

        }

        public static Vector3 normalizeScale(Vector3 location, Vector3 max)
        {

            var scaledLoc = new Vector3(location.x / max.x, location.y / max.y, location.z / max.z);
            return scaledLoc;
        }

        public static Vector3 normalizeVector(Vector3 location, Vector3 min, Vector3 max)
        {

            var alignedLoc = normalizeLocation(location, min);
            var scaledLoc = normalizeScale(location, max);

            return scaledLoc;
        }

        public static GameObject CreatePlaneWithHole(BoundsInt _plane, BoundsInt _hole, Vector3Int size, bool isInverted = false)
        {

            // FIXME: CONVERT TO 1/1 RELATIVE UNITS

            // NORMALIZE COORDINATES RELATIVE TO THE PLANE


            var min = new Vector3(_plane.min.x, 0, _plane.min.z); // generalize more by not setting to 0
            var max = new Vector3(_plane.max.x, 1, _plane.max.z);


            // plane XY

            Vector3 bottomLeftPlane = normalizeLocation(new Vector3(_plane.min.x, 0, _plane.min.z), min) - new Vector3(_plane.size.x/2f, 0, _plane.size.z/2f);
            Vector3 bottomRightPlane = normalizeLocation(new Vector3(_plane.max.x, 0, _plane.min.z), min) - new Vector3(_plane.size.x / 2f, 0, _plane.size.z / 2f);
            Vector3 topLeftPlane = normalizeLocation(new Vector3(_plane.min.x, 0, _plane.max.z), min) - new Vector3(_plane.size.x / 2f, 0, _plane.size.z / 2f);
            Vector3 topRightPlane = normalizeLocation(new Vector3(_plane.max.x, 0, _plane.max.z), min) - new Vector3(_plane.size.x / 2f, 0, _plane.size.z / 2f);

            // hole XY

            Vector3 bottomLeftHole = normalizeLocation(new Vector3(_hole.min.x, 0, _hole.min.z), min) - new Vector3(_plane.size.x / 2f, 0, _plane.size.z / 2f);
            Vector3 bottomRightHole = normalizeLocation(new Vector3(_hole.max.x, 0, _hole.min.z), min) - new Vector3(_plane.size.x / 2f, 0, _plane.size.z / 2f);
            Vector3 topLeftHole = normalizeLocation(new Vector3(_hole.min.x, 0, _hole.max.z), min) - new Vector3(_plane.size.x / 2f, 0, _plane.size.z / 2f);
            Vector3 topRightHole = normalizeLocation(new Vector3(_hole.max.x, 0, _hole.max.z), min) - new Vector3(_plane.size.x / 2f, 0, _plane.size.z / 2f);

            Vector3[] vertices = new Vector3[]
            {
            
                topLeftPlane,
                topRightPlane,
                bottomRightPlane,
                bottomLeftPlane,

                bottomLeftHole,
                topLeftHole,
                topRightHole,
                bottomRightHole

            };

            // likely wrong double check
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z) * 0.5f;
            }

            int[] triangles;

            triangles = new int[]
            {
                3,0,5,
                3,5,4,
                3,4,2,
                2,4,7,
                2,7,1,
                1,7,6,
                1,6,5,
                1,5,0
            };

            Mesh plane = new Mesh();
            plane.vertices = vertices;
            plane.triangles = triangles;
            plane.uv = uvs;
            plane.RecalculateNormals();
            plane.RecalculateBounds();

            GameObject planeObject = new GameObject("HoledFloor", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            planeObject.GetComponent<MeshFilter>().mesh = plane;
            planeObject.GetComponent<MeshCollider>().sharedMesh = plane;

            return planeObject;




        }

        public static GameObject CreateCuboid(Vector3Int size, int uvUnit)
        {

            // redo code with generation based o n

            // from code monkey

            Vector3 bottomLeftV1 = new Vector3(-size.x/2f, -size.y / 2f, -size.z / 2f);
            Vector3 bottomRightV1 = new Vector3(-size.x / 2f, size.y / 2f, -size.z / 2f);
            Vector3 topLeftV1 = new Vector3(-size.x / 2f, -size.y / 2f, size.z / 2f);
            Vector3 topRightV1 = new Vector3(-size.x / 2f, size.y /2f, size.z / 2f);

            Vector3 bottomLeftV2 = new Vector3(size.x / 2f, -size.y / 2f, -size.z / 2f);
            Vector3 bottomRightV2 = new Vector3(size.x / 2f, size.y / 2f, -size.z / 2f);
            Vector3 topLeftV2 = new Vector3(size.x / 2f, -size.y / 2f, size.z / 2f);
            Vector3 topRightV2 = new Vector3(size.x / 2f, size.y / 2f, size.z / 2f);

            Vector3[] vertices = new Vector3[]
            {
                // left 
                topLeftV1,
                topRightV1,
                bottomLeftV1,
                bottomRightV1,

                // right 
                topLeftV2,
                topRightV2,
                bottomLeftV2,
                bottomRightV2,

                //top
                topRightV1,
                topRightV2,
                bottomRightV1,
                bottomRightV2,

                //bottom
                topLeftV1,
                topLeftV2,
                bottomLeftV1,
                bottomLeftV2,


                //front
                topLeftV1,
                topLeftV2,
                topRightV1,
                topRightV2,

                //back
                bottomLeftV1,
                bottomLeftV2,
                bottomRightV1,
                bottomRightV2,
        };

          


            // anti clockwise
            int[] triangles = new int[]
            {
                // left
                0,1,2,
                2,1,3,

                // right
                4,6,5,
                5,6,7,

                // top
                8,9,10,
                10,9,11,

                // bottom
                12,14,13,
                13,14,15,

                // front 
                16,17,18,
                18,17,19,

                // back 
                20,22,21,
                21,22,23,
            };




            // likely wrong double check
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {

                if (i < 8)
                {
                    uvs[i] = new Vector2(vertices[i].z, vertices[i].y);
                }
                else if (i < 16)
                {
                    uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
                }
                else
                {
                    uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
                }
            }

            Mesh cuboid = new Mesh();
            cuboid.vertices = vertices;
            cuboid.triangles = triangles;
            cuboid.uv = uvs;
            cuboid.RecalculateNormals();
            cuboid.RecalculateBounds();
            cuboid.RecalculateTangents();
       

            GameObject cuboidObject = new GameObject("Floor", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            cuboidObject.GetComponent<MeshFilter>().mesh = cuboid;
            cuboidObject.GetComponent<MeshCollider>().sharedMesh = cuboid;

            return cuboidObject;
        }
    }
}