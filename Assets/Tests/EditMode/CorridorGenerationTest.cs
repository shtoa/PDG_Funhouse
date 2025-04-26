using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using dungeonGenerator;
using UnityEditor;
using System.IO;

public class CorridorGenerationTest
{

    // end to end corridor generation test
    [Test]
    [TestCaseSource(nameof(EndToEndCorridorGenerationCases))]
    public void EndToEndCorridorGenerationTest((List<SpaceNode> spaceNodes, bool[,,] availableVoxelGrid, Vector3Int minRoomDim) testData)
    {
        CorridorGenerator corridorGenerator = new CorridorGenerator();
        Assert.Greater(corridorGenerator.CreateCorridors(testData.spaceNodes, 1, 1, testData.minRoomDim, 2, testData.availableVoxelGrid).Count, 0);
    }

    public static IEnumerable<(List<SpaceNode> spaceNodes, bool[,,] availableVoxelGrid, Vector3Int minRoomDim)> EndToEndCorridorGenerationCases()
    {
         
        string path = Application.dataPath + "/Tests/EditMode/ProblemConfigs";
        if (Directory.Exists(path))
        {

            string[] files = Directory.GetFiles(path, "*.txt");
            DungeonConfig dungeonConfigLoad = new DungeonConfig();  
            foreach (string file in files)
            {
                dungeonConfigLoad = Load(File.ReadAllText(file));
                Debug.Log($"corridorWidth: {dungeonConfigLoad.corridorWidth}");
                yield return GenerateSpaceNodes(dungeonConfigLoad.maxIterations,
                                   dungeonConfigLoad.roomBoundsMin,
                                   dungeonConfigLoad.splitCenterDeviation,
                                   dungeonConfigLoad.roomOffsetMin,
                                   dungeonConfigLoad.dungeonBounds.size,
                                   dungeonConfigLoad.randomSeed);


            }
           
        } 
       


    }



    public static (List<SpaceNode> spaceNodes, bool[,,] availableVoxelGrid, Vector3Int minRoomDim) GenerateSpaceNodes(int maxIterations, BoundsInt roomBoundsMin, Vector3 splitCenterDeviationPercent, Vector3Int roomOffset, Vector3Int dungeonDim, int seed, int corridorWidth = 1, int wallThickness = 1, int corridorHeight = 2)
    {

        Random.InitState(seed);

        // Calculate the Dungeon Floor Bounds:

        // FIX ME:
        var roomWidthMin = roomBoundsMin.size.x;
        var roomLengthMin = roomBoundsMin.size.z;
        var roomHeightMin = roomBoundsMin.size.y;

        #region 1. Space Partitioning

        // 1.1 Generate BSP graph based on minRoomWidth, minRoomLength and maxIterations
        BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonDim.x, dungeonDim.z, dungeonDim.y); // initialize BSP class

        // Vector of Minimum Space need to accomodate given Room Size

        // TODO: Add check if this value is impossible
        var minSpaceDim = new Vector3Int(
            roomWidthMin + roomOffset.x + wallThickness * 2,
            roomLengthMin + roomOffset.y + wallThickness * 2,
            roomHeightMin + roomOffset.z

        );

        var allNodeSpaces = bsp.PartitionSpace(maxIterations, minSpaceDim, splitCenterDeviationPercent);  // include roomOffset and wallThickness to have correct placement
        // 1.2 Extract the leaves, which represent the possible room positions (BSP Step May lead to bsp overrliance)
        var roomSpaces = GraphHelper.GetLeaves(bsp.RootNode);

        #endregion

        #region 2. Room Placement

        // TODO: Make this be passed into the generator
        var minRoomDim = new Vector3Int(
            roomWidthMin,
            roomLengthMin,
            roomHeightMin
        );

        // FIXME: Check if this value is correct
        var totalRoomOffset = roomOffset + new Vector3Int(1, 1, 0) * wallThickness * 2;

        // 2.1 Place rooms within the possible room bounds taking into account room offset
        RoomCalculator roomGenerator = new RoomCalculator(minRoomDim, totalRoomOffset, corridorWidth); // FIXME: Make more general remove corriodr width dependency
        var rooms = roomGenerator.PlaceRoomsInSpaces(roomSpaces);



        bool[,,] availableVoxelGrid = new bool[dungeonDim.x, dungeonDim.y, dungeonDim.z];
        // 0 is available space
        // 1 is taken space

        // assign voxel grid
        DungeonCalculator.assignVoxelGrid(availableVoxelGrid);
        

        // remove room spaces
        DungeonCalculator.removeRoomsFromVoxelGrid(availableVoxelGrid, rooms);
        //visualizeVoxelGrid(availableVoxelGrid);

        #endregion

        return (allNodeSpaces,availableVoxelGrid, minRoomDim);

    }
    [System.Serializable]
    private class DungeonConfig
    {
        public int maxIterations;
        public BoundsInt dungeonBounds;
        public Vector3 splitCenterDeviation;
        public BoundsInt roomBoundsMin;
        public Vector3Int roomOffsetMin;
        public Vector3Int roomPlacementRandomness;
        public bool seededGenerationEnabled;
        public int randomSeed;
        public int corridorWidth = 1;
        public int corridorHeight = 2;
        public int wallThickness = 1;
    }
    private static DungeonConfig Load(string saveFileString)
    {
        DungeonConfig dungeonConfig = JsonUtility.FromJson<DungeonConfig>(saveFileString);

        //Debug.Log($"Loaded dungeonConfig {saveFileString}");
        //Debug.Log($"Loaded dungeonConfig {dungeonConfig.roomBoundsMin.size}");

        return dungeonConfig;
    }



}
