using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DCAtlasInstance : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    [HideInInspector]
    private string m_data;

    [SerializeField]
    [HideInInspector]
    private List<Texture2D> m_textures;

    public DCAtlas atlas;

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if(atlas == null && !string.IsNullOrEmpty(m_data))
        {
            atlas = JsonConvert.DeserializeObject<DCAtlas>(m_data);
            ((ISerializationCallbackReceiver)atlas).OnAfterDeserialize();
            atlas.atlasTextures = m_textures.ToList();
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        m_textures = atlas.atlasTextures.ToList();
        m_data = JsonConvert.SerializeObject(atlas);
    }
}

