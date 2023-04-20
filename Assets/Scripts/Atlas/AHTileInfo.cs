using Modding.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AHTileInfo
{
    public string name;
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 pivot;
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 moveOffset;
}

