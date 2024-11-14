using System;
using System.Collections.Generic;
using UnityEngine;


namespace tutorialGenerator
{
    internal class RoomGenerator
    {
        private int maxIterations;
        private int roomWidthMin;
        private int roomLengthMin;

        public RoomGenerator(int maxIterations, int roomWidthMin, int roomLengthMin)
        {
            this.maxIterations = maxIterations;
            this.roomWidthMin = roomWidthMin;
            this.roomLengthMin = roomLengthMin;
        }

        public List<RoomNode> GenerateRoomsInSpaces(List<Node> roomSpaces, float roomBottomCornerModifier, float roomTopCorrnerModifier, int roomOffset)
        {

            List<RoomNode> listToReturn = new List<RoomNode>();

            foreach (var space in roomSpaces)
            {

                Vector2Int newBottomLeftPoint = StructureHelper.GenerateBottomLeftCornerBetween(
                    space.BottomLeftAreaCorner, space.TopRightAreaCorner, roomBottomCornerModifier, roomOffset
                    );
                Vector2Int newTopRightPoint = StructureHelper.GenerateTopRightCornerBetween(
                    space.BottomLeftAreaCorner, space.TopRightAreaCorner, roomTopCorrnerModifier, roomOffset
                    );

                space.BottomLeftAreaCorner = newBottomLeftPoint;
                space.TopRightAreaCorner = newTopRightPoint;

                space.BottomRightAreaCorner = new Vector2Int(newTopRightPoint.x, newBottomLeftPoint.y);
                space.TopLeftAreaCorner = new Vector2Int(newBottomLeftPoint.x, newTopRightPoint.y);

                listToReturn.Add((RoomNode)space);

            }

            return listToReturn;
        }
    }
}