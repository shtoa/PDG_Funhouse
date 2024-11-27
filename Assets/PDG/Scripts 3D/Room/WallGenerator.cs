//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using dungeonGenerator;

//namespace dungeonGenerator
//{
//    public class WallGenerator : MonoBehaviour
//    {
//        WallCalculator wallCalculator;
//        WallCreator wallCreator;


//        List<Node> roomSpaces;
//        public WallGenerator(List<Node> RoomSpaces)
//        {
//            roomSpaces = RoomSpaces;
//        }

//        private void Start()
//        {
//           List<BoundsInt> Walls = wallCalculator.calculate(roomSpaces);
//           wallCreator.create(Walls);
//        }


//    }

   
//}