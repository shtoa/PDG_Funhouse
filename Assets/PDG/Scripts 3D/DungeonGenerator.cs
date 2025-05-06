using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.Windows;
using Unity.Loading;
using File = System.IO.File;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using static dungeonGenerator.DungeonGenerator;
using System.Threading.Tasks;
using UnityEngine.Pool;
using tutorialGenerator;
using UnityEditor.Rendering;
using Unity.EditorCoroutines.Editor;

namespace dungeonGenerator

{

    public class DungeonGenerator : MonoBehaviour
    {

        //[HideInInspector]

        public int maxIterations;

        [Header("Dungeon Properties")]
        public BoundsInt dungeonBounds; // TODO: Find Way to Visualize the bounds inside a mini Window

        [Header("Split Properties")]
        public Vector3 splitCenterDeviation;

        [Header("Room Properties")]
        public BoundsInt roomBoundsMin;
        public Vector3Int roomOffsetMin;

        public Vector3Int roomPlacementRandomness;

        [Header("Seed")]
        public bool seededGenerationEnabled;
        public int randomSeed;

        // add checking if name already exists
        [Header("Config Saving")]
        public string configName = "dungeonConfig";
        public TextAsset dungeonConfig;

        private List<BoundsInt> wallBounds = new List<BoundsInt>();
        private List<BoundsInt> doorBounds = new List<BoundsInt>();

        [Header("Editor Generation Settings")]
        public bool isEditorBatched = true;


        [Header("Testing")]
        public int testCount = 1;

        [SerializeField]
        List<TextAsset> dungeonTestConfigs;


        [SerializeField]
        [Header("Object Pooling")]
        public GameObject WindowSpawner;
        public GameObject WallSpawner;
        public GameObject LightSpawner;


        // [Header("Corridor Properties")]
        [HideInInspector]
        public int corridorWidth = 1;

        [HideInInspector]
        public int corridorHeight = 2;

        [HideInInspector]
        public int corridorWidthAndWall;

        // [Header("Wall Properties")]
        [HideInInspector]
        public int wallThickness = 1;


        public List<Node> roomList { get; private set; }
        public GameObject startRoom { get; private set; }
        public Action OnDungeonFinished { get; internal set; }

        private static Dictionary<CollectableType, int> total = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);


        public event Action OnDungeonRegenerated;

        public void RegenerateDungeon()
        {
            //Debug.unityLogger.logEnabled = false;
            if (dungeonGenerated == null || dungeonGenerated.Task.IsCompleted)
            {
                DeleteDungeon();
                corridorWidthAndWall = corridorWidth + 2 * wallThickness;
                GenerateDungeon();
                OnDungeonRegenerated?.Invoke();
            }
        }

        // private List<WindowAsset> allWindows = new List<WindowAsset>();
        public void DeleteDungeon()
        {
            // loop over room objects return all spawned assets
            WindowSpawner.GetComponent<WindowSpawner>()._windowPool.ResetPrevInstances();
            WallSpawner.GetComponent<WallSpawner>()._wallPool.ResetPrevInstances();
            LightSpawner.GetComponent<LightSpawner>()._lightPool.ResetPrevInstances();
            
            // destroy room objects
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }

            Node.curNodeID = 0;
        }

        public void DeletePreviousDungeon()
        {
            // loop over room objects return all spawned assets
            WindowSpawner.GetComponent<WindowSpawner>()._windowPool.ResetPrevInstances();
            WallSpawner.GetComponent<WallSpawner>()._wallPool.ResetPrevInstances();
            LightSpawner.GetComponent<LightSpawner>()._lightPool.ResetPrevInstances();


            // destroy room objects
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).tag == "toDelete")
                {
                    GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }

            Node.curNodeID = 0;
        }

        public void SaveDungeon()
        {
            DungeonConfig dungeonConfigSave = new DungeonConfig();

            setConfigFromTo(this, dungeonConfigSave); // copies config from 
            SaveManager.Save(dungeonConfigSave, configName);
        }

        public void LoadDungeon()
        {
            DungeonConfig dungeonConfigLoad = SaveManager.Load(dungeonConfig);
            setConfigFromTo(dungeonConfigLoad, this);
        }

        public void GetInfo()
        {
            DungeonStatTrack.getDeviceInfo();     
        }

        public void setConfigFromTo(object from, object to)
        {

            foreach (var field in typeof(DungeonConfig).GetFields())
            {
                var sourceField = from.GetType().GetField(field.Name);
                var targetField = to.GetType().GetField(field.Name);

                //Debug.Log(field.Name);
                //Debug.Log(sourceField.Name);

                if (sourceField != null) //  && sourceProp.CanRead && sourceProp.CanWrite
                {
                    var value = sourceField.GetValue(from);
                    targetField.SetValue(to, value);
                }

            }
        }
        [System.Serializable]
        public class TestDungeonGenerationWrapper
        {
            public List<AllRoomStatsWrapper> allRoomStats = new List<AllRoomStatsWrapper>();
        }
        [System.Serializable]
        public class AllRoomStatsWrapper
        {
            public List<DungeonStatTrack.RoomStats> roomStats = new List<DungeonStatTrack.RoomStats>();
        }


        public void TestDungeonGeneration(int testCount)
        {
            foreach (TextAsset config in dungeonTestConfigs) {


                DungeonConfig testDungeonConfig = SaveManager.Load(config);

                setConfigFromTo(testDungeonConfig, this);

                TestDungeonGenerationWrapper testStats = new TestDungeonGenerationWrapper();
                var curTest = 1;
                while (testCount >= curTest) {

                    AllRoomStatsWrapper allRoomStats = new AllRoomStatsWrapper();

                

                    if (!seededGenerationEnabled)
                    {
                        randomSeed = Random.Range(int.MinValue, int.MaxValue);
                    }

                    Random.InitState(randomSeed); // set generation seed

                    DungeonCalculator calculator = new DungeonCalculator(dungeonBounds, new System.Random(randomSeed));

                    // TODO: Make objects for Room Properties, Wall Properties, Corridor Properties to pass down
                    roomList = calculator.CalculateDungeon(maxIterations,
                                                           roomBoundsMin,
                                                           splitCenterDeviation,
                                                           corridorWidthAndWall,
                                                           wallThickness,
                                                           roomOffsetMin,
                                                           corridorHeight);

                    InitializeStartAndEnd(calculator.RoomSpaces);
                    curTest++;

                    allRoomStats.roomStats = DungeonStatTrack.getRoomStats(roomList);
                    testStats.allRoomStats.Add(allRoomStats);
                    Debug.Log($"Running Test {testCount}");


                    EditorUtility.DisplayProgressBar("Testing Dungeon Generation", $"Doing Test: {curTest}", (float)curTest / (float)testCount);
                }
                EditorUtility.ClearProgressBar();


                SaveManager.Save(testStats, $"allRoomStats_{config.name}");
            }

        }
        private IEnumerator GenerateDungeonCoroutine(TaskCompletionSource<bool> dungeonGenerated)
        {
            GenerateAsync(dungeonGenerated, 0f, randomSeed, Application.isPlaying || isEditorBatched);//Application.isPlaying);

            yield return new WaitUntil(() => dungeonGenerated.Task.IsCompleted);
  
        }
        private TaskCompletionSource<bool> dungeonGenerated;
        private void GenerateDungeon()
        {

            if (dungeonGenerated == null || dungeonGenerated.Task.IsCompleted)
            {
                if (!seededGenerationEnabled)
                {
                    randomSeed = Random.Range(int.MinValue, int.MaxValue);
                }


                Debug.Log("Generating Dungeon");
                dungeonGenerated = new TaskCompletionSource<bool>();

                if (!Application.isPlaying)
                    EditorCoroutineUtility.StartCoroutineOwnerless(GenerateDungeonCoroutine(dungeonGenerated));
                
                else StartCoroutine(GenerateDungeonCoroutine(dungeonGenerated));
                
            }
            //Stopwatch st = new Stopwatch();
            //st.Start();

        


            //Random.InitState(randomSeed); // set generation seed

            //DungeonCalculator calculator = new DungeonCalculator(dungeonBounds, new System.Random(randomSeed));

            //// TODO: Make objects for Room Properties, Wall Properties, Corridor Properties to pass down
            //roomList = calculator.CalculateDungeon(maxIterations, 
            //                                       roomBoundsMin, 
            //                                       splitCenterDeviation, 
            //                                       corridorWidthAndWall, 
            //                                       wallThickness, 
            //                                       roomOffsetMin, 
            //                                       corridorHeight);

            //if (roomList.Count == 0)
            //{
            //    this.RegenerateDungeon();
            //    return;
            //}


            //InitializeStartAndEnd(calculator.RoomSpaces);

            //DungeonDecorator decorator = GetComponent<DungeonDecorator>();
            //decorator.roomGenerator = new RoomGenerator(roomList, this.gameObject);
            //decorator.roomGenerator.GenerateRooms(roomList);

            //checkDungeonConnections(roomList);

            //st.Stop();

            //DungeonStatTrack.roomList = roomList;

            //setConfigFromTo(this, DungeonStatTrack.dungeonConfig);

            //Debug.Log($"Generation Took {st.ElapsedMilliseconds} Milliseconds");
            //DungeonStatTrack.GenerationTime = st.ElapsedMilliseconds;
        }

        private void checkDungeonConnections(List<Node> roomList)
        {
            foreach (Node room in roomList)
            {
                if(room.ConnectionsList.Count() == 0 && room.RoomType != RoomType.Corridor)
                {
                    throw new Exception("Not All Rooms Connected");
                }
            }
        }

        private void InitializeStartAndEnd(List<Node> roomSpaces)
        {
            // Pick Start and End Rooms
            var startAndEnd = GraphHelper.ChooseStartAndEnd(roomSpaces); // change the data structure here


            // get edge rooms  
            // TODO: MOVE THIS INTO SEPARATE FUNCTION
            var deadEnds = GraphHelper.GetLeaves(startAndEnd[0], false); // does not find unique dead ends due to loops
            int deadEndsCount = 0; 

            foreach (var deadEnd in deadEnds.Distinct())
            {
                if (deadEnd.Bounds.position != startAndEnd[0].Bounds.position && deadEnd.Bounds.position != startAndEnd[1].Bounds.position && deadEnd.ConnectionsList.Count() == 1)
                {
                    deadEnd.RoomType = RoomType.DeadEnd;
                    deadEndsCount++;
                } 
            }
            
            Debug.Log($"DeadEnd Count: {deadEndsCount}");

            // TODO: Refactor this to an event 
            total[CollectableType.cylinder] = 1;
            total[CollectableType.sphere] = deadEndsCount; // -1
            total[CollectableType.cube] = roomSpaces.Count - 2 - total[CollectableType.sphere];

            saveDungeonConfig();
            GameManager.Instance.total = total;
            GameManager.Instance.numCollected = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);
        }

        void saveDungeonConfig()
        {
            
            //Debug.Log("Save Config");
            string savePath = Application.dataPath + "/Save/DungeonConfig.txt";
            File.WriteAllText(savePath, 
                "cylinder"+","+ total[CollectableType.cylinder]+"\n"+
                "sphere" + "," + total[CollectableType.sphere] + "\n"+
                "cube" + "," + total[CollectableType.cube] + "\n"
            );
            
        }

        [Header("DungeonAutoGeneration")]

        public int maxIterationGeneration = 40;
        public int currentGenerationI = 0;

        void loadDungeonConfig()
        {
            string savePath = Application.dataPath + "/Save/DungeonConfig.txt";
            List<string> configValues = File.ReadLines(savePath).ToList();


            foreach (var line in configValues)
            {
                //Debug.Log(line.Split(",")[1]);
                if (line.Contains("cylinder"))
                {
                    total[CollectableType.cylinder] = int.Parse(line.Split(",")[1]);
                } else if (line.Contains("sphere"))
                {
                    total[CollectableType.sphere] = int.Parse(line.Split(",")[1]);
                } else if (line.Contains("cube"))
                {
                    total[CollectableType.cube] = int.Parse(line.Split(",")[1]);
                }
            }
        }

        private void Awake()
        {
            loadDungeonConfig();
            GameManager.Instance.total = total;


            currentGenerationI = 0;
            maxIterationGeneration = 0;

            Debug.unityLogger.logEnabled = false;
            StartCoroutine("GenerateDungeonAuto");
        }
        IEnumerator GenerateDungeonAuto()
        {
            currentGenerationI = 0;
            while (maxIterationGeneration > currentGenerationI)
            {
          
                var dungeonGenerated = new TaskCompletionSource<bool>();
                GenerateAsync(dungeonGenerated, 0.1f, new System.Random().Next(), true); // 1f

                yield return new WaitUntil(()=>dungeonGenerated.Task.IsCompleted);

                maxIterations = new System.Random().Next(10, 20);


                roomBoundsMin = new BoundsInt(
                    roomBoundsMin.position,
                    new Vector3Int(new System.Random().Next(10, 20),
                                new System.Random().Next(6, 15),
                                new System.Random().Next(10, 20)
                    )
                    );
                splitCenterDeviation = new Vector3((float)new System.Random().NextDouble(),
                                (float)new System.Random().NextDouble(),
                                (float)new System.Random().NextDouble()
                    );
                dungeonBounds = new BoundsInt(
                    roomBoundsMin.position,
                    new Vector3Int(new System.Random().Next(5 * roomBoundsMin.size.x, 10 * roomBoundsMin.size.x),
                                new System.Random().Next(5 * roomBoundsMin.size.y, 10 * roomBoundsMin.size.y),
                                new System.Random().Next(5 * roomBoundsMin.size.z, 10 * roomBoundsMin.size.z)
                    )
                    );

                currentGenerationI++;
                Debug.Log($"currentGenerationI {currentGenerationI}");

                Debug.unityLogger.logEnabled = false;

                yield return new WaitForSeconds(1f);

            }
        }



        private async void GenerateAsync(TaskCompletionSource<bool> dungeonGenerated, float delay, int seed, bool isBatched)
        {
           
            DungeonDecorator decorator = GetComponent<DungeonDecorator>();
            decorator.roomGenerator = new RoomGenerator(roomList, this.gameObject);
            DungeonCalculator calculator = new DungeonCalculator(dungeonBounds, new System.Random(seed));
            var result = await Task.Run(() =>
            {
             
                roomList = calculator.CalculateDungeon(maxIterations,
                                             roomBoundsMin,
                                             splitCenterDeviation,
                                             corridorWidthAndWall,
                                             wallThickness,
                                             roomOffsetMin,
                                             corridorHeight);

                return roomList;
            });
         
            InitializeStartAndEnd(calculator.RoomSpaces);
            await Task.WhenAll(decorator.roomGenerator.CalculateRoomWalls(result));
            decorator.roomGenerator.SetupRoomParents();

            var roomsComplete = new TaskCompletionSource<bool>();

            var roomsPerBatch = isBatched ? 1 : result.Count;

            OnDungeonRegenerated?.Invoke();

            StartCoroutine(decorator.roomGenerator.GenerateRoomsBatch(result, roomsPerBatch, roomsComplete, delay));


         

            await roomsComplete.Task;

            OnDungeonFinished?.Invoke();

            dungeonGenerated.SetResult(true); 

        }

        /// <summary>
        /// Add Checks for Slider Ranges for Valid Generation
        /// </summary>

        [ExecuteInEditMode]
        void OnValidate()
        {

            // TODO: Make this more modular 

            // Max Iterations
            maxIterations = Mathf.Max(maxIterations,0);

            // wallThickness
            wallThickness = Mathf.Max(1, wallThickness);

            // Dungeon Bounds 
            // --- Dont Allow Dungeons of Negative sizes
            // TODO: Find Method to do it in one call
            dungeonBounds = new BoundsInt(dungeonBounds.position,
                new Vector3Int(
                    Mathf.Max(dungeonBounds.size.x, 2 * wallThickness + roomOffsetMin.x + roomBoundsMin.size.x),
                    Mathf.Max(dungeonBounds.size.y, 1),
                    Mathf.Max(dungeonBounds.size.z, 2 * wallThickness + + roomOffsetMin.y + roomBoundsMin.size.z)
                )
            );

            // Split Center Deviation
            // --- Clamp between 0-1
            splitCenterDeviation.x = Mathf.Clamp01(splitCenterDeviation.x);
            splitCenterDeviation.y = Mathf.Clamp01(splitCenterDeviation.y);
            splitCenterDeviation.z = Mathf.Clamp01(splitCenterDeviation.z);


            // Room Bounds 
            // --- Dont Allow Rooms larger than the dungeon
            // TODO: Find Method to do it in one call
            roomBoundsMin = new BoundsInt(Vector3Int.zero,
                new Vector3Int(
                    Mathf.Clamp(roomBoundsMin.size.x, 3, dungeonBounds.size.x), // minimum dungeon size is set to 3 as otherwise corridor positioning looks weird 
                    Mathf.Clamp(roomBoundsMin.size.y, 6, Math.Min(dungeonBounds.size.y, 32)), // due to voxel remesher working on 32 int blocks
                    Mathf.Clamp(roomBoundsMin.size.z, 3, dungeonBounds.size.z)
                )
            );

            // TODO: Change to allow Dungeons without Offset
            // --- Dont Allow Rooms larger than the dungeon
            roomOffsetMin = new Vector3Int(Mathf.Max(1,roomOffsetMin.x),Mathf.Max(1,roomOffsetMin.y), Mathf.Max(1, roomOffsetMin.z));

            // Corridor Width
            // --- Dont Allow Corridors larger than the minimum size dimension (width / length)

            var minDim = Mathf.Min(roomBoundsMin.size.x, roomBoundsMin.size.z);


            this.corridorWidth = Mathf.Clamp(corridorWidth,1, minDim-2);


            

        }




    }


}