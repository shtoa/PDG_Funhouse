using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class FontManager : MonoBehaviour
{
    [SerializeField]
    public Font mainFont;
    public static Font MainFont;
    [SerializeField]
    public Material mainFontMaterial;
    public static Material MainFontMaterial;

    [SerializeField]
    public TMP_FontAsset tmpFontAssetMain;
    public static TMP_FontAsset TmpFontAssetMain;

    private void Awake()
    {
        MainFont = mainFont;
        MainFontMaterial = mainFontMaterial;
        TmpFontAssetMain = tmpFontAssetMain;
    }
}
