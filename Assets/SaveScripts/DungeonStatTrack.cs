using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dungeonGenerator;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.Profiling;
using System.Text;
using static dungeonGenerator.DungeonStatTrack;

namespace dungeonGenerator {
    public static class DungeonStatTrack
    {

        public static float GameTime = 0;

        [System.Serializable]
        public class RoomStats
        {
            public Vector3Int roomPosition;
            public Vector3Int roomSize;
            public int numberOfConnection;
            public int distanceFromStart;
            public string roomType;
            public string corridorType;
            public string roomMainSplit;
            public List<string> doorConnections;
            public List<string> verticalConnections;
            public int roomID;
        }
        public static List<RoomStats> getRoomStats()
        {
            List<RoomStats> roomStatsList = new List<RoomStats>();
            foreach(var room in DungeonStatTrack.roomList)
            {
                RoomStats roomStats = new RoomStats();

                roomStats.roomPosition = room.Bounds.position;
                roomStats.roomSize = room.Bounds.size;
                roomStats.numberOfConnection = room.ConnectionsList.Count;
                roomStats.distanceFromStart = room.ConnectionDepthIndex;
                roomStats.roomType = room.RoomType.ToString();
                roomStats.corridorType = room.CorridorType.ToString();
                roomStats.roomMainSplit = room.SplitPosition.ToString();

                List<string> doorConnections = new List<string>();
                foreach(var placement in room.DoorPlacements)
                {
                    doorConnections.Add(placement.PositionType.ToString());
                }

                roomStats.doorConnections = doorConnections; // convert to list of split positions

                List<string> verticalConnections = new List<string>();
                foreach (var placement in room.HolePlacements)
                {
                    verticalConnections.Add(placement.PositionType.ToString());
                }

                roomStats.verticalConnections = verticalConnections; // convert to list of split positions

                roomStats.roomID = room.NodeID; 

                roomStatsList.Add(roomStats);
            }

            return roomStatsList;
        }

        [System.Serializable]
        public class DungeonStats
        {
            public List<RoomStats> roomStats;
            public long GenerationTime;
            public float DungeonCompletionTime;
        }
        public static DungeonStats getDungeonStats()
        {

            DungeonStats dungeonStats = new DungeonStats();
            dungeonStats.roomStats = getRoomStats();
            dungeonStats.GenerationTime = DungeonStatTrack.GenerationTime;
            dungeonStats.DungeonCompletionTime = DungeonStatTrack.DungeonCompletionTime;

            var dungeonStatsJson = JsonUtility.ToJson(dungeonStats);
            return dungeonStats;
        }

        public static float DungeonCompletionTime;
        public static long GenerationTime;

        [System.Serializable]
        public class DeviceInfo
        {
            public string Model;
            public string GraphicsCard;
            public int VRAM;
            public string OS;
            public string CPU;
            public string Resolution;
        }
        public static DeviceInfo getDeviceInfo()
        {

            DeviceInfo deviceInfo = new DeviceInfo();

            deviceInfo.Model = SystemInfo.deviceModel;
            deviceInfo.GraphicsCard = SystemInfo.graphicsDeviceName;
            deviceInfo.VRAM = SystemInfo.graphicsMemorySize/1000;
            deviceInfo.OS = SystemInfo.operatingSystem;
            deviceInfo.CPU = SystemInfo.processorType;
            deviceInfo.Resolution = Screen.currentResolution.ToString();

            return deviceInfo;

        }


        // using profiler
        public static ProfilerRecorder triangleCountProfiler;
        public static ProfilerRecorder systemMemoryUsedProfiler;
        public static ProfilerRecorder mainThreadTimeProfiler;
        public static ProfilerRecorder drawCallsCountProfiler;

        [System.Serializable]
        public class TrackedPerformanceStats
        {
            public List<int> fps = new List<int>();
            public List<long> triangles = new List<long>();
            public List<long> systemMemoryUsed = new List<long>();
            public List<long> drawCallsCount = new List<long>();
            public List<int> roomIDsVisited = new List<int>();
            public List<float> timeStampsVisited = new List<float>();
            public List<float> timeStampsCollected = new List<float>();
        }

        public static TrackedPerformanceStats trackedPerformanceStats = new TrackedPerformanceStats();

        public static void trackPerformanceStats()
        {
            trackedPerformanceStats = new TrackedPerformanceStats();

            triangleCountProfiler = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            systemMemoryUsedProfiler = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            drawCallsCountProfiler = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");

        }

        public static IEnumerator updateTrackPerformanceStats()
        {

            while (GameManager.Instance.gameState != GameManager.GameState.Ended && Application.isPlaying) {
                
                trackedPerformanceStats.triangles.Add(triangleCountProfiler.LastValue);
                trackedPerformanceStats.systemMemoryUsed.Add(systemMemoryUsedProfiler.LastValue/(1024*1024));
                trackedPerformanceStats.drawCallsCount.Add(drawCallsCountProfiler.LastValue/(1024*1024));
                
                trackedPerformanceStats.fps.Add((int)(1f / Time.unscaledDeltaTime));

                yield return new WaitForSeconds(10);
            }
        }

        public static void disposeTrackersPerformanceStats()
        {

            triangleCountProfiler.Dispose();
            systemMemoryUsedProfiler.Dispose();
            drawCallsCountProfiler.Dispose();

        }
        public static TrackedPerformanceStats getTrackPerformanceStats()
        {
            return trackedPerformanceStats;
        }

            [System.Serializable]
        public class DungeonTestData
        {
            public DeviceInfo deviceInfo;
            public DungeonConfig dungeonConfig;
            public DungeonStats dungeonStats;
            public TrackedPerformanceStats trackedPerformanceData;
        }

        public static List<Node> roomList;
        public static void saveDungeonTestData()
        {
            DungeonTestData dungeonTestData = new DungeonTestData();
            dungeonTestData.dungeonStats = getDungeonStats();
            dungeonTestData.deviceInfo = getDeviceInfo();
            dungeonTestData.trackedPerformanceData = getTrackPerformanceStats();
            dungeonTestData.dungeonConfig = DungeonStatTrack.dungeonConfig;

            SaveManager.Save(dungeonTestData, "dungeonTest");

        }

        public static void addCollectedTime(CollectableType t)
        {
            DungeonStatTrack.trackedPerformanceStats.timeStampsCollected.Add(DungeonStatTrack.GameTime);
        }

        public static DungeonConfig dungeonConfig = new DungeonConfig();



    }

    
}
