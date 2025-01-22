using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
     
        if (other.CompareTag("Player"))
        {
            Debug.Log("CColliding with player");
            other.gameObject.GetComponent<PlayerController>().isClimbing = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().isClimbing = false;
        }
    }
}
