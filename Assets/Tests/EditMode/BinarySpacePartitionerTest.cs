using System;
using System.Collections;
using System.Collections.Generic;
using dungeonGenerator;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

public class BinarySpacePartitionerTest
{
    #region BinarySpacePartitionerInitialization
    
    [Test]
    [TestCase(10,10)]
    [TestCase(5, 15)]
    [TestCase(21, 3)]
    public void BinarySpacePartitionerInitializationTest(int dungeonWidth, int dungeonLength)
    {
        // Check if initialzied correctly

        BinarySpacePartitioner testPartitioner = new BinarySpacePartitioner(dungeonWidth,dungeonLength);
        
        Assert.IsTrue(testPartitioner.RootNode.Bounds.position.Equals(Vector3Int.zero), "RootNode Position");
        Assert.IsTrue(testPartitioner.RootNode.Bounds.size.Equals(new Vector3Int(dungeonWidth,0,dungeonLength)), "RootNode Size");

    }

    #endregion

    #region SplitHorizontally
    [Test]
    [TestCaseSource(nameof(SplitHorizontallyCase))]
    public void SplitHorizontallyTest((SpaceNode spaceToSplit, int hSplitPosition) testData)
    {
        SpaceNode testNode = testData.spaceToSplit;
        SpaceNode topNode = null;
        SpaceNode bottomNode = null; 

        int hSplitPosition = testData.hSplitPosition;

        (bottomNode, topNode) = BinarySpacePartitioner.SplitHorizontally(testNode, hSplitPosition);
        
        // check if Nodes Returned are Null

        Assert.IsNotNull(topNode, "topNode not null");
        Assert.IsNotNull(bottomNode, "bottomNode not null");

        // --- bottomNode Checks ---
        
        Assert.IsTrue(bottomNode.Bounds.position.Equals(testNode.Bounds.position), "bottomNode position");
        Assert.IsTrue(bottomNode.Bounds.size.Equals(new Vector3Int(testNode.Bounds.size.x,0,hSplitPosition)), "bottomNode Size");
        Assert.IsTrue(bottomNode.Parent.Equals(testNode), "bottomNode Parent");                            
        Assert.IsTrue(bottomNode.TreeLayerIndex.Equals(testNode.TreeLayerIndex + 1), "bottomNode layerIndex");
        Assert.IsTrue(bottomNode.SplitPosition.Equals(SplitPosition.Bottom), "bottomNode splitPosition");

        // --- topNode Checks ---
        
        Assert.IsTrue(topNode.Bounds.position.Equals(testNode.Bounds.position + new Vector3Int(0,0,hSplitPosition)), "topNode position");
        Assert.IsTrue(topNode.Bounds.size.Equals(new Vector3Int(testNode.Bounds.size.x, 0, testNode.Bounds.size.z - hSplitPosition)), "topNode Size");
        Assert.IsTrue(topNode.Parent.Equals(testNode), "topNode Parent");
        Assert.IsTrue(topNode.TreeLayerIndex.Equals(testNode.TreeLayerIndex + 1), "topNode layerIndex");
        Assert.IsTrue(topNode.SplitPosition.Equals(SplitPosition.Top), "topNode splitPosition");

    }

