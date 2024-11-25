using System;
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
    MeshFilter meshF;
    bool isAddedToMinimap;
    private bool isMinimapObject = false;
    public AnimationCurve materialFadeOut;

    public bool IsMinimapObject { get => isMinimapObject; set => isMinimapObject = value; }
    //public AnimationCurve MaterialFadeOut { get => materialFadeOut; set => materialFadeOut = value; }

    public void Start()
    {

        postCollectionTime = 0f;
        isCollected = false;
        isAddedToMinimap = false;

    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && isCollected == false && !isMinimapObject)
        {
            if(collectableType == CollectableType.sphere )
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

    private GameObject minimapObject;

    public void Update()
    {
        if (!isAddedToMinimap && !isMinimapObject)
        {

            if (gameObject.TryGetComponent<MeshFilter>(out meshF))
            {

                minimapObject = GameObject.Instantiate(gameObject, transform.position + Vector3.up * 10f, transform.rotation);
                minimapObject.transform.Rotate(Vector3.right, 45);
                minimapObject.transform.localScale = Vector3.one * 1.5f;
                minimapObject.transform.position += Vector3.forward * 2;
                minimapObject.transform.parent = transform;
                isAddedToMinimap = true;
                minimapObject.GetComponent<TestCollectable>().IsMinimapObject = true;
                minimapObject.layer = LayerMask.NameToLayer("MiniMap");
                minimapObject.GetComponent<Collider>().enabled = false;

            }

        }


        if (isCollected)
        {
          
           

            postCollectionTime += 0.01f; // or time delta time
            GetComponent<MeshRenderer>().materials[0].SetFloat("_isCollected", 1);
            GetComponent<MeshRenderer>().materials[0].SetFloat("_collectedTime", materialFadeOut.Evaluate(postCollectionTime));
            GetComponent<MeshRenderer>().materials[1].SetFloat("_isCollected", 1);
            GetComponent<MeshRenderer>().materials[1].SetFloat("_collectedTime", materialFadeOut.Evaluate(postCollectionTime));

            MeshRenderer minimapRenderer = minimapObject.GetComponent<MeshRenderer>();
            minimapRenderer.materials[0].SetFloat("_isCollected", 1);
            minimapRenderer.materials[0].SetFloat("_collectedTime", materialFadeOut.Evaluate(postCollectionTime));
            minimapRenderer.materials[1].SetFloat("_isCollected", 1);
            minimapRenderer.materials[1].SetFloat("_collectedTime", materialFadeOut.Evaluate(postCollectionTime));
        }

        if(postCollectionTime > 1f && isCollected) 
        {
            Destroy(gameObject);
        }
    }
}
