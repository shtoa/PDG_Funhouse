using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public enum
    CollectableType
{
    sphere,
    cylinder,
    cube
}

public class MeshCollectableCreator : MonoBehaviour

{
    public static MeshCollectableCreator Instance;

    [SerializeField]
    public AnimationCurve MaterialFadeOut;
    public Material CollectableMaterial;
    public Material CollectableOutline;

    private List<Material> mList = new List<Material>();

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
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
