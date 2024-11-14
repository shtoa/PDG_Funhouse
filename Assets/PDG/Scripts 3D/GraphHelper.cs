using System.Collections.Generic;
using UnityEngine.UIElements;

namespace dungeonGenerator
{
    public class GraphHelper
    {
        static public List<Node> GetLeaves(Node parentNode)
        {
            Queue<Node> nodesToCheck = new Queue<Node>();
            List<Node> leaves = new List<Node>();

            // if rootnode is passed
            if (parentNode.ChildrenNodeList.Count == 0)
            {
                return new List<Node> { parentNode };
            }

            // add all parents children to check for leaves
            foreach (var child in parentNode.ChildrenNodeList)
            {
                nodesToCheck.Enqueue(child);
            }
            
            while (nodesToCheck.Count > 0)
            {
                Node currentNode = nodesToCheck.Dequeue();

                if(currentNode.ChildrenNodeList.Count > 0)
                {
                    foreach (var child in currentNode.ChildrenNodeList)
                    {
                        nodesToCheck.Enqueue(child);
                    }
                }
                else
                {
                    leaves.Add(currentNode);
                }
            }

            return leaves;

        }

    }
}