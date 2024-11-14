using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


using dungeonGenerator;
using System;
using tutorialGenerator;

namespace dungeonGenerator
{
    public class BinarySpacePartitioner
    {
        private int dungeonWidth;
        private int dungeonHeight;
        private SpaceNode rootNode;

        public SpaceNode RootNode { get => rootNode; set => rootNode = value; }

        public BinarySpacePartitioner(int dungeonWidth, int dungeonHeight)
        {
            this.dungeonWidth = dungeonWidth;
            this.dungeonHeight = dungeonHeight;
            this.RootNode = new SpaceNode(new BoundsInt(
                Vector3Int.zero,
                new Vector3Int(dungeonWidth, 0, dungeonHeight)),
                null, 0);
        }

        public List<SpaceNode> PartitionSpace(int maxIterations, int roomWidthMin, int roomLengthMin, float splitCenterDeviationPercent)
        {
      
            Queue<SpaceNode> spacesToSplit = new Queue<SpaceNode>(); // initialize queue of spaces to split
            List<SpaceNode> allSpaces = new List<SpaceNode>(); // initialize list of split spaces to return 


            spacesToSplit.Enqueue(RootNode);
            allSpaces.Add(RootNode);

            int iterations = 0;

            while (iterations < maxIterations && spacesToSplit.Count > 0) { 
            
                iterations++;
                SpaceNode currentSpace = spacesToSplit.Dequeue();
                SplitSpaces(currentSpace, allSpaces, roomWidthMin, roomLengthMin, spacesToSplit, splitCenterDeviationPercent); // start splitting spaces

            }

            return allSpaces;
        }

        #region Splitting Helper Methods

        /// <summary>
        /// Split the rooms based on their current width and height
        /// </summary>

        private void SplitSpaces(SpaceNode currentSpace, List<SpaceNode> splitSpaces, int roomWidthMin, int roomLengthMin, Queue<SpaceNode> spacesToSplit, float splitCenterDeviationPercent)
        {
            int spaceWidth = currentSpace.Bounds.size.x;
            int spaceHeight = currentSpace.Bounds.size.z;

            if (spaceWidth > 2*roomWidthMin && spaceHeight > 2 * roomLengthMin)
            {
                // if both width and height are enough to split split randomly
                if(currentSpace.SplitPosition.Equals(SplitPosition.Left) || currentSpace.SplitPosition.Equals(SplitPosition.Right)) // Random.Range(0,1) == 0
                {
                    splitHorizontally(currentSpace, splitSpaces, roomLengthMin, spacesToSplit, splitCenterDeviationPercent);
                } else
                {
                    splitVertically(currentSpace, splitSpaces, roomWidthMin, spacesToSplit, splitCenterDeviationPercent);
                }

            } else if (spaceWidth > 2 * roomWidthMin)
            {
                // if only width is large enough to split, split vertically
                splitVertically(currentSpace, splitSpaces, roomWidthMin, spacesToSplit, splitCenterDeviationPercent);
            } else if (spaceHeight > 2 * roomLengthMin)
            {
                // if only height is large enough to split, split horizontally
                splitHorizontally(currentSpace, splitSpaces, roomLengthMin, spacesToSplit, splitCenterDeviationPercent);
            } 
        }

       
        private void splitVertically(SpaceNode currentSpace, List<SpaceNode> splitSpaces, int roomWidthMin, Queue<SpaceNode> spacesToSplit, float splitCenterDeviationPercent)
        {
            SpaceNode leftNode, rightNode;

            //int vSplitPosition = Random.Range(roomWidthMin, currentSpace.Bounds.size.x - roomWidthMin);
            float center = currentSpace.Bounds.size.x / 2f;
            float centerDeviation = (center - roomWidthMin);

            int vSplitPosition = (int)Random.Range(center - centerDeviation*splitCenterDeviationPercent, 
                                                   center + centerDeviation*splitCenterDeviationPercent); 

            leftNode = new SpaceNode(
                new BoundsInt(currentSpace.Bounds.min,
                new Vector3Int(vSplitPosition, currentSpace.Bounds.size.y, currentSpace.Bounds.size.z)),
                currentSpace,
                currentSpace.TreeLayerIndex + 1
            );

            rightNode = new SpaceNode(
                new BoundsInt(currentSpace.Bounds.min + new Vector3Int(vSplitPosition,0,0),
                new Vector3Int(currentSpace.Bounds.size.x - vSplitPosition, currentSpace.Bounds.size.y, currentSpace.Bounds.size.z)),
                currentSpace,
                currentSpace.TreeLayerIndex + 1

            );

            leftNode.SplitPosition = SplitPosition.Left; 
            rightNode.SplitPosition = SplitPosition.Right;

            // can separate further
            spacesToSplit.Enqueue( leftNode );
            spacesToSplit.Enqueue( rightNode );

            splitSpaces.Add(leftNode );
            splitSpaces.Add(rightNode);

        }

        private void splitHorizontally(SpaceNode currentSpace, List<SpaceNode> splitSpaces, int roomLengthMin, Queue<SpaceNode> spacesToSplit, float splitCenterDeviationPercent)
        {
            SpaceNode topNode, bottomNode;

            //int hSplitPosition = Random.Range(roomLengthMin, currentSpace.Bounds.size.z - roomLengthMin);
            int center = currentSpace.Bounds.size.z / 2;
            float centerDeviation = (center - roomLengthMin);
            int hSplitPosition = (int)Random.Range(center - centerDeviation*splitCenterDeviationPercent,
                                                   center + centerDeviation*splitCenterDeviationPercent);


            topNode = new SpaceNode(
                new BoundsInt(currentSpace.Bounds.min,
                new Vector3Int(currentSpace.Bounds.size.x, currentSpace.Bounds.size.y, hSplitPosition)),
                currentSpace,
                currentSpace.TreeLayerIndex + 1
            );

            bottomNode = new SpaceNode(
                new BoundsInt(currentSpace.Bounds.min + new Vector3Int(0, 0, hSplitPosition),
                new Vector3Int(currentSpace.Bounds.size.x, currentSpace.Bounds.size.y, currentSpace.Bounds.size.z - hSplitPosition)),
                currentSpace,
                currentSpace.TreeLayerIndex + 1
            );

            topNode.SplitPosition = SplitPosition.Top;
            bottomNode.SplitPosition = SplitPosition.Bottom;

            // can separate further
            spacesToSplit.Enqueue(topNode);
            spacesToSplit.Enqueue(bottomNode);

            splitSpaces.Add(topNode);
            splitSpaces.Add(bottomNode);

        }
        #endregion
    }
}