using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace tutorialGenerator
{
    public class DungeonCreator : MonoBehaviour
    {
        public int dungeonWidth, dungeonLength;
        public int roomWidthMin, roomLengthMin;
        public int maxIterations;
        public int corridorWidth;
        [Range(0, 0.3f)]
        public float roomBottomCornerModifier;
        [Range(0.7f, 1f)]
        public float roomTopCorrnerModifier;
        [Range(0, 2f)]
        public int roomOffset;

        public GameObject wallVertical, wallHorizontal;
        List<Vector3Int> possibleDoorVerticalPosition;
        List<Vector3Int> possibleDoorHorizontalPosition;

        List<Vector3Int> possibleWallHorizontalPosition;
        List<Vector3Int> possibleWallVerticalPosition;


        public Material material;


        // Start is called before the first frame update
        void Start()
        {
            createDungeon();
        }

        private void createDungeon()
        {
            DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);
            var listOfRooms = generator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, roomBottomCornerModifier, roomTopCorrnerModifier, roomOffset, corridorWidth);

            GameObject wallParent = new GameObject("WallParent");
            wallParent.transform.parent = transform;

            possibleDoorVerticalPosition = new List<Vector3Int>();
            possibleDoorHorizontalPosition = new List<Vector3Int>();
            possibleWallHorizontalPosition = new List<Vector3Int>();
            possibleWallVerticalPosition = new List<Vector3Int>();

            for (int i = 0; i < listOfRooms.Count; i++)
            {
                createMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
            }

            createWalls(wallParent);
        }

        private void createWalls(GameObject wallParent)
        {
            foreach (var wallPos in possibleWallHorizontalPosition)
            {
                createWall(wallParent, wallPos, wallHorizontal);
            }

            foreach (var wallPos in possibleWallVerticalPosition)
            {
                createWall(wallParent, wallPos, wallVertical);
            }
        }

        private void createWall(GameObject wallParent, Vector3Int wallPos, GameObject wallPrefab)
        {
            Instantiate(wallPrefab, wallPos, Quaternion.identity, wallParent.transform);
        }

        private void createMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner)
        {
            // from code monkey

            Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
            Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
            Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
            Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

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
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
            }

            int[] triangles = new int[]
            {
            0,
            1,
            2,
            2,
            1,
            3
            };

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateBounds();

            GameObject dungeonFloor = new GameObject("Mesh" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

            dungeonFloor.transform.position = Vector3.zero;
            dungeonFloor.transform.localScale = Vector3.one;
            dungeonFloor.transform.parent = transform;

            dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
            dungeonFloor.GetComponent<MeshRenderer>().material = material;
            dungeonFloor.GetComponent<MeshCollider>().sharedMesh = mesh;

            dungeonFloor.transform.position = dungeonFloor.transform.position + new Vector3(0, 0.25f, 0);

            for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
            {
                var wallPosition = new Vector3(row, 0, bottomLeftV.z);
                AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
            }

            for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
            {
                var wallPosition = new Vector3(row, 0, topRightV.z);
                AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
            }

            for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
            {
                var wallPosition = new Vector3(bottomLeftV.x, 0, col);
                AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
            }

            for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
            {
                var wallPosition = new Vector3(bottomRightV.x, 0, col);
                AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
            }

        }

        private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
        {
            Vector3Int point = Vector3Int.CeilToInt(wallPosition);
            if (wallList.Contains(point))
            {
                doorList.Add(point);
                wallList.Remove(point);
            }
            else
            {
                wallList.Add(point);
            }
        }
    }
}