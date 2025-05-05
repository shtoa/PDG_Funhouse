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
    [TestCase(10,10,5)]
    [TestCase(5, 15, 4)]
    [TestCase(21, 3, 10)]
    public void BinarySpacePartitionerInitializationTest(int dungeonWidth, int dungeonHeight, int dungeonLength)
    {
        // Check if initialzied correctly

        BinarySpacePartitioner testPartitioner = new BinarySpacePartitioner(dungeonWidth, dungeonLength, dungeonHeight);
        
        Assert.IsTrue(testPartitioner.RootNode.Bounds.position.Equals(Vector3Int.zero), "RootNode Position");
        Assert.IsTrue(testPartitioner.RootNode.Bounds.size.Equals(new Vector3Int(dungeonWidth, dungeonHeight, dungeonLength)), "RootNode Size");

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
                ), null, 0, 0), 2);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(-10, 0, 5),
               new Vector3Int(20, 0, 10)
               ), null, 0, 0), 5);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(30, 0, -5),
               new Vector3Int(3, 0, 20)
               ), null, 0, 0), 14);
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
                ), null, 0, 0), 2);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(-10, 0, 5),
               new Vector3Int(20, 0, 10)
               ), null, 0, 0), 5);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(30, 0, -5),
               new Vector3Int(20, 0, 4)
               ), null, 0, 0), 14);
    }

    #endregion

    #region SplitPerpendicular
    [Test]
    [TestCaseSource(nameof(SplitPerpendicularCase))]

    // Test
    public void SplitPerpendicularTest((SpaceNode spaceToSplit, int vSplitPosition) testData)
    {
        SpaceNode testNode = testData.spaceToSplit;

        SpaceNode upNode = null;
        SpaceNode downNode = null;

        int vSplitPosition = testData.vSplitPosition;

        (downNode, upNode) = BinarySpacePartitioner.SplitPerpendicular(testNode, vSplitPosition);

        // check if Nodes Returned are Null

        Assert.IsNotNull(upNode, "leftNode not null");
        Assert.IsNotNull(downNode, "rightNode not null");

        // --- leftNode Checks ---

        Assert.IsTrue(upNode.Bounds.position.Equals(testNode.Bounds.position + new Vector3Int(0, vSplitPosition, 0)), "upNode position");
        Assert.IsTrue(upNode.Bounds.size.Equals(new Vector3Int(testNode.Bounds.size.x, testNode.Bounds.size.y - vSplitPosition, testNode.Bounds.size.z)), "upNode Size");
        Assert.IsTrue(upNode.Parent.Equals(testNode), "upNode Parent");
        Assert.IsTrue(upNode.TreeLayerIndex.Equals(testNode.TreeLayerIndex + 1), "upNode layerIndex");
        Assert.IsTrue(upNode.SplitPosition.Equals(SplitPosition.Up), "upNode splitPosition");

        // --- rightNode Checks ---

        Assert.IsTrue(downNode.Bounds.position.Equals(testNode.Bounds.position), "downNode position");
        Assert.IsTrue(downNode.Bounds.size.Equals(new Vector3Int(testNode.Bounds.size.x, vSplitPosition, testNode.Bounds.size.z)), "downNode Size");
        Assert.IsTrue(downNode.Parent.Equals(testNode), "downNode Parent");
        Assert.IsTrue(downNode.TreeLayerIndex.Equals(testNode.TreeLayerIndex + 1), "downNode layerIndex");
        Assert.IsTrue(downNode.SplitPosition.Equals(SplitPosition.Down), "downNode splitPosition");

    }

    // Test Case
    public static IEnumerable<(SpaceNode, int)> SplitPerpendicularCase()
    {
        yield return (new SpaceNode(
            new BoundsInt(
                new Vector3Int(0, 0, 0),
                new Vector3Int(10, 10, 10)
                ), null, 0, 0), 2);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(-10, 0, 5),
               new Vector3Int(20, 20, 10)
               ), null, 0, 0), 5);
        yield return (new SpaceNode(
           new BoundsInt(
               new Vector3Int(30, 0, -5),
               new Vector3Int(20, 10, 4)
               ), null, 0, 0), 14);
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
                  BinarySpacePartitioner.GetSplitPosition(size, minSize, splitCenterDeviationPercent, new System.Random(10)),
                  Throws.TypeOf<Exception>());

        }
        else
        {
            // Goal is to produce a split that contains a room of min size (test the behaviour)
            int splitPos = BinarySpacePartitioner.GetSplitPosition(size, minSize, splitCenterDeviationPercent, new System.Random(10));

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
    public void SplitSpaceTest((SpaceNode spaceToSplit, Vector3Int minSpaceDim, Vector3 splitCenterDeviation) testData)
    {
        /// This function will test if the behaviour of the SplitSpace function
        /// The core aspect of the function is to produce the correct orrientation based on the inputs
        /// Therefore only the SplitPosition of the nodes will be tested, as well as null outputs

        // set Random seed for Testing 

        Random.State orignalState = Random.state;
        Random.InitState(10);

        SpaceNode testNode = testData.spaceToSplit;

        Vector3Int minSpaceDim = testData.minSpaceDim;
        Vector3 splitCenterDeviation = testData.splitCenterDeviation;

        int sizeX = testNode.Bounds.size.x;
        int sizeZ = testNode.Bounds.size.z;

        SpaceNode node1, node2;

        if (sizeX > 2*minSpaceDim.x && sizeZ > 2 * minSpaceDim.y)
        {
            (node1, node2) = BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation, new System.Random(10));
        }
        else if (sizeX > 2 * minSpaceDim.x)
        {
            (node1, node2) = BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation, new System.Random(10));

            Assert.True(node1.SplitPosition.Equals(SplitPosition.Left) || node1.SplitPosition.Equals(SplitPosition.Right), "Split Horizontally node1");
            Assert.True(node2.SplitPosition.Equals(SplitPosition.Left) || node2.SplitPosition.Equals(SplitPosition.Right), "Split Horizontally node2");

        } else if (sizeZ > 2 * minSpaceDim.y)
        {
            (node1, node2) = BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation, new System.Random(10));

            Assert.True(node1.SplitPosition.Equals(SplitPosition.Top) || node1.SplitPosition.Equals(SplitPosition.Bottom), "Split Vertically node1");
            Assert.True(node2.SplitPosition.Equals(SplitPosition.Top) || node2.SplitPosition.Equals(SplitPosition.Bottom), "Split Vertically node2");
            
            
        } else
        {
            Assert.That(() => BinarySpacePartitioner.SplitSpace(testNode, minSpaceDim, splitCenterDeviation, new System.Random(10)),
                Throws.TypeOf<Exception>());
        }

        Random.state = orignalState; // reset to original Random State
        
    }

    // Test Case
    public static IEnumerable<(SpaceNode spaceToSplit, Vector3Int minSpaceDim, Vector3 splitCenterDeviation)> SplitSpaceCases()
    {

        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(20, 0, 20)
                            ),
                            null,
                            0,
                            0
                        ),
                        new Vector3Int(4,6,4),
                        new Vector3(1f,1f,1f));

        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(10, 0, 14)
                            ),
                            null,
                            0,
                            0
                        ),
                        new Vector3Int(2, 14, 2),
                        new Vector3(0.5f, 1f, 0.5f));

        yield return (new SpaceNode(
                    new BoundsInt(
                        new Vector3Int(0, 0, 0),
                        new Vector3Int(13, 0, 25)
                    ),
                    null,
                    0,
                    0
                ),
                new Vector3Int(13, 2, 2),
                new Vector3(1f, 0.5f, 0.5f));

        yield return (new SpaceNode(
                   new BoundsInt(
                       new Vector3Int(0, 0, 0),
                       new Vector3Int(13, 0, 13)
                   ),
                   null,
                   0,
                   0
               ),
               new Vector3Int(13, 13, 13),
               new Vector3(1f, 1f, 0.5f));
    }

    #endregion

    #region GetSplitableAxisTest

    // Test
    [Test]
    [TestCaseSource(nameof(GetSplitableAxisCases))]
    public void GetSplitableAxisTest((SpaceNode spaceToSplit, Vector3Int minSpaceDim) testData)
    {
        SpaceNode testNode = testData.spaceToSplit;

        Vector3Int minSpaceDim = testData.minSpaceDim;

        bool3 testAxis = BinarySpacePartitioner.GetSplitableAxis(testNode.Bounds, minSpaceDim);

        int sizeX = testNode.Bounds.size.x;
        int sizeZ = testNode.Bounds.size.z;
        int sizeY = testNode.Bounds.size.y;


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

        if (sizeY > 2 * minSpaceDim.z)
        {
            Assert.True(testAxis.y, "splitable along z");
        }
        else
        {
            Assert.False(testAxis.y, "not splitable along z");
        }

    }

    // Test Case
    public static IEnumerable<(SpaceNode spaceToSplit, Vector3Int minSpaceDim)> GetSplitableAxisCases()
    {

        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(10, 0, 10)
                            ),
                            null,
                            0,
                            0
                        ),
                        new Vector3Int(2, 2, 2));
        yield return (new SpaceNode(
                                new BoundsInt(
                                    new Vector3Int(0, 0, 0),
                                    new Vector3Int(5, 0, 1)
                                ),
                                null,
                                0,
                                0
                            ),
                            new Vector3Int(2, 1, 1));
        yield return (new SpaceNode(
                            new BoundsInt(
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(2, 0, 6)
                            ),
                            null,
                            0,
                            0
                        ),
                        new Vector3Int(2, 3, 4));
    }
    #endregion

    #region AddSplitSpaces
    [Test]
    public void AddSplitSpacesTest()
    {
        BinarySpacePartitioner testPartitioner = new BinarySpacePartitioner(10, 10, 10);

        // Create empty ndoes
        var testNode1 = new SpaceNode(new BoundsInt(), null, 0, 0);
        var testNode2 = new SpaceNode(new BoundsInt(), null, 0, 0);

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
