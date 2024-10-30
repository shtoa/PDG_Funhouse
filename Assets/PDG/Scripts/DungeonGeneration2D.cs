using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-2)]

// https://tutorials.eu/binary-trees-in-c-sharp/#:~:text=Binary%20trees%20are%20a%20fundamental%20data%20structure%20that%20is%20widely,such%20as%20searching%20and%20sorting.
public class BinaryTree
{
    public Node<BoundsInt> Root { get; set; } = null!;

    //public void Add(BoundsInt value)
    //{
    //    if (Root == null)
    //    {
    //        Root = new Node<BoundsInt>(value);
    //    }
    //    else
    //    {
    //        Root.Add(value);
    //    }
    //}
}

public class Node<BoundsInt> 
{
    public BoundsInt Value { get; set; }
    public Node<BoundsInt> Left { get; set; } = null!;
    public Node<BoundsInt> Right { get; set; } = null!;

    public Node(BoundsInt value) => Value = value;

    //public void Add(BoundsInt newValue)
    //{
    //    if (newValue.CompareTo(Value) < 0)
    //    {
    //        if (Left == null)
    //        {
    //            Left = new Node<BoundsInt>(newValue);
    //        }
    //        else
    //        {
    //            Left.Add(newValue);
    //        }
    //    }
    //    else
    //    {
    //        if (Right == null)
    //        {
    //            Right = new Node<T>(newValue);
    //        }
    //        else
    //        {
    //            Right.Add(newValue);
    //        }
    //    }
    //}
}



public class DungeonGeneration2D : MonoBehaviour
{

    // redo this with tree structure for connections

    public static BinaryTree tree;
    public static HashSet<Tuple<BoundsInt, BoundsInt>> childPairs = new HashSet<Tuple<BoundsInt, BoundsInt>>();
    
    public void findChildPairs(Node<BoundsInt> root)
    {
        
        if(root.Value == null)
        {
            //print("failed root");
            return;
        }

        if(root.Left != null && root.Right != null)
        {
            Tuple<BoundsInt, BoundsInt> pair = new Tuple<BoundsInt, BoundsInt>(root.Left.Value, root.Right.Value);
            childPairs.Add(pair);

        }


        if(root.Left != null)
        {
            findChildPairs(root.Left);
        } else if (root.Right != null)
        {
            findChildPairs(root.Right);
        }

        
    }

    //public void findRoom(Node<BoundsInt> root)
    //{
    //    if(root.Value == null)
    //    {
    //        return;
    //    }

    //    if (root.Left != null)
    //    {
    //        if (root.Left.Value == root.Value)
    //        {
    //            return root.Left;
    //        }
    //        else { 
    //            findRoom(root.Left);
    //        }
    //    }
    //    else if (root.Right != null)
    //    {
    //        findRoom(root.Right);
    //    }
    //}

    public static List<BoundsInt> BinarySpacePartioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<Node<BoundsInt>> roomsQueue = new Queue<Node<BoundsInt>>();
        List<BoundsInt> roomsList = new List<BoundsInt>();

   

        tree = new BinaryTree();
        tree.Root = new Node<BoundsInt>(spaceToSplit);

        roomsQueue.Enqueue(tree.Root);

        var nrSplits = 0;
        childPairs = new HashSet<Tuple<BoundsInt, BoundsInt>>();

