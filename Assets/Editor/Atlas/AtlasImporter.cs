using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Assets.Editor.DC
{
    [ScriptedImporter(1, "atlas")]
    internal class AtlasImporter : ScriptedImporter
    {
        public List<Texture2D> atlasTextures = new List<Texture2D>();

        public DCAtlas atlasObj;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var atlas = AtlasHelper.ReadAtlas(File.ReadAllBytes(ctx.assetPath));
            atlasObj = new DCAtlas();

            var inst = ScriptableObject.CreateInstance<DCAtlasInstance>();

            var texNames = new List<string>();
            
            foreach(var pair in atlas)
            {
                var png = pair.Key;

                var id = texNames.IndexOf(png);
                if(id == -1)
                {
                    id = texNames.Count;
                    texNames.Add(png);

                    if(atlasTextures.Count <= id)
                    {
                        atlasTextures.Add(null);
                    }
                    atlasObj.atlasTextures.Add(atlasTextures[id]);
                    atlasObj.atlasTextureNames.Add(png);
                    
                }

                foreach(var t in pair.Value)
                {
                    var tile = new DCAtlas.Tile();
                    tile.originalName = t.name;

                    var id_part = t.name.Split('_').Last().TrimStart('0');
                    if(!string.IsNullOrEmpty(id_part))
                    {
                        if(int.TryParse(id_part, out var t_id))
                        {
                            t.index = t_id;
                            t.name = t.name.Substring(0, t.name.LastIndexOf('_'));
                        }
                    }
                    else
                    {
                        if(t.name.EndsWith("0"))
                        {
                            t.index = 0;
                            t.name = t.name.Substring(0, t.name.LastIndexOf('_'));
                        }
                    }

                    var clipId = atlasObj.animNames.IndexOf(t.name);

                    if(clipId == -1)
                    {
                        clipId = atlasObj.animNames.Count;
                        atlasObj.animNames.Add(t.name);
                        var c = new DCAtlas.Clip();
                        c.name = t.name;
                        c.atlas = atlasObj;
                        atlasObj.clips.Add(c);
                    }

                    var clip = atlasObj.clips[clipId];
                    if(t.index >= clip.frames.Count)
                    {
                        clip.frames.AddRange(Enumerable.Repeat(-1, t.index - clip.frames.Count + 1));
                    }

                    clip.frames[t.index] = atlasObj.tiles.Count;

                    tile.name = t.name;
                    tile.rect = new Rect(t.x, t.y, t.width, t.height);
                    tile.offset = new Vector2(t.offsetX, t.offsetY);
                    tile.atlas = atlasObj;
                    tile.atlasId = id;
                    tile.index = t.index;
                    tile.originalSize = new Vector2(t.originalWidth, t.originalHeight);

                    atlasObj.tiles.Add(tile);
                }
            }

            inst.atlas = atlasObj;
            ctx.AddObjectToAsset("Atlas", inst);
            ctx.SetMainObject(inst);
        }
    }

    [CustomEditor(typeof(AtlasImporter))]
    internal class AtlasImporterEditor : UnityEditor.Editor
    {
        public Vector2 scroller;

        public override void OnInspectorGUI ()
        {
            var t = (AtlasImporter)target;
            var t_s = new SerializedObject(t);
            var m_Tex = t_s.FindProperty(nameof(AtlasImporter.atlasTextures));
            
            if (t.atlasObj == null)
            {
                GUILayout.Label("Should Import");
                return;
            }

            scroller = GUILayout.BeginScrollView(scroller);
            GUILayout.BeginVertical();

            GUILayout.Label("Atlas Textures", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();

            
            
            for(int i = 0; i < t.atlasObj.atlasTextureNames.Count; i++)
            {
                var name = t.atlasObj.atlasTextureNames[i];
                if(m_Tex.arraySize <= i)
                {
                    m_Tex.InsertArrayElementAtIndex(m_Tex.arraySize);
                }

                var tex = m_Tex.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(tex, new GUIContent(name));

            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            
            if (EditorGUI.EndChangeCheck())
            {
                t_s.ApplyModifiedProperties();
            }
        }
    }
}
