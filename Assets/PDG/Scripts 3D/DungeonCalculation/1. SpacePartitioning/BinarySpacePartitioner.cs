using Codice.CM.Client.Differences;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using System;

[assembly: InternalsVisibleTo("EditMode")]

namespace dungeonGenerator
{

    public class BinarySpacePartitioner
    
    {
        // Class Fields
        private SpaceNode rootNode;
        
        internal Queue<SpaceNode> spacesToSplit = new Queue<SpaceNode>(); // internal used for testing purposes
        internal List<SpaceNode> allSpaces = new List<SpaceNode>(); // internal used for testing purposes

        // Getter - Setters
        public SpaceNode RootNode { get => rootNode; set => rootNode = value; }

        private int maxFloors = 3;

        /* <summary>
            Helper Method to perform randomness checks to stop splitting of the space
        </summary> */
        internal static bool hasStoppedSplitting(SpaceNode spaceToSplit, int treeLayerIndexThreshold, float stopSpitChance)
        {
            //return ((spaceToSplit.TreeLayerIndex > treeLayerIndexThreshold) && (stopSpitChance <= Random.value));
            return false;
        }



        /* <summary>
            Instantiate BSP with root node equal to the size of the Dungeon
        </summary> */

        public BinarySpacePartitioner(int dungeonWidth, int dungeonLength, int dungeonHeight)
        {
            // Create the root of the BSP tree equal to the starting space to split ie. dungeonWidth, dungeonHeight
            this.RootNode = new SpaceNode(new BoundsInt(
                Vector3Int.zero,
                new Vector3Int(dungeonWidth, dungeonHeight, dungeonLength)),
                null, 0, 0);
        }

        /* <summary>
            Partitions the rootNode till the maxIterations have been exceeded or no more partitions are possible.
        </summary> */

        public List<SpaceNode> PartitionSpace(int maxIterations, Vector3Int minSpaceDim, Vector3 splitCenterDeviationPercent)
        {
      
            spacesToSplit = new Queue<SpaceNode>(); // initialize queue of spaces to split
            allSpaces = new List<SpaceNode>(); // initialize list of split spaces to return 

            spacesToSplit.Enqueue(RootNode); // add RootNode to be split
            allSpaces.Add(RootNode); // add RootNode to the spaces to return forming the BSP graph

            int iterations = 0; // initialize n iteration to 0

            // while the iterations are not exceeded and there are spaces to split, split the spaces
            while (iterations < maxIterations && spacesToSplit.Count > 0)
            {

                SpaceNode spaceToSplit = spacesToSplit.Dequeue(); // dequeue space to split

                var splitableAxis = GetSplitableAxis(spaceToSplit.Bounds, minSpaceDim);

                // Vars to allow the splitting to have chance to stop at a certain depth 
                // FIX ME: Move to Dungeon Generator 

                float splitStopThreshold = 0.0f;
                int splitStopDepth = 5;
                var hasSplitStopped = (splitStopThreshold < Random.value && spaceToSplit.TreeLayerIndex > splitStopDepth);


                if (
                    (splitableAxis.x || splitableAxis.z || splitableAxis.y) 
                    && !hasStoppedSplitting(spaceToSplit, splitStopDepth, splitStopThreshold)
                ) // only split space if possible to split
                {
                    var splitSpaces = SplitSpace(spaceToSplit, minSpaceDim, splitCenterDeviationPercent); // split the space
                    AddSplitSpaces(spacesToSplit, allSpaces, splitSpaces);
                }

                // Increment the iterations 
                iterations++;

            }

            return allSpaces; // return the spaces that for the BSP tree
        }

        internal void AddSplitSpaces(Queue<SpaceNode> spacesToSplit, List<SpaceNode> allSpaces, (SpaceNode, SpaceNode) splitSpaces)
        {
            // add split rooms to splitSpaces and spaces to Split
            if (splitSpaces.Item1 is not null)
            {
                allSpaces.Add(splitSpaces.Item1);
                spacesToSplit.Enqueue(splitSpaces.Item1);
       

            }

            if(splitSpaces.Item2 is not null)
            {
                allSpaces.Add(splitSpaces.Item2);
                spacesToSplit.Enqueue(splitSpaces.Item2);
            }
        }

        #region Splitting Helper Methods

