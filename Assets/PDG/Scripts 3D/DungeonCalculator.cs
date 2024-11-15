using System;
using System.Collections.Generic;
using tutorialGenerator;
using UnityEngine;


using dungeonGenerator;

namespace dungeonGenerator {
    public class DungeonCalculator
    {
        private int dungeonWidth;
        private int dungeonLength;

        public DungeonCalculator(int dungeonWidth, int dungeonLength)
        {
            this.dungeonWidth = dungeonWidth;
            this.dungeonLength = dungeonLength;
        }

        /// <summary>
        /// Calculate the dungeon floor bounds that can be used to generate the dungeon meshes
        /// </summary>

        public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, Vector2 splitCenterDeviationPercent)
        {
            // 1. Generate BSP graph based on minRoomWidth, minRoomLength and maxIterations
            BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
            var bspGraph = bsp.PartitionSpace(maxIterations, roomWidthMin, roomLengthMin, splitCenterDeviationPercent);

            // 2. Extract the leaves, which represent the possible room positions
            var roomSpaces = GraphHelper.GetLeaves(bsp.RootNode);

            // 3. Place rooms within the possible room bounds taking into account room offset
            RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomWidthMin, roomLengthMin);
            var rooms = roomGenerator.PlaceRoomsInSpaces(roomSpaces);

            // 4. Generate the corridors to connect the rooms
            //CorridorGenerator corridorGenerator = new CorridorGenerator();
             
            // 5. Return a list of bounds on which the floor will be (Rooms and Corridors) 
            return new List<Node>(rooms);
        }
    }
}