        // check rooms that can be split
        while (roomsQueue.Count > 0)
        {
           // print(roomsQueue.Count);
            var roomNode = roomsQueue.Dequeue();
            BoundsInt room = roomNode.Value;

            //Debug.Log($"Current room size y {room.size.y} curr room size x {room.size.x}");

            if(room.size.y >= minHeight && room.size.x >= minWidth)
            {
                // play around with the splitting a proc gen
                if(Random.value < 0.5f)
                {
                    if (room.size.y >= 2 * minHeight)
                    {
                        SplitHorizontally(minHeight, minWidth, roomsQueue, room, roomNode);
                        nrSplits++;

                    }
                    else if (room.size.x >= 2 * minWidth)
                    {
                        SplitVertically(minWidth, minHeight, roomsQueue, room, roomNode);
                        nrSplits++;

                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                 else
                {
               
                    if (room.size.x >= 2 * minWidth)
                    {
                        SplitVertically(minWidth, minHeight, roomsQueue, room, roomNode);
                        nrSplits++;


                    }
                    else if (room.size.y >= 2 * minHeight)
                    {
                        SplitHorizontally(minHeight, minWidth, roomsQueue, room, roomNode);
                        nrSplits++;

                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }

        }

        //print($"nrSplits was {nrSplits}");

        return roomsList;
    }

    private static void SplitVertically(int minWidth, int minHeight, Queue<Node<BoundsInt>> roomsQueue, BoundsInt room, Node<BoundsInt> curNode)
    {
        //var xSplit = Random.Range(1, room.size.x); // minWidth, room.size.x - minWidth to make a grid
        var xSplit = Random.Range(minWidth, room.size.x - minWidth);
        //var xSplit = room.size.x - minWidth;

        BoundsInt roomLeft = new BoundsInt(room.min,new Vector3Int(xSplit,room.size.y, room.size.z));
        BoundsInt roomRight = new BoundsInt(new Vector3Int(room.min.x+xSplit, room.min.y, room.min.z), 
                               new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
    

        curNode.Left = new Node<BoundsInt>(roomLeft);
        curNode.Right = new Node<BoundsInt>(roomRight);


        Tuple<BoundsInt, BoundsInt> pair = new Tuple<BoundsInt, BoundsInt>(curNode.Left.Value, curNode.Right.Value);
        
        
        if((roomLeft.size.x >= minWidth && roomLeft.size.y >= minHeight) && (roomRight.size.x >= minWidth && roomRight.size.y >= minHeight))
        {
            childPairs.Add(pair);
        }
      


        roomsQueue.Enqueue(curNode.Left);
        roomsQueue.Enqueue(curNode.Right);



    }

    private static void SplitHorizontally(int minHeight, int minWidth,Queue<Node<BoundsInt>> roomsQueue, BoundsInt room, Node<BoundsInt> curNode)
    {
        //var ySplit = Random.Range(1, room.size.y);
        var ySplit = Random.Range(minHeight, room.size.y - minHeight);
        //var ySplit = room.size.y - minHeight;

        BoundsInt roomDown = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt roomUp = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
                               new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        

        curNode.Left = new Node<BoundsInt>(roomDown);
        curNode.Right = new Node<BoundsInt>(roomUp);

        Tuple<BoundsInt, BoundsInt> pair = new Tuple<BoundsInt, BoundsInt>(curNode.Left.Value, curNode.Right.Value);
        
        if ((roomUp.size.x >= minWidth && roomUp.size.y >= minHeight) && (roomDown.size.x >= minWidth && roomDown.size.y >= minHeight))
        {
            childPairs.Add(pair);
        }

        roomsQueue.Enqueue(curNode.Left);
        roomsQueue.Enqueue(curNode.Right);

    }


    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    [Range(0, 10)]
    private float offset = 1;

    [SerializeField]
    Vector3 _dungeonLocation = Vector3.zero;


    [SerializeField] public GameObject Waypoint;
    [SerializeField] public GameObject WaypointHolder;
    [SerializeField] public GameObject WallHolder;
    [SerializeField] public GameObject coridoorHolder;

    List<Vector3> orderedRoomLocs = new List<Vector3>();

    private void Awake()
    {
        Vector3Int startPosition = Vector3Int.zero;
        var roomList = BinarySpacePartioning(new BoundsInt((Vector3Int)startPosition, 
                       new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight); // (Vector3Int)startPosition

        Debug.Log($"Room Count {roomList.Count}");

        // HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        // floor = CreateSimpleRooms(roomList);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // destroy platforms 
        //for (int i = Waypoint.transform.childCount - 1; i >= 0; i--)
        //{
        //    DestroyImmediate(Waypoint.transform.GetChild(i).gameObject);
        //}


        // create floor
        var platformHeight = 0f;


        // do this in separate function (separrate concerns)
        List<Vector3> roomCenters = new List<Vector3>();
        foreach (var room in roomList)
        {
            platformHeight += 0.5f;
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor
           

            g.transform.localScale = new Vector3(room.size.x-offset, 0.25f, room.size.y-offset);
            g.transform.position = new Vector3(room.center.x, 0.25f, room.center.y) + _dungeonLocation;

            roomCenters.Add(g.transform.position);

            //GameObject newWaypoint = GameObject.Instantiate(Waypoint) as GameObject;
            
            //newWaypoint.transform.parent = WaypointHolder.transform;
            //newWaypoint.transform.position = g.transform.position + platformHeight * Vector3.up;


            g.transform.parent = transform;
         
        }

        // find the shortest path between rooms (do waypoint addition here)

        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;

        Vector3 closestRoomToPlayer = Vector3.zero;
        float dist = float.MaxValue;

        foreach (var center in roomCenters)
        {
            float curDist = Vector3.Distance(playerPos, center);
            if (curDist < dist)
            {
                dist = curDist;
                closestRoomToPlayer = center;
            }
        }

        orderedRoomLocs.Add(closestRoomToPlayer);

        var curRoom = closestRoomToPlayer;
        List<Vector3> allRoomCenters = new List<Vector3>();


        allRoomCenters.AddRange(roomCenters);


        AddWallsAroundRooms(roomList);




        //roomCenters.Remove(curRoom);

        #region while coridor generator
        //while (roomCenters.Count > 0)
        //foreach (var room in roomCenters)
        //{
        //    List<Vector3> allRoomCentersButCurrent = new List<Vector3>(allRoomCenters);
        //    allRoomCentersButCurrent.Remove(curRoom);
        //    print($"count of all rooms but current {allRoomCenters.Count}");
        //    Vector3 closest = FindClosestPoint(curRoom, allRoomCentersButCurrent);
        //    roomCenters.Remove(closest); // closest
        //    orderedRoomLocs.Add(closest);
        //    curRoom = closest;

        //}
        #endregion

        #region Coridor Generation using Tree Work in Progress
        //findChildPairs(tree.Root);

        //print($"Count of childpairs {childPairs.Count}");

        // create coridoors 
        //foreach (var room in roomCenters)
        //{
        //    List<Vector3> allRoomCentersButCurrent = new List<Vector3>(allRoomCenters);
        //    Vector3 closest = FindClosestPoint(room, allRoomCentersButCurrent);
        //    orderedRoomLocs.Add(room);
        //    orderedRoomLocs.Add(closest);

        //}
        //var player = GameObject.FindGameObjectWithTag("Player");
        //player.transform.position = orderedRoomLocs[0] + new Vector3(0, player.GetComponent<CharacterController>().height, 0);
        #endregion




        #region Platform Generation
        //foreach (var roomCenter in orderedRoomLocs)
        //{

        //    GameObject newWaypoint = GameObject.Instantiate(Waypoint); // add waypoint for platforms

        //    newWaypoint.transform.parent = WaypointHolder.transform;
        //    newWaypoint.transform.position = roomCenter + platformHeight * Vector3.up;

        //}

        //for(int i = 1; i < orderedRoomLocs.Count; i+=2)
        //{
        //    Vector3 start = orderedRoomLocs[i-1];
        //    Vector3 end = orderedRoomLocs[i];

        //    Vector3 dir = end-start;


        //    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor
        //    g.transform.position = (end + start) / 2f;
        //    g.transform.localScale = new Vector3(Mathf.Sqrt(dir.sqrMagnitude), 0.25f, 1f);
        //    g.transform.rotation = Quaternion.Euler(0,-Vector3.SignedAngle(dir, new Vector3(1,0,0), Vector3.up),0);
        //}
        #endregion


        #region Coridor generation
        // corridors using nodes

        foreach (var child in childPairs)
        {

            Vector3 start = child.Item1.center;
            start = new Vector3(start.x, 0.25f, start.y) + _dungeonLocation;
           
            Vector3 end = child.Item2.center;
            end = new Vector3(end.x, 0.25f, end.y) + _dungeonLocation;
            

            Vector3 dir = end - start;
            //print($"the current dir is {dir}");


            // cheat method so change later

            var corridorScale = 0f;
            var corridorPosition = (end + start)/2f;
           // print($"dir y {dir}");
            
            //if(dir.z > 0f)
            //{
            //    print($"Current max {child.Item2.min}" +
            //        $"Current min {child.Item1.max}" +
            //        $"" +
            //        $"");
            //    corridorScale = (child.Item2.min.z - child.Item1.max.z)+offset;
            //    corridorPosition = corridorPosition + new Vector3(0, 0, corridorScale);
            //    //corridorPosition = new Vector3(child.Item1.center.x, 0,
            //    //   offset

            //    //    ) ;

            //} else if (dir.x > 0f)
            //{
            //    corridorScale = (child.Item2.min.x - child.Item1.max.x)+offset;
            //    corridorPosition = corridorPosition + new Vector3(corridorScale, 0, 0);
            //} 
            


            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor
                                                                           //g.transform.localScale = new Vector3(Mathf.Sqrt(dir.sqrMagnitude)-1f, 0.25f, 1f); // subtract the width
            g.transform.localScale = new Vector3(Mathf.Sqrt(dir.sqrMagnitude) - 1f, 0.25f, 1f); // subtract the width
            g.transform.position = corridorPosition;
            //g.transform.localScale = new Vector3(corridorScale, 0.25f, 1f); // subtract the width
           

            g.transform.rotation = Quaternion.Euler(0, -Vector3.SignedAngle(dir, new Vector3(1, 0, 0), Vector3.up), 0);
            g.transform.parent = coridoorHolder.transform;
        }

        #endregion


        #region Join Floor and Cooridor Mesh

//MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
//CombineInstance[] combine = new CombineInstance[meshFilters.Length-1];

//int x = 1;
//while (x < meshFilters.Length)
//{
//    combine[x-1].mesh = meshFilters[x].sharedMesh;
//    combine[x-1].transform = meshFilters[x].transform.localToWorldMatrix;
//    //meshFilters[x].gameObject.SetActive(false);

//    x++;
//}

//print($"length of combine {combine.Length}");

//Mesh mesh = new Mesh();
////mesh.CombineMeshes(combine);
////transform.GetComponent<MeshFilter>().sharedMesh = mesh;
////transform.position = Vector3.zero;

//GetComponent<MeshCollider>().sharedMesh = mesh;
        #endregion


        RemoveWallsWithCoridoors(coridoorHolder, WallHolder);
    }

    private void RemoveWallsWithCoridoors(GameObject coridoorHolder, GameObject wallHolder)
    {


   
        for(var i = (wallHolder.transform.childCount-1); i >= 0; i--) 
        {
       
            for (var j = (coridoorHolder.transform.childCount-1); j >= 0; j--)
            {

                //print(coridoorHolder.transform.GetChild(j).GetComponent<Renderer>().bounds);

                if (coridoorHolder.transform.GetChild(j).GetComponent<Renderer>().bounds.Intersects(wallHolder.transform.GetChild(i).GetComponent<Renderer>().bounds))
                {

                    #region add walls that do not intersect around path
                    //Bounds wallBounds = wallHolder.transform.GetChild(i).GetComponent<Renderer>().bounds;
                    //Bounds coridoorBounds = coridoorHolder.transform.GetChild(j).GetComponent<Renderer>().bounds;

                    //if ((coridoorHolder.transform.GetChild(j).rotation.y == 0))

                    //    if ((coridoorBounds.min.z - wallBounds.min.z) > 0) { 
                    //        {
                    //            var wallLength = (coridoorBounds.min.z - wallBounds.min.z);

                    //            var wallHeight = 1.5f;
                    //            var wallThickness = 0.1f;




                    //            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor

                    //            leftWall.transform.position = new Vector3(wallBounds.center.x, wallHeight / 2f, wallBounds.center.z + coridoorBounds.size.z/2f + wallLength/2f);
                    //            leftWall.transform.localScale = new Vector3(wallThickness, wallHeight, wallLength);
                    //            leftWall.transform.parent = WallHolder.transform;
                    //        }

                    //    } 
                    //    if ((wallBounds.max.z - coridoorBounds.max.z) > 0)
                    //    {

                    //        {
                    //            var wallLength = (wallBounds.max.z - coridoorBounds.max.z);

                    //            var wallHeight = 1.5f;
                    //            var wallThickness = 0.1f;




                    //            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor

                    //            rightWall.transform.position = new Vector3(wallBounds.center.x, wallHeight / 2f, wallBounds.center.z - coridoorBounds.size.z / 2f - wallLength / 2f);
                    //            rightWall.transform.localScale = new Vector3(wallThickness, wallHeight, wallLength);
                    //            rightWall.transform.parent = WallHolder.transform;
                    //        }




                    //        // top and bottom 



                    //    }
                    #endregion


                    GameObject.Destroy(wallHolder.transform.GetChild(i).gameObject); // destroy the wall at current position if intersects with coridoor
                }


            }
        }
    }

    private void AddWallsAroundRooms(List<BoundsInt> roomList)
    {
        foreach (var room in roomList)
        {
            // bottom wall and top wall

            //g.transform.localScale = new Vector3(room.size.x - offset, 0.25f, room.size.y - offset);
            var wallLength = room.size.y - offset;

            var wallHeight = 1.5f;
            var wallThickness = 0.1f;

            GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor


            bottom.transform.position = new Vector3(room.center.x-(room.size.x-offset)/2f-wallThickness/2f, 0.25f+wallHeight/2f, room.center.y) + _dungeonLocation;
            bottom.transform.localScale = new Vector3(wallThickness, wallHeight, wallLength);
            bottom.transform.parent = WallHolder.transform;

            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor


            top.transform.position = new Vector3(room.center.x + (room.size.x - offset) / 2f + wallThickness / 2f, 0.25f + wallHeight / 2f, room.center.y) + _dungeonLocation;
            top.transform.localScale = new Vector3(wallThickness, wallHeight, wallLength);
            top.transform.parent = WallHolder.transform;


            // left and right wall
            wallLength = room.size.x - offset;

            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor


            left.transform.position = new Vector3(room.center.x, 
                                      0.25f + wallHeight / 2f,
                                      room.center.y - (room.size.y - offset) / 2f - wallThickness / 2f) + _dungeonLocation;
            left.transform.localScale = new Vector3(wallLength, wallHeight, wallThickness);
            left.transform.parent = WallHolder.transform;


            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube); // add room floor

            right.transform.position = new Vector3(room.center.x, 0.25f + wallHeight / 2f, room.center.y + (room.size.y - offset) / 2f + wallThickness / 2f) + _dungeonLocation;
            right.transform.localScale = new Vector3(wallLength, wallHeight, wallThickness);
            right.transform.parent = WallHolder.transform;
        }
    }





    private Vector3 FindClosestPoint(Vector3 curRoom, List<Vector3> roomCenters)
    {
        Vector3 closest = Vector3.zero;
        float dist = float.MaxValue;

        foreach(var center in roomCenters)
        {
            float curDist = Vector3.Distance(curRoom, center);

            //print($"the cur dist is {curDist}");

            if(curDist < dist && !curRoom.Equals(center) && curDist!=0)
            {
                dist = curDist;
                closest = center;
            }
        }

        return closest;
    }
}