        internal static bool3 GetSplitableAxis(BoundsInt spaceToSplit, Vector3Int minSpaceDim)
        {
            bool3 splitableAxis = new bool3(false, false, false);

            if(spaceToSplit.size.x > 2 * minSpaceDim.x)
            {
                splitableAxis.x = true;
            }

            if(spaceToSplit.size.z > 2 * minSpaceDim.y)
            {
                splitableAxis.z = true; 
            }

            // TODO: For 3D BSP Implementation

            if (spaceToSplit.size.y > 2 * minSpaceDim.z)
            {
                splitableAxis.y = true;
            }

            return splitableAxis;
        }

        /* <summary>
            Split the space based on their current width and height
        </summary> */

        internal static (SpaceNode, SpaceNode) SplitSpace(SpaceNode spaceToSplit, Vector3Int minSpaceDim, Vector3 splitCenterDeviationPercent)
        {
            // get the width and height of the spaceToSplit 
            Debug.Log(minSpaceDim);

            var splitableAxis = GetSplitableAxis(spaceToSplit.Bounds, minSpaceDim);
            
            // width and height of space 
            var spaceWidth = spaceToSplit.Bounds.size.x;
            var spaceLength = spaceToSplit.Bounds.size.z;
            var spaceHeight = spaceToSplit.Bounds.size.y;

            // check if both width and height are large enough to split
            if (splitableAxis.x && splitableAxis.z && splitableAxis.y)
            {
                // TODO: Add random splitting / more control
                // spaceToSplit.SplitPosition.Equals(SplitPosition.Left) || spaceToSplit.SplitPosition.Equals(SplitPosition.Right)

                var splitValHeight = Random.value;

                Debug.Log($"<color=#00FF14> SPLIT PROBABILITY B4 {(spaceWidth * spaceLength)} </color>");

                // BIAS SPLITTING VERTICALLY FOR LARGER SPACES

                // BIAS FOR SPLITTING VERTICALLY EARLY TO AVOID GENERATION OF TOWERS
                if (splitValHeight < 0.5){

                    //Debug.Log($"<color=#00FF14> SPLIT WITH PROBABILITY {(1 - (minSpaceDim.x * minSpaceDim.y) / (spaceWidth * spaceLength))} </color>");
                    Debug.Log("SPLIT ALONG UP...DOWN");
                    return SplitPerpendicular(spaceToSplit, GetSplitPosition(spaceHeight, minSpaceDim.z, splitCenterDeviationPercent.z)); // TO DO: Develop with Random Positioning
                
                } else {

                    var splitVal = Random.value;

                    if (splitVal < 0.5)
                    {
                        return SplitHorizontally(spaceToSplit, GetSplitPosition(spaceLength, minSpaceDim.y, splitCenterDeviationPercent.y));
                    }

                    else
                    {

                        return SplitVertically(spaceToSplit, GetSplitPosition(spaceWidth, minSpaceDim.x, splitCenterDeviationPercent.x));
                    }
                }


            } else if (splitableAxis.x && splitableAxis.z)
            {
                var splitVal = Random.value;

                if (splitVal < 0.5)
                {
                    return SplitHorizontally(spaceToSplit, GetSplitPosition(spaceLength, minSpaceDim.y, splitCenterDeviationPercent.y));
                }

                else
                {

                    return SplitVertically(spaceToSplit, GetSplitPosition(spaceWidth, minSpaceDim.x, splitCenterDeviationPercent.x));
                }
            }
            else if (splitableAxis.x) // if only width is large enough to split, split vertically
            {
                return SplitVertically(spaceToSplit, GetSplitPosition(spaceWidth, minSpaceDim.x, splitCenterDeviationPercent.x));
            }
            else if (splitableAxis.z)  // if only height is large enough to split, split horizontally
            {
                return SplitHorizontally(spaceToSplit, GetSplitPosition(spaceLength, minSpaceDim.y, splitCenterDeviationPercent.y));

            }
            else if (splitableAxis.y)
            {
                Debug.Log("SPLIT ALONG UPDOWN");
                return SplitPerpendicular(spaceToSplit, GetSplitPosition(spaceHeight, minSpaceDim.z, splitCenterDeviationPercent.z));
            }
            else
            {
                throw new System.Exception("SplitSpace(): Space Cannot be Split Further");
            }

        }

