using dungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class sunController : MonoBehaviour
{


    [SerializeField]
    Transform playerTransform;
    [SerializeField]
    Transform skyBoxTransform;


    [SerializeField]
    DungeonGenerator dungeonGenerator;

    [SerializeField]
    Camera cam;
    private Texture2D dungeonViewImageSave;
    public RenderTexture dungeonViewTexture;

    private Vector3 orbitCenter;
    private Vector3 dungeonLookAt;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.one*20f;
        orbitCenter = dungeonGenerator.transform.position + dungeonGenerator.dungeonBounds.size / 2;

        cam.transform.parent = transform;
        cam.transform.position = transform.position;

        UpdateDungeonCameraLookAt();

        dungeonGenerator.OnDungeonRegenerated += UpdateDungeonCameraLookAt;

        dungeonGenerator.OnDungeonFinished -= SaveDungeonView;

    }

    private void SaveDungeonView()
    {
        var old_texture = RenderTexture.active;
        dungeonViewImageSave = new Texture2D(dungeonViewTexture.width, dungeonViewTexture.height, TextureFormat.ARGB32, false);
        RenderTexture.active = dungeonViewTexture;
        dungeonViewImageSave.ReadPixels(new Rect(0, 0, dungeonViewTexture.width, dungeonViewTexture.height), 0, 0);
        dungeonViewImageSave.Apply();
        RenderTexture.active = old_texture;

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/DungeonImages/DungeonImage.png");
        File.WriteAllBytes(uniquePath,dungeonViewImageSave.EncodeToPNG());
    }
        

    private void UpdateDungeonCameraLookAt()
    {
        var boundsSize = dungeonGenerator.dungeonBounds.size;

        dungeonLookAt = dungeonGenerator.dungeonBounds.size / 2 + dungeonGenerator.transform.position;
        cam.orthographicSize = math.max(math.max(boundsSize.x, boundsSize.y), boundsSize.z);
    }

    // Update is called once per frame
    void Update()
    {
        // set Rotation around center
        transform.position = orbitCenter + new Vector3(100*Mathf.Cos(Time.time/10),50,100*Mathf.Sin(Time.time/10));
        // Set Local Rotation

        transform.LookAt(playerTransform.position);
        
        transform.transform.Rotate(0, 90, 0);
        cam.transform.LookAt(dungeonLookAt);

        //cam.transform.localEulerAngles = new Vector3(0, -90f, 0f);







    }
}
