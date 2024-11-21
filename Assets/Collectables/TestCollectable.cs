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
    public bool isCollected;

    private float postCollectionTime;
    public void Start()
    {

        postCollectionTime = 0f;
        isCollected = false;

    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && isCollected == false)
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
            isCollected = true;
        }
    }

    public void Update()
    {
        if(isCollected)
        {
            postCollectionTime += 0.01f; // or time delta time
            GetComponent<MeshRenderer>().materials[0].SetFloat("_isCollected", 1);
            GetComponent<MeshRenderer>().materials[0].SetFloat("_collectedTime", postCollectionTime);
            GetComponent<MeshRenderer>().materials[1].SetFloat("_isCollected", 1);
            GetComponent<MeshRenderer>().materials[1].SetFloat("_collectedTime", postCollectionTime);

        }

        if(postCollectionTime > 1f && isCollected) 
        {
            Destroy(gameObject);
        }
    }
}
