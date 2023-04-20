using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildAB
{
    public static readonly string OutputDir = "HKMod/AssetBundle";
    [MenuItem("Assets/Build Asset")]
    static void BuildAssets()
    {
        BuildAssetBundle(BuildTarget.StandaloneWindows64);
    }

    static void BuildAssetBundle(BuildTarget platform)
    {
        var path = Path.Combine(OutputDir, platform.ToString());
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, platform);
    }
}