        internal static int GetSplitPosition(int size, int minSize, float splitCenterDeviationPercent)
        {

            int center = size / 2;
            int centerDeviation = (center - minSize);


            if (centerDeviation < 0)
            {
                Debug.Log(size);
                Debug.Log(minSize);
                Debug.Log(centerDeviation);
                throw new System.Exception("getSplitPostion(): Space Cannot be Split Further");
            }


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

            var splitStopThreshold = 0.0f;
            var splitStopDepth = 3;

            if (!hasStoppedSplitting(spaceToSplit, splitStopDepth, splitStopThreshold))
            {
                // set left node 
                leftNode = new SpaceNode(
                    new BoundsInt(spaceToSplit.Bounds.min,
                    new Vector3Int(vSplitPosition, spaceToSplit.Bounds.size.y, spaceToSplit.Bounds.size.z)),
                    spaceToSplit,
                    spaceToSplit.TreeLayerIndex + 1,
                    spaceToSplit.FloorIndex
                );

                leftNode.SplitPosition = SplitPosition.Left;
            } else
            {
                leftNode = null;
            }

            if (!hasStoppedSplitting(spaceToSplit, splitStopDepth, splitStopThreshold))
            {
                //// set right node            
                rightNode = new SpaceNode(
                    new BoundsInt(spaceToSplit.Bounds.min + new Vector3Int(vSplitPosition, 0, 0),
                    new Vector3Int(spaceToSplit.Bounds.size.x - vSplitPosition, spaceToSplit.Bounds.size.y, spaceToSplit.Bounds.size.z)),
                    spaceToSplit,
                    spaceToSplit.TreeLayerIndex + 1,
                    spaceToSplit.FloorIndex

                );

                rightNode.SplitPosition = SplitPosition.Right;
            } else
            {
                rightNode = null;
            }

            //// return left and right nodes

            return ( leftNode, rightNode );

        }

        /* <summary>
            Split the space Horizontally
        </summary> */

        internal static (SpaceNode bottomNode, SpaceNode topNode) SplitHorizontally(SpaceNode spaceToSplit, int hSplitPosition)
        {
            // intialize top and bottom nodes
            SpaceNode topNode, bottomNode;

            var splitStopThreshold = 0.5f;
            var splitStopDepth = 3;

            if (!hasStoppedSplitting(spaceToSplit, splitStopDepth, splitStopThreshold))
            {
                // set bottom node
                topNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min + new Vector3Int(0, 0, hSplitPosition),
                new Vector3Int(spaceToSplit.Bounds.size.x, spaceToSplit.Bounds.size.y, spaceToSplit.Bounds.size.z - hSplitPosition)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1,
                spaceToSplit.FloorIndex
            );

                topNode.SplitPosition = SplitPosition.Top;
            } else
            {
                topNode = null;
            }

            if (!hasStoppedSplitting(spaceToSplit, splitStopDepth, splitStopThreshold)) // dont remove it if the bottom node is the entrance
            {
                // set bottom node
                bottomNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min,
                new Vector3Int(spaceToSplit.Bounds.size.x, spaceToSplit.Bounds.size.y, hSplitPosition)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1,
                spaceToSplit.FloorIndex
            );

                bottomNode.SplitPosition = SplitPosition.Bottom; // TODO: Decouple this for a more general solution
            } else
            {
                bottomNode = null;
            }


            // return the results of the split
            return (bottomNode, topNode);

        }

        /* <summary>
            Split the space Perpendicular
        </summary> */

        internal static (SpaceNode downNode, SpaceNode upNode) SplitPerpendicular(SpaceNode spaceToSplit, int zSplitPosition)
        {
            // intialize top and bottom nodes
            SpaceNode upNode, downNode;

            var splitStopThreshold = 0.0f;
            var splitStopDepth = 3;

            if (!hasStoppedSplitting(spaceToSplit, splitStopDepth, splitStopThreshold))
            {
                // set bottom node
                upNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min + new Vector3Int(0, zSplitPosition, 0),
                new Vector3Int(spaceToSplit.Bounds.size.x, spaceToSplit.Bounds.size.y - zSplitPosition, spaceToSplit.Bounds.size.z)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1,
                spaceToSplit.FloorIndex + 1
            );

                upNode.SplitPosition = SplitPosition.Up;
            } else
            {
                upNode = null;
            }

            if (!hasStoppedSplitting(spaceToSplit, splitStopDepth, splitStopThreshold))
            {
                // set bottom node
                downNode = new SpaceNode(
                new BoundsInt(spaceToSplit.Bounds.min,
                new Vector3Int(spaceToSplit.Bounds.size.x, zSplitPosition, spaceToSplit.Bounds.size.z)),
                spaceToSplit,
                spaceToSplit.TreeLayerIndex + 1,
                spaceToSplit.FloorIndex
            );

                downNode.SplitPosition = SplitPosition.Down; // TODO: Decouple this for a more general solution
            } else
            {
                downNode = null;
            }

            // return the results of the split
            return (downNode, upNode);

        }


        #endregion
    }
}