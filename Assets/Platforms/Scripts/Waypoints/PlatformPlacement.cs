using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using System;
using TMPro;

//[ExecuteInEditMode]

[DefaultExecutionOrder(-1)]
public class PlatformPlacement : MonoBehaviour
{
 
    // Start is called before the first frame update
    [SerializeField] public GameObject _platform;
    private float nPlatforms = 0;
    [SerializeField] public GameObject platformHolder;
    [SerializeField] GameObject _goal;
    [SerializeField] Canvas _canvas;
    [SerializeField] TextMeshPro _platformTimer;
    public float _spacing = 0f;


    private List<Vector3> Waypoints = new List<Vector3>();
    private List<float> distanceAlongLine = new List<float> ();

    private float timeElapsed;
    private GameObject startPlatform;
    private GameObject endPlatform;


    
    void Start()
    {
        for (int i = platformHolder.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(platformHolder.transform.GetChild(i).gameObject);
        }


        for (int i = platformHolder.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(platformHolder.transform.GetChild(i).gameObject);
        }


        foreach (Transform child in transform){
            Waypoints.Add(child.transform.position);
        }

        print($"the number of waypoints is {Waypoints.Count}");


        if (Waypoints.Count > 2) {

           
            for(var i = 1; i < Waypoints.Count; i++)
            {
                Vector3 p1 = Waypoints[i - 1];
                Vector3 p2 = Waypoints[i];

                float distAlongLine = Vector3.Distance(p1, p2);
                float platformSize = 47f*_platform.GetComponent<BoxCollider>().size.x; // remember scale

                distAlongLine -= 2f * platformSize;
       

                nPlatforms = Mathf.FloorToInt(distAlongLine/(platformSize+_spacing)); // figure out how to add spacing
       

                for(int plat = 0; plat < nPlatforms; plat++)
                {

                    Vector3 newLoc = Vector3.Lerp(p1, p2, (plat) / nPlatforms);
                    Vector3 rotateTo = new Vector3();

                    if (plat == 0 && nPlatforms > 0)
                    {
                        rotateTo = Vector3.Lerp(p1, p2, (plat+1) / nPlatforms) - p1;
                    }
                    else
                    {
                        rotateTo = newLoc - p1;
                    }
                    
                    
                    rotateTo = rotateTo.normalized; 
                    Vector3 lookLoc = _platform.transform.forward;



                   float angle = Vector3.SignedAngle(new Vector3(lookLoc.x, 0, lookLoc.z), new Vector3(rotateTo.x, 0, rotateTo.z), Vector3.up);
      
          


                    GameObject g = GameObject.Instantiate(_platform, newLoc, Quaternion.Euler(270, angle, 0f));

                    if(i == 1 && plat == 0)
                    {
                        startPlatform = g;
                        g.GetComponent<PlatformMovement>().isStart = true;
                        print(startPlatform);
                        
                    }

                    if(plat == 0)
                    {
                        g.GetComponent<PlatformMovement>().isWaypoint = true;
                        print(g.GetComponent<PlatformMovement>().platformType);
                    }
                    g.transform.parent = platformHolder.transform;

                    //  && (i == Waypoints.Count-1)
                    if ((plat == nPlatforms - 1)) {
                        GameObject goal = GameObject.Instantiate(_goal, newLoc, Quaternion.Euler(-90f, angle+90f, 0f)); // -90f
                        goal.transform.position = g.transform.position;
                        goal.transform.parent = g.transform;

                        if((i == Waypoints.Count - 1))
                        {
                            endPlatform = g;
                            g.GetComponent<PlatformMovement>().isEnd = true;
                        }
                    
                    
                    }
                }

            }

            
        
        
        }


        //foreach (Vector3 pos in Waypoints)
        //{
        //    if (_platform)
        //    {
        //        GameObject g = GameObject.Instantiate(_platform, pos, Quaternion.Euler(270f, 0, 0));
        //    }
        //}
    }

    void Update()
    {
        foreach (Transform child in transform)
        {
            if (child.transform.hasChanged)
            {
                // update positions of the platforms / recalculate for the branch
            }

            
        }

        if (startPlatform.gameObject.GetComponent<PlatformMovement>().hasStarted)
        {
            timeElapsed += Time.deltaTime;
            _platformTimer.text = "The Time " + Math.Round(timeElapsed, 2).ToString();
        }
        if (endPlatform.gameObject.GetComponent<PlatformMovement>().hasEnded)
        {
            print(timeElapsed);
            _platformTimer.text = "The END Time " + Math.Round(timeElapsed, 2).ToString() + "!!! ";
            timeElapsed = 0;

            startPlatform.GetComponent<PlatformMovement>().hasStarted = false;
            endPlatform.GetComponent<PlatformMovement>().hasEnded = false;

        }
    }

}
