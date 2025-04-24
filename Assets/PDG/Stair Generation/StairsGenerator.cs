using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dungeonGenerator;
using System.Linq;
using System;
using UnityEngine.ProBuilder;

namespace dungeonGenerator
{
    public static class StairsGenerator
    {
        static void ConnectPlatforms(Transform transform, Vector3 startPos, Material stairsMaterial, Vector3 plane1position, Vector3 planeSize, Vector3 plane2offset)
        {



            Vector3 offsetDirection = Mathf.Abs(plane2offset.x - plane1position.x) > Mathf.Abs(plane2offset.z - plane1position.z) ?
                                      Mathf.Sign(plane2offset.x - plane1position.x) * Vector3.right :
                                      Mathf.Sign(plane2offset.z - plane1position.z) * Vector3.forward;

            Debug.Log($"offset direction: {offsetDirection}");

            // figure out which vertices are needed

            Vector3 bottomLeftV = plane1position
                                  + Vector3.Scale(offsetDirection, planeSize / 2)
                                  + Vector3.Scale(new Vector3(1, 0, 1)
                                                    - new Vector3(Mathf.Abs(offsetDirection.x),
                                                                  Mathf.Abs(offsetDirection.y),
                                                                  Mathf.Abs(offsetDirection.z)
                                                                  ),
                                                    -planeSize / 2);

            Vector3 bottomRightV = plane1position
                                   + Vector3.Scale(offsetDirection, planeSize / 2)
                                   + Vector3.Scale(new Vector3(1, 0, 1)
                                                    - new Vector3(Mathf.Abs(offsetDirection.x),
                                                                  Mathf.Abs(offsetDirection.y),
                                                                  Mathf.Abs(offsetDirection.z)
                                                                  ),
                                                    planeSize / 2);

            Vector3 topLeftV = plane1position + new Vector3(plane2offset.x,
                                            plane2offset.y,
                                            plane2offset.z) - bottomLeftV;

            Vector3 topRightV = plane1position + new Vector3(plane2offset.x,
                                            plane2offset.y,
                                            plane2offset.z) - bottomRightV;

            Vector3[] vertices = new Vector3[]
            {
            topRightV,
            topLeftV,
            bottomLeftV,
            bottomRightV
            };

            GameObject stairsPlane = MeshHelper.CreatePlaneFromPoints(vertices, 2, offsetDirection.x < 0 || offsetDirection.z > 0);
            stairsPlane.GetComponent<MeshRenderer>().sharedMaterial = stairsMaterial;

            stairsPlane.transform.localPosition = startPos;
            stairsPlane.transform.SetParent(transform);

        }

        public static void GenerateStairs(Transform transform, Vector3 startPos, Material stairsMaterial, Vector3 planeSize, List<Vector3> planeOffsets)
        {
            Vector3 curOffset = Vector3.zero;
            Vector3 prevOffset = Vector3.zero;

            GameObject plane = MeshHelper.CreatePlane(Vector3Int.CeilToInt(planeSize), 2, false);
            plane.GetComponent<MeshRenderer>().sharedMaterial = stairsMaterial;
            plane.transform.localPosition = startPos;
            plane.transform.SetParent(transform);

            foreach (var planeOffset in planeOffsets)
            {
                if (Mathf.Abs(planeOffset.x) > planeSize.x || Mathf.Abs(planeOffset.z) > planeSize.z)
                {
                    StairsGenerator.ConnectPlatforms(transform, startPos, stairsMaterial, curOffset, planeSize, curOffset + planeOffset);
                    StairsGenerator.AddConnectionWalls(transform, startPos, stairsMaterial, curOffset, planeSize, curOffset + planeOffset);
                    StairsGenerator.AddPlatformWalls(transform, startPos, stairsMaterial, curOffset, planeSize, curOffset + planeOffset, -1 * prevOffset);



                    // add ceiling 
                    StairsGenerator.ConnectPlatforms(transform, startPos + Vector3.up * 0.999f, stairsMaterial, curOffset, planeSize, curOffset + planeOffset);
                }

                curOffset += planeOffset;



                // create single wall 
                if (prevOffset != Vector3.zero)
                {
                    for (int i = 0; i < 4; i++)
                    {

                        var plane2offset = curOffset;
                        var plane1position = curOffset - planeOffset;


                        Vector3 offsetDirection = Mathf.Abs(plane2offset.x - plane1position.x) > Mathf.Abs(plane2offset.z - plane1position.z) ?
                                         Mathf.Sign(plane2offset.x - plane1position.x) * Vector3.right :
                                         Mathf.Sign(plane2offset.z - plane1position.z) * Vector3.forward;

                        Vector3 offsetDirection2 = Mathf.Abs(prevOffset.x) > Mathf.Abs(prevOffset.z) ?
                                    Mathf.Sign(prevOffset.x) * Vector3.right :
                                    Mathf.Sign(prevOffset.z) * Vector3.forward;


                        if (offsetDirection != Quaternion.AngleAxis(90 * i, Vector3.up) * Vector3.forward &&
                            -offsetDirection2 != Quaternion.AngleAxis(90 * i, Vector3.up) * Vector3.forward

                            )
                        {
                            plane = MeshHelper.CreatePlane(new Vector3Int(2, 0, 1), 2);
                            plane.transform.Rotate(-90f, 0f, 0);// = Quaternion.Euler(-90f, 0f, 0);

                            //Debug.Log($"meshFitlerVert {plane.GetComponent<MeshFilter>().sharedMesh.vertices[0]}");

                            //var rot = Quaternion.Euler(-90f, 0f, 0);
                            //var rot2 = Quaternion.Euler(0f, 90f * i, 0);

                            //Vector3[] planeVertices = new Vector3[plane.GetComponent<MeshFilter>().sharedMesh.vertices.Length];


                            //var pivot = new Vector3(2, 0, 0); //startPos + curOffset - planeOffset + Vector3.up * 0.5f + Vector3.forward - Vector3.forward * 0.001f;

                            //for (int j = 0; j < planeVertices.Length; j++)
                            //{
                            //    planeVertices[j] = rot*plane.GetComponent<MeshFilter>().sharedMesh.vertices[j];
                            //    planeVertices[j] = rot2 * (planeVertices[j]);
                            //}
                            //Mesh mesh = new Mesh();
                            //mesh = plane.GetComponent<MeshFilter>().sharedMesh;

                            //mesh.vertices = planeVertices;
                            //mesh.RecalculateTangents();
                            //mesh.RecalculateNormals();

                            //plane.GetComponent<MeshFilter>().sharedMesh = null;

                            //plane.GetComponent<MeshFilter>().sharedMesh = mesh;





                            plane.GetComponent<MeshRenderer>().sharedMaterial = stairsMaterial;


                            plane.transform.localPosition = startPos + curOffset - planeOffset + Vector3.up * 0.5f + Vector3.forward - Vector3.forward * 0.001f;



                            plane.transform.RotateAround(startPos + curOffset - planeOffset, Vector3.up, 90 * i);


                            // add light
                            GameObject light = GameObject.Instantiate(GameObject.Find("DungeonGen").GetComponent<DungeonDecorator>().lightMesh);
                            light.transform.Rotate(0f, 0f, 90f);
                            light.transform.localPosition = startPos + curOffset - planeOffset + Vector3.up * 0.5f + Vector3.forward - Vector3.forward * 0.15f;
                            light.transform.RotateAround(startPos + curOffset - planeOffset, Vector3.up, 90 * i);
                            light.transform.SetParent(transform);

                            plane.transform.SetParent(transform);
                        }
                    }
                }

                // bottomPlane

                plane = MeshHelper.CreatePlane(Vector3Int.CeilToInt(planeSize), 2, false);
                plane.GetComponent<MeshRenderer>().sharedMaterial = stairsMaterial;
                plane.transform.localPosition = startPos + curOffset;
                plane.transform.SetParent(transform);


                if (planeOffset != planeOffsets.Last())
                {
                    plane = MeshHelper.CreatePlane(Vector3Int.CeilToInt(planeSize), 2, false);
                    plane.GetComponent<MeshRenderer>().sharedMaterial = stairsMaterial;
                    plane.transform.localPosition = startPos + curOffset + Vector3.up * 0.999f;
                    plane.transform.SetParent(transform);
                }


                prevOffset = planeOffset;
            }
        }

        private static void AddPlatformWalls(Transform transform, Vector3 startPos, Material stairsMaterial, Vector3 plane1position, Vector3 planeSize, Vector3 plane2offset, Vector3 prevOffset)
        {
           
        }

