using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterControllerUtils {

    public static Vector3 GetNormalWithSphereCase(CapsuleCollider capsuleCollider, LayerMask layerMask = default)
    {
        
        Vector3 normal = Vector3.zero;
        Vector3 center = capsuleCollider.transform.position + capsuleCollider.center;
        float distance = capsuleCollider.height / 2f; // + characterController.stepOffset + 0.01f;

        RaycastHit hit;
        if(Physics.SphereCast(center, capsuleCollider.radius, Vector3.down, out hit, distance, layerMask))
        {
            normal = hit.normal;
        }

        return normal;
    }



}
