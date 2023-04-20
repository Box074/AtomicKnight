using Modding.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DCAtlas : ISerializationCallbackReceiver
{
    [Serializable]
    public class Tile
    {
        public string originalName;
        public string name;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 xy;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 wh;
        [JsonIgnore]
        public Rect rect
        {
            get => new Rect(xy, wh);
            set
            {
                xy = value.position;
                wh = value.size;
            }
        }
        public int index;

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 offset;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 originalSize;

        [NonSerialized]
        [JsonIgnore]
        public DCAtlas atlas;

        public int atlasId;

        [JsonIgnore]
        public Texture2D atlasTexture
        {
            get
            {
                return atlas.atlasTextures[atlasId];
            }
        }

        public Sprite GenerateSprite()
        {
            return Sprite.Create(atlasTexture, 
                new Rect(rect.x, atlasTexture.height - rect.y - rect.height, rect.width, rect.height), 
                new Vector2(0f, 0f), 100,
                0, SpriteMeshType.FullRect);
        }
    }

    [Serializable]
    public class Clip
    {
        public string name;
        public List<int> frames = new List<int>();

        [NonSerialized]
        [JsonIgnore]
        public DCAtlas atlas;

        public Tile GetFrame(int id) => atlas.tiles[frames[id]];
    }

    public List<string> atlasTextureNames = new List<string>();

    [JsonIgnore]
    public List<Texture2D> atlasTextures = new List<Texture2D>();
    public List<Tile> tiles = new List<Tile>();

    public List<string> animNames = new List<string>();
    public List<Clip> clips = new List<Clip>();

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        foreach (var tile in tiles)
        {
            tile.atlas = this;
        }
        foreach (var clip in clips)
        {
            clip.atlas = this;
        }
    }
}
