using System.Collections;
using System.Collections.Generic;
using dungeonGenerator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BinarySpacePartitionerTest
{
    // A Test behaves as an ordinary method
    [Test]
    [TestCase(10,10)]
    [TestCase(5, 15)]
    [TestCase(21, 3)]
    public void BinarySpacePartitionerInitializationTest(int dungeonWidth, int dungeonLength)
    {
        // Use the Assert class to test conditions

        BinarySpacePartitioner testPartitioner = new BinarySpacePartitioner(dungeonWidth,dungeonLength);
        
        Assert.IsTrue(testPartitioner.RootNode.Bounds.position.Equals(Vector3Int.zero), "RootNode Position");
        Assert.IsTrue(testPartitioner.RootNode.Bounds.size.Equals(new Vector3Int(dungeonWidth,0,dungeonLength)), "RootNode Size");

    }

    [Test]
    [TestCase(0,0,10,10,2)]
    [TestCase(-10, 5, 20, 10, 5)]
    [TestCase(30, -5, 3, 20, 14)]
    public void SplitHorizontallyTest(int PosX, int PosZ,int SizeX, int SizeZ, int hSplitPosition)
    {
        SpaceNode testNode = new SpaceNode(
            new BoundsInt(
                new Vector3Int(PosX, 0, PosZ),
                new Vector3Int(SizeX, 0, SizeZ)
            ), null, 0);

        SpaceNode topNode = null;
        SpaceNode bottomNode = null; 

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

    [Test]
    [TestCase(0, 0, 10, 10, 2)]
    [TestCase(-10, 5, 20, 10, 5)]
    [TestCase(30, -5, 20, 4, 14)]
    public void SplitVerticallyTest(int PosX, int PosZ, int SizeX, int SizeZ, int vSplitPosition)
    {
        SpaceNode testNode = new SpaceNode(
            new BoundsInt(
                new Vector3Int(PosX, 0, PosZ),
                new Vector3Int(SizeX, 0, SizeZ)
            ), null, 0);

        SpaceNode leftNode = null;
        SpaceNode rightNode = null;

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


}
