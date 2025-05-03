using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BloodShaderUpdate : MonoBehaviour
{
    public float fillAmount = 0.5f;

    // Update is called once per frame
    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3 worldPos = transform.TransformPoint(new Vector3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z));

       
        Vector3 pos = worldPos - transform.position - new Vector3(0, fillAmount, 0);
        GetComponent<MeshRenderer>().material.SetVector("_Fill", pos);

    }
}
