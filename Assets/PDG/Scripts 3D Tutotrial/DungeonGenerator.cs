using System;
using System.Collections.Generic;
using System.Linq;


namespace tutorialGenerator
{
    public class DungeonGenerator
    {

        RoomNode rootNode;
        List<RoomNode> allSpaceNodes = new List<RoomNode>();


        private int dungeonWidth;
        private int dungeonLength;


        public DungeonGenerator(int dungoenWidth, int dungeonLength)
        {
            this.dungeonWidth = dungoenWidth;
            this.dungeonLength = dungeonLength;
        }

        public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCorrnerModifier, int roomOffset, int corridorWidth)
        {
            BinarySpacePartioner bsp = new BinarySpacePartioner(dungeonWidth, dungeonLength);
            var allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);

            List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeaves(bsp.RootNode);
            RoomGenerator roomGenrator = new RoomGenerator(maxIterations, roomWidthMin, roomLengthMin);
            List<RoomNode> roomList = roomGenrator.GenerateRoomsInSpaces(roomSpaces, roomBottomCornerModifier, roomTopCorrnerModifier, roomOffset);

            CorridorGenerator corridorGenerator = new CorridorGenerator();
            var corridorList = corridorGenerator.CreateCorridor(allNodesCollection, corridorWidth);


            return new List<Node>(roomList).Concat(corridorList).ToList();
        }
    }
}