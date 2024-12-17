using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public enum
    CollectableType
{
    sphere,
    cylinder,
    cube
}

[InitializeOnLoadAttribute]
[ExecuteAlways]
public class MeshCollectableCreator : MonoBehaviour

{
    public static MeshCollectableCreator instance;



    /// <summary>
    /// singleton logic from: <see href="https://stackoverflow.com/a/67717318"> stackOverflow: derHugo </see>
    /// </summary>
    public static MeshCollectableCreator Instance
    {
        get
        {
            // if instance exists return
            if (instance) return instance;

            // if object exists in scene return object
            instance = FindObjectOfType<MeshCollectableCreator>();
            if (instance) return instance;

            // if object doesnt exist return new object
            instance = new GameObject("MeshCollectableCreator").AddComponent<MeshCollectableCreator>();
            return instance;
        }
    }

    [SerializeField]
    public AnimationCurve MaterialFadeOut;
    public Material CollectableMaterial;
    public Material CollectableOutline;

    private List<Material> mList = new List<Material>();


    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        mList.Clear();

        mList.Add(CollectableMaterial);
        mList.Add(CollectableOutline);
    }
    public GameObject GenerateCollectable(CollectableType collectableType, Transform parentTransform)
    {
        GameObject collectable = null;

        switch (collectableType)
        {
            case CollectableType.sphere:
                collectable = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                collectable.AddComponent<MeshCollectable>().collectableType = CollectableType.sphere;
                break;
            case CollectableType.cylinder:
                collectable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                collectable.AddComponent<MeshCollectable>().collectableType = CollectableType.cylinder;
                break;
            case CollectableType.cube:
                collectable = GameObject.CreatePrimitive(PrimitiveType.Cube);
                collectable.AddComponent<MeshCollectable>().collectableType = CollectableType.cube;
                break;
        }

        if (collectable != null)
        {

            collectable.transform.SetParent(parentTransform, false);

            collectable.GetComponent<Collider>().isTrigger = true;
            collectable.GetComponent<MeshRenderer>().SetMaterials(mList);
            //collectable.GetComponent<MeshCollectable>().materialFadeOut = MaterialFadeOut;

        }

        return collectable;
    }
}