    public static IEnumerable<(SpaceNode, int)> SplitHorizontallyCase()
    {
        yield return (new SpaceNode(
            new BoundsInt(
                new Vector3Int(0, 0, 0),
                new Vector3Int(10, 0, 10)
                ), null, 0), 2);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(-10, 0, 5),
               new Vector3Int(20, 0, 10)
               ), null, 0), 5);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(30, 0, -5),
               new Vector3Int(3, 0, 20)
               ), null, 0), 14);
    }

    #endregion

    #region SplitVertically
    [Test]
    [TestCaseSource(nameof(SplitVerticallyCase))]
  
    // Test
    public void SplitVerticallyTest((SpaceNode spaceToSplit, int vSplitPosition) testData)
    {
        SpaceNode testNode = testData.spaceToSplit;

        SpaceNode leftNode = null;
        SpaceNode rightNode = null;

        int vSplitPosition = testData.vSplitPosition;

        (leftNode, rightNode) = BinarySpacePartitioner.SplitVertically(testNode, vSplitPosition);

        // check if Nodes Returned are Null

        Assert.IsNotNull(leftNode, "leftNode not null");
        Assert.IsNotNull(rightNode, "rightNode not null");

        // --- leftNode Checks ---

        Assert.IsTrue(leftNode.Bounds.position.Equals(testNode.Bounds.position), "leftNode position");
        Assert.IsTrue(leftNode.Bounds.size.Equals(new Vector3Int(vSplitPosition, 0, testNode.Bounds.size.z)), "leftNode Size");
        Assert.IsTrue(leftNode.Parent.Equals(testNode), "leftNode Parent");
        Assert.IsTrue(leftNode.TreeLayerIndex.Equals(testNode.TreeLayerIndex + 1), "leftNode layerIndex");
        Assert.IsTrue(leftNode.SplitPosition.Equals(SplitPosition.Left), "leftNode splitPosition");

        // --- rightNode Checks ---

        Assert.IsTrue(rightNode.Bounds.position.Equals(testNode.Bounds.position + new Vector3Int(vSplitPosition, 0, 0)), "rightNode position");
        Assert.IsTrue(rightNode.Bounds.size.Equals(new Vector3Int(testNode.Bounds.size.x - vSplitPosition, 0, testNode.Bounds.size.z)), "rightNode Size");
        Assert.IsTrue(rightNode.Parent.Equals(testNode), "rightNode Parent");
        Assert.IsTrue(rightNode.TreeLayerIndex.Equals(testNode.TreeLayerIndex + 1), "rightNode layerIndex");
        Assert.IsTrue(rightNode.SplitPosition.Equals(SplitPosition.Right), "rightNode splitPosition");

    }

    // Test Case
    public static IEnumerable<(SpaceNode, int)> SplitVerticallyCase()
    {
        yield return (new SpaceNode(
            new BoundsInt(
                new Vector3Int(0, 0, 0),
                new Vector3Int(10, 0, 10)
                ), null, 0), 2);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(-10, 0, 5),
               new Vector3Int(20, 0, 10)
               ), null, 0), 5);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(30, 0, -5),
               new Vector3Int(20, 0, 4)
               ), null, 0), 14);
    }

    #endregion

    #region GetSplitPositionTest
    [Test]
    [TestCase(20,2,1f)]
    [TestCase(20, 18, 0.5f)]
    [TestCase(20, 5, 0f)]
    [TestCase(20, 20, 0.3f)]
    [TestCase(2, 2, 0.7f)]
    public void GetSplitPositionTest(int size, int minSize, float splitCenterDeviationPercent)
    {
        // set Random seed for Testing 

        Random.State orignalState = Random.state;
        Random.InitState(10);

        // Instead of Testing value, test if the value is appropriate
    

        if (size / 2 - minSize < 0)
        {

            // Check if exception is thrown for invalid Split Positions
            // https://stackoverflow.com/a/50829971

            Assert.That(() => 
                  BinarySpacePartitioner.GetSplitPosition(size, minSize, splitCenterDeviationPercent),
                  Throws.TypeOf<Exception>());

        }
        else
        {
            // Goal is to produce a split that contains a room of min size (test the behaviour)
            int splitPos = BinarySpacePartitioner.GetSplitPosition(size, minSize, splitCenterDeviationPercent);

            Assert.IsNotNull(splitPos, "splitPos not null");
            Assert.GreaterOrEqual(splitPos, minSize);
            Assert.LessOrEqual(splitPos, size - minSize);
        }

        Random.state = orignalState; // reset to original Random State
    }
    #endregion

    #region SplitSpaceTest

    // Test

    [Test]
    [TestCaseSource(nameof(SplitSpaceCases))]
    public void SplitSpaceTest((SpaceNode spaceToSplit, Vector2Int minSpaceDim, Vector2 splitCenterDeviation) testData)
    {
        /// This function will test if the behaviour of the SplitSpace function
        /// The core aspect of the function is to produce the correct orrientation based on the inputs
        /// Therefore only the SplitPosition of the nodes will be tested, as well as null outputs

        // set Random seed for Testing 

        Random.State orignalState = Random.state;
        Random.InitState(10);

        SpaceNode testNode = testData.spaceToSplit;

        Vector2Int minSpaceDim = testData.minSpaceDim;
        Vector2 splitCenterDeviation = testData.splitCenterDeviation;

        int sizeX = testNode.Bounds.size.x;
        int sizeZ = testNode.Bounds.size.z;

        SpaceNode node1, node2;

        if (sizeX > 2*minSpaceDim.x && sizeZ > 2 * minSpaceDim.y)
        {
            (node1, node2) = BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation);
        }
        else if (sizeX > 2 * minSpaceDim.x)
        {
            (node1, node2) = BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation);

            Assert.True(node1.SplitPosition.Equals(SplitPosition.Left) || node1.SplitPosition.Equals(SplitPosition.Right), "Split Horizontally node1");
            Assert.True(node2.SplitPosition.Equals(SplitPosition.Left) || node2.SplitPosition.Equals(SplitPosition.Right), "Split Horizontally node2");

        } else if (sizeZ > 2 * minSpaceDim.y)
        {
            (node1, node2) = BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation);

            Assert.True(node1.SplitPosition.Equals(SplitPosition.Top) || node1.SplitPosition.Equals(SplitPosition.Bottom), "Split Vertically node1");
            Assert.True(node2.SplitPosition.Equals(SplitPosition.Top) || node2.SplitPosition.Equals(SplitPosition.Bottom), "Split Vertically node2");
            
            
        } else
        {
            Assert.That(() => BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation),
                Throws.TypeOf<Exception>());
        }

        Random.state = orignalState; // reset to original Random State
        
    }

    // Test Case
    public static IEnumerable<(SpaceNode spaceToSplit, Vector2Int minSpaceDim, Vector2 splitCenterDeviation)> SplitSpaceCases()
    {

        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(20, 0, 20)
                            ),
                            null,
                            0
                        ),
                        new Vector2Int(4,6),
                        new Vector2(1f,1f));

        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(10, 0, 14)
                            ),
                            null,
                            0
                        ),
                        new Vector2Int(2, 14),
                        new Vector2(0.5f, 1f));

        yield return (new SpaceNode(
                    new BoundsInt(
                        new Vector3Int(0, 0, 0),
                        new Vector3Int(13, 0, 25)
                    ),
                    null,
                    0
                ),
                new Vector2Int(13, 2),
                new Vector2(1f, 0.5f));

        yield return (new SpaceNode(
                   new BoundsInt(
                       new Vector3Int(0, 0, 0),
                       new Vector3Int(13, 0, 13)
                   ),
                   null,
                   0
               ),
               new Vector2Int(13, 13),
               new Vector2(1f, 1f));
    }

    #endregion

    #region GetSplitableAxisTest

    // Test
    [Test]
    public void GetSplitableAxisTest((SpaceNode spaceToSplit, Vector2Int minSpaceDim) testData)
    {
        SpaceNode testNode = testData.spaceToSplit;

        Vector2Int minSpaceDim = testData.minSpaceDim;

        bool3 testAxis = BinarySpacePartitioner.GetSplitableAxis(testNode.Bounds, minSpaceDim);

        int sizeX = testNode.Bounds.size.x;
        int sizeZ = testNode.Bounds.size.z;


        if (sizeX > 2 * minSpaceDim.x)
        {
            Assert.True(testAxis.x, "splitable along x");
        } else
        {
            Assert.False(testAxis.x, "not splitable along x");
        }

        if (sizeZ > 2 * minSpaceDim.y)
        {
            Assert.True(testAxis.z, "splitable along z");
        } else
        {
            Assert.False(testAxis.z, "not splitable along z");
        }

    }

    // Test Case
    public static IEnumerable<(SpaceNode spaceToSplit, Vector2Int minSpaceDim)> GetSplitableAxisCases()
    {

        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(10, 0, 10)
                            ),
                            null,
                            0
                        ),
                        new Vector2Int(2, 2));
        yield return (new SpaceNode(
                                new BoundsInt(
                                    new Vector3Int(0, 0, 0),
                                    new Vector3Int(5, 0, 1)
                                ),
                                null,
                                0
                            ),
                            new Vector2Int(2, 1));
        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(2, 0, 6)
                            ),
                            null,
                            0
                        ),
                        new Vector2Int(2, 3));
    }
    #endregion

    #region AddSplitSpaces
    [Test]
    public void AddSplitSpacesTest()
    {
        BinarySpacePartitioner testPartitioner = new BinarySpacePartitioner(10, 10);

        // Create empty ndoes
        var testNode1 = new SpaceNode(new BoundsInt(), null, 0);
        var testNode2 = new SpaceNode(new BoundsInt(), null, 0);

        // try adding to queue and list
        testPartitioner.AddSplitSpaces(testPartitioner.spacesToSplit, testPartitioner.allSpaces, (testNode1, testNode2));

        // Check if added to spacesToSplit

        Assert.Contains(testNode1, testPartitioner.spacesToSplit);
        Assert.Contains(testNode2, testPartitioner.spacesToSplit);

        // Check if added to allSpaces

        Assert.Contains(testNode1, testPartitioner.allSpaces);
        Assert.Contains(testNode2, testPartitioner.allSpaces);


    }
    #endregion

}
