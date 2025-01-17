using System.Buffers.Text;
using UnityEngine;
using System.Collections.Generic;

namespace dungeonGenerator
{


    public class SpaceNode : Node
    {

        public SpaceNode(BoundsInt Bounds, Node parentNode, int index) : base(parentNode)
        {
            this.Bounds = Bounds;
            this.Parent = parentNode;
            this.TreeLayerIndex = index;
            
        }


    }
}