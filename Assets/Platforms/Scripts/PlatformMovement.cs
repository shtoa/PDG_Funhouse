using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



public enum PlatformMovementType
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    Both = 3,
}

public class PlatformMovement : MonoBehaviour

{
    [SerializeField] float moveMag = 10f;
    [SerializeField] float moveFreq = 10f;
  
    
    public bool isWaypoint = false;
    
    public bool isStart = false;
    public bool isEnd = false;
    public bool hasStarted = false;
    public bool hasEnded = false;

    private Vector3 _currentPosition;
    private float offsetSin;

    public PlatformMovementType platformType;


    // Start is called before the first frame update
    void Start()
    {
        _currentPosition = transform.position;
        offsetSin = Random.Range(0, 360f);


        // check if it is possible for enum ot have two states

        platformType = (!isWaypoint) ? (PlatformMovementType)Random.Range(0,4) : 
                       Random.value > 0.5f ? PlatformMovementType.None : PlatformMovementType.Vertical;

        

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // fix glitchiness
       switch(platformType) { 
            case PlatformMovementType.Horizontal:
                transform.position = _currentPosition + moveMag * Mathf.Sin(moveFreq * Time.time + offsetSin) * transform.right;
                break;
            case PlatformMovementType.Vertical:
                transform.position = _currentPosition + moveMag * Mathf.Sin(moveFreq * Time.time + offsetSin) * transform.forward;
                break;
            case PlatformMovementType.Both:
                transform.position = _currentPosition + moveMag * Mathf.Sin(moveFreq * Time.time + offsetSin) * transform.right + moveMag * Mathf.Cos(moveFreq * Time.time + offsetSin) * transform.forward;
                break;

        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
       

        if(other.tag == "Player")
        {
            other.transform.SetParent(transform);

            if (isStart)
            {
                hasStarted = true;
                print("isStart");
            }

            if (isEnd)
            {
                hasEnded = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.transform.SetParent(null);
        }
    }
}