        private static void AddConnectionWalls(Transform transform, Vector3 startPos, Material stairsMaterial, Vector3 plane1position, Vector3 planeSize, Vector3 plane2offset)
        {



            Vector3 offsetDirection = Mathf.Abs(plane2offset.x - plane1position.x) > Mathf.Abs(plane2offset.z - plane1position.z) ?
                                      Mathf.Sign(plane2offset.x - plane1position.x) * Vector3.right :
                                      Mathf.Sign(plane2offset.z - plane1position.z) * Vector3.forward;

            Debug.Log($"offset direction: {offsetDirection}");

            // figure out which vertices are needed


            // Left Wall Connection

            Vector3 bottomLeftV = plane1position
            + Vector3.Scale(offsetDirection, planeSize / 2)
            + Vector3.Scale(new Vector3(1, 0, 1)
                              - new Vector3(Mathf.Abs(offsetDirection.x),
                                            Mathf.Abs(offsetDirection.y),
                                            Mathf.Abs(offsetDirection.z)
                                            ),
                              planeSize / 2);


            Vector3 bottomRightV = plane1position + new Vector3(plane2offset.x,
                                            plane2offset.y,
                                            plane2offset.z) - (plane1position
                                  + Vector3.Scale(offsetDirection, planeSize / 2)
                                  + Vector3.Scale(new Vector3(1, 0, 1)
                                                    - new Vector3(Mathf.Abs(offsetDirection.x),
                                                                  Mathf.Abs(offsetDirection.y),
                                                                  Mathf.Abs(offsetDirection.z)
                                                                  ),
                                                    -planeSize / 2));


            Vector3 topRightV = bottomLeftV + new Vector3(0, 1f, 0);

            Vector3 topLeftV = bottomRightV + new Vector3(0,1f,0);

        

            Vector3[] vertices = new Vector3[]
            {
            topRightV,
            topLeftV,
            bottomLeftV,
            bottomRightV
            };

            GameObject stairsPlane = MeshHelper.CreatePlaneFromPoints(vertices, 2, offsetDirection.x > 0 || offsetDirection.z < 0, true);
            stairsPlane.GetComponent<MeshRenderer>().sharedMaterial = stairsMaterial;

            stairsPlane.transform.localPosition = startPos + stairsPlane.GetComponent<MeshFilter>().sharedMesh.normals[0]*0.001f;
            stairsPlane.transform.SetParent(transform);



            // Right Wall Connection 

            bottomLeftV = plane1position
            + Vector3.Scale(offsetDirection, planeSize / 2)
            + Vector3.Scale(new Vector3(1, 0, 1)
                              - new Vector3(Mathf.Abs(offsetDirection.x),
                                            Mathf.Abs(offsetDirection.y),
                                            Mathf.Abs(offsetDirection.z)
                                            ),
                              -planeSize / 2);


            bottomRightV = plane1position + new Vector3(plane2offset.x,
                                            plane2offset.y,
                                            plane2offset.z) - (plane1position
                                  + Vector3.Scale(offsetDirection, planeSize / 2)
                                  + Vector3.Scale(new Vector3(1, 0, 1)
                                                    - new Vector3(Mathf.Abs(offsetDirection.x),
                                                                  Mathf.Abs(offsetDirection.y),
                                                                  Mathf.Abs(offsetDirection.z)
                                                                  ),
                                                    planeSize / 2));


            topRightV = bottomLeftV + new Vector3(0, 1f, 0);

            topLeftV = bottomRightV + new Vector3(0, 1f, 0);



            vertices = new Vector3[]
            {
            topRightV,
            topLeftV,
            bottomLeftV,
            bottomRightV
            };

            stairsPlane = MeshHelper.CreatePlaneFromPoints(vertices, 2, offsetDirection.x > 0 || offsetDirection.z < 0, true);
            stairsPlane.GetComponent<MeshRenderer>().sharedMaterial = stairsMaterial;

            stairsPlane.transform.localPosition = startPos + stairsPlane.GetComponent<MeshFilter>().sharedMesh.normals[0] * 0.001f;
            stairsPlane.transform.SetParent(transform);
        }



        //// Start is called before the first frame update
        //void Start()
        //{
        //    Vector3 planeSize = new Vector3(2, 0, 2);

        //    List<Vector3> planeOffsets = new List<Vector3>
        //    {
        //        new Vector3(0f, 2.5f, -14f),
        //        new Vector3(-13f, 2.5f, 0f),
        //        new Vector3(0f, 2.5f, 17f),
        //        new Vector3(13f, 2.5f, 0f),
        //        new Vector3(0f, 2.5f, -17f),
        //        new Vector3(-13f, 2.5f, 0f),
        //        new Vector3(0, 0f, 5f),
        //        new Vector3(13f, 2.5f, 0f),

        //    };

        //    GenerateStairs(planeSize, planeOffsets);

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}
    }
}
