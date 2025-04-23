using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayTest", menuName = "PlayTesting/PlayTest")]
public class PlayTestScriptableObject : ScriptableObject
{
    public string testName;
    public List<TextAsset> dungeonConfigs;

}
