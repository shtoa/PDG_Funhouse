using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;


[assembly: InternalsVisibleTo("EditMode")]

namespace dungeonGenerator
{

    public class BinarySpacePartitioner
    
    {
        // Class Fields
        private SpaceNode rootNode;

        // Getter - Setters
        public SpaceNode RootNode { get => rootNode; set => rootNode = value; }

        /* <summary>
            Instantiate BSP with root node equal to the size of the Dungeon
        </summary> */

        public BinarySpacePartitioner(int dungeonWidth, int dungeonHeight)
        {
            // Create the root of the BSP tree equal to the starting space to split ie. dungeonWidth, dungeonHeight
            this.RootNode = new SpaceNode(new BoundsInt(
                Vector3Int.zero,
                new Vector3Int(dungeonWidth, 0, dungeonHeight)),
                null, 0);
        }

        /* <summary>
            Partitions the rootNode till the maxiterations have been exceeded or no more partitions are possible.
        </summary> */

        public List<SpaceNode> PartitionSpace(int maxIterations, Vector2Int minSpaceDim, Vector2 splitCenterDeviationPercent)
        {
      
            Queue<SpaceNode> spacesToSplit = new Queue<SpaceNode>(); // initialize queue of spaces to split
            List<SpaceNode> allSpaces = new List<SpaceNode>(); // initialize list of split spaces to return 

            spacesToSplit.Enqueue(RootNode); // add RootNode to be split
            allSpaces.Add(RootNode); // add RootNode to the spaces to return forming the BSP graph

            int iterations = 0; // initialize n iteration to 0


            // while the iterations are not exceeded and there are spaces to split, split the spaces
            while (iterations < maxIterations && spacesToSplit.Count > 0)
            {

                SpaceNode spaceToSplit = spacesToSplit.Dequeue(); // dequeue space to split
                var splitSpaces = SplitSpace(spaceToSplit, minSpaceDim, splitCenterDeviationPercent); // split the space

                addSplitSpaces(spacesToSplit, allSpaces, splitSpaces);

                // Increment the iterations 
                iterations++;

            }

            return allSpaces; // return the spaces that for the BSP tree
        }

        internal void addSplitSpaces(Queue<SpaceNode> spacesToSplit, List<SpaceNode> allSpaces, (SpaceNode, SpaceNode) splitSpaces)
        {
            // add split rooms to splitSpaces and spaces to Split
            if (splitSpaces.Item1 is not null && splitSpaces.Item2 is not null)
            {
                allSpaces.Add(splitSpaces.Item1);
                allSpaces.Add(splitSpaces.Item2);

                spacesToSplit.Enqueue(splitSpaces.Item1);
                spacesToSplit.Enqueue(splitSpaces.Item2);

            }
        }

        #region Splitting Helper Methods

        /* <summary>
            Split the space based on their current width and height
        </summary> */

        internal static (SpaceNode, SpaceNode) SplitSpace(SpaceNode spaceToSplit, Vector2Int minSpaceDim, Vector2 splitCenterDeviationPercent)
        {
            // get the width and height of the spaceToSplit 
            // TODO: Change to Vector2Int
            int spaceWidth = spaceToSplit.Bounds.size.x;
            int spaceHeight = spaceToSplit.Bounds.size.z;


            // TODO: Check if this returns expected behaviour
            Vector2Int splitPositions = new Vector2Int(
                getSplitPosition(spaceWidth, minSpaceDim.x, splitCenterDeviationPercent.x),
                getSplitPosition(spaceHeight, minSpaceDim.y, splitCenterDeviationPercent.y)

            );


            // check if both width and height are large enough to split
            if (spaceWidth > 2 * minSpaceDim.x && spaceHeight > 2 * minSpaceDim.y)
            {
                // TODO: Add random splitting / more control
                if (spaceToSplit.SplitPosition.Equals(SplitPosition.Left) || spaceToSplit.SplitPosition.Equals(SplitPosition.Right))
                {
                    return SplitHorizontally(spaceToSplit, splitPositions.y);
                }
                else
                {
                    return SplitVertically(spaceToSplit, splitPositions.x);
                }

            }
            else if (spaceWidth > 2 * minSpaceDim.x) // if only width is large enough to split, split vertically
            {
                return SplitVertically(spaceToSplit, splitPositions.x);
            }
            else if (spaceHeight > 2 * minSpaceDim.y)  // if only height is large enough to split, split horizontally
            {
                return SplitHorizontally(spaceToSplit, splitPositions.y);
            }

            return (null, null); // FIXME: Might Pose Future Errors

        }

        internal static int getSplitPosition(int size, int minSize, float splitCenterDeviationPercent)
        {
            int center = size / 2;
            int centerDeviation = (center - minSize);

            int splitPosition = (int)Random.Range(center - centerDeviation * splitCenterDeviationPercent,
                                                   center + centerDeviation * splitCenterDeviationPercent);

            return splitPosition;
        }

        /* <summary>
            Split the space Vertically
        </summary> */

        internal static (SpaceNode leftNode, SpaceNode rightNode) SplitVertically(SpaceNode spaceToSplit, int vSplitPosition)
        {
            SpaceNode leftNode, rightNode;

            // set left node 
            leftNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min,
                new Vector3Int(vSplitPosition, spaceToSplit.Bounds.size.y, spaceToSplit.Bounds.size.z)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1
            );

            leftNode.SplitPosition = SplitPosition.Left;

            // set right node            
            rightNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min + new Vector3Int(vSplitPosition,0,0),
                new Vector3Int(spaceToSplit.Bounds.size.x - vSplitPosition, spaceToSplit.Bounds.size.y, spaceToSplit.Bounds.size.z)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1

            );

            rightNode.SplitPosition = SplitPosition.Right;
            
            // return left and right nodes
            return( leftNode, rightNode );

        }

        internal static (SpaceNode bottomNode, SpaceNode topNode) SplitHorizontally(SpaceNode spaceToSplit, int hSplitPosition)
        {
            // intialize top and bottom nodes
            SpaceNode topNode, bottomNode;
        
            // set bottom node
            topNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min + new Vector3Int(0, 0, hSplitPosition),
                new Vector3Int(spaceToSplit.Bounds.size.x, spaceToSplit.Bounds.size.y, spaceToSplit.Bounds.size.z - hSplitPosition)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1
            );

            topNode.SplitPosition = SplitPosition.Top;

            // set bottom node
            bottomNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min,
                new Vector3Int(spaceToSplit.Bounds.size.x, spaceToSplit.Bounds.size.y, hSplitPosition)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1
            );

            bottomNode.SplitPosition = SplitPosition.Bottom; // TODO: Decouple this for a more general solution


            // return the results of the split
            return (bottomNode, topNode);

        }
        #endregion
    }
}