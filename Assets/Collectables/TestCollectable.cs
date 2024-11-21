using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum
    CollectableType
{
    sphere,
    cylinder,
    cube
}


public class TestCollectable : MonoBehaviour
{
    public CollectableType collectableType;
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(collectableType == CollectableType.sphere)
            {
                GameMaster.spheresCollected++;
                
            } else if(collectableType== CollectableType.cube){


                GameMaster.cubesCollected++;


            } else if(collectableType == CollectableType.cylinder)
            {
                GameMaster.cylinderCollected++;

            }

            // destroy collectable aftyer collision
            Destroy(transform.gameObject);
        }
    }
}
