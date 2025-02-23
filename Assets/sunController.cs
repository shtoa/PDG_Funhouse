using dungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunController : MonoBehaviour
{


    [SerializeField]
    Transform playerTransform;
    [SerializeField]
    Transform skyBoxTransform;


    [SerializeField]
    DungeonGenerator dungeonGenerator;


    private Vector3 orbitCenter;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.one*20f;
        orbitCenter = dungeonGenerator.transform.position + dungeonGenerator.dungeonBounds.size / 2;




    }

    // Update is called once per frame
    void Update()
    {
        // set Rotation around center
        transform.position = orbitCenter + new Vector3(100*Mathf.Cos(Time.time/10),50,100*Mathf.Sin(Time.time/10));
        
        // Set Local Rotation
        
        transform.LookAt(playerTransform.position);
        transform.transform.Rotate(0, 90, 0);


        


       

    }
}
