using UnityEngine;
using UnityEngine.Pool;

public interface IAssetPoolObject<T> where T : MonoBehaviour
{
    public void SetPool(ObjectPool<T> _assetPool);
}