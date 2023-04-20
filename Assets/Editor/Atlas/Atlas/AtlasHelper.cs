using System.Collections.Generic;
using System.IO;
using System.Text;

public static class AtlasHelper
    {
        public static string ReadString(BinaryReader _reader)
        {
            int num = _reader.ReadByte();
            if (num == byte.MaxValue)
            {
                num = _reader.ReadUInt16();
            }
            if (num != 0)
            {
                return new string(_reader.ReadChars(num));
            }
            return "";
        }
        public static void WriteString(BinaryWriter _writer, string _stringToWrite)
        {
            if (_stringToWrite.Length >= byte.MaxValue)
            {
                _writer.Write(byte.MaxValue);
                _writer.Write((ushort)_stringToWrite.Length);
            }
            else
            {
                _writer.Write((byte)_stringToWrite.Length);
            }
            _writer.Write(_stringToWrite.ToCharArray());
        }
        public static Dictionary<string, List<Tile>> ReadAtlas(byte[] stream)
        {
            using(var ms = new MemoryStream(stream))
            {
                return ReadAtlas(ms);
            }
        }
        public static Dictionary<string, List<Tile>> ReadAtlas(Stream stream)
        {
            var binreader = new BinaryReader(stream, Encoding.UTF8, true);
            bool isBin = binreader.ReadByte() == 'B' && binreader.ReadByte() == 'A' && binreader.ReadByte() == 'T' && binreader.ReadByte() == 'L';
            List<Tile> list = null;
            Dictionary<string, List<Tile>> tiles = new Dictionary<string, List<Tile>>();
            if (!isBin)
            {

                binreader.Close();
                stream.Seek(0, SeekOrigin.Begin);
                string[] lines;
                using (StreamReader reader = new StreamReader(stream))
                    lines = reader.ReadToEnd().Replace("\r\n", "\n").Split('\n');
                string name = "";
                (int, int) texSize = (0, 0);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "")
                    {
                        list = null;
                        continue;
                    }
                    if (list == null)
                    {
                        name = lines[i];
                        if (!tiles.TryGetValue(name, out list))
                        {
                            list = new List<Tile>();
                            tiles.Add(name, list);
                        }
                        i++;
                        var sizet = lines[i].Split(':')[1].Trim().Split(',');
                        texSize.Item1 = int.Parse(sizet[0].Trim());
                        texSize.Item2 = int.Parse(sizet[1].Trim());
                        i += 3;
                        continue;
                    }
                    var tileName = lines[i++].Split(':')[0].Trim();
                    i++;
                    var xy = lines[i++].Split(':')[1].Trim().Split(',');
                    var size = lines[i++].Split(':')[1].Trim().Split(',');
                    var orig = lines[i++].Split(':')[1].Trim().Split(',');
                    var offset = lines[i++].Split(':')[1].Trim().Split(',');

                    var index = int.Parse(lines[i].Split(':')[1].Trim());

                    var x = int.Parse(xy[0].Trim());
                    var y = int.Parse(xy[1].Trim());

                    var width = int.Parse(size[0].Trim());
                    var height = int.Parse(size[1].Trim());

                    var origWidth = int.Parse(orig[0].Trim());
                    var origHeight = int.Parse(orig[1].Trim());

                    var offsetx = int.Parse(offset[0].Trim());
                    var offsety = int.Parse(offset[1].Trim());

                    var tile = new Tile
                    {
                        texName = name,
                        name = tileName,
                        x = x - 1,
                        y = y - 1,
                        width = width + 2,
                        height = height + 2,
                        originalWidth = origWidth + 2,
                        originalHeight = origHeight + 2,
                        offsetX = offsetx,
                        offsetY = origHeight - height - offsety,
                        index = index,
                        atlasWidth = texSize.Item1,
                        atlasHeight = texSize.Item2
                    };
                    list.Add(tile);
                }
            }
            else
            {
                while (binreader.BaseStream.Position + 18L < binreader.BaseStream.Length)
                {
                    var prevPosition = stream.Position;

                    if (!(binreader.ReadByte() == 'B' && binreader.ReadByte() == 'A' && binreader.ReadByte() == 'T' && binreader.ReadByte() == 'L')) stream.Position = prevPosition;

                    string text = ReadString(binreader);
                    if (!tiles.TryGetValue(text, out list))
                    {
                        list = new List<Tile>();
                        tiles.Add(text, list);
                    }
                    if (text == "")
                    {
                        break;
                    }
                    while (binreader.BaseStream.Position + 18L < binreader.BaseStream.Length)
                    {
                        Tile tile = new Tile();
                        tile.name = ReadString(binreader);
                        if (tile.name == "")
                        {
                            break;
                        }
                        tile.index = binreader.ReadUInt16();
                        tile.x = binreader.ReadUInt16();
                        tile.y = binreader.ReadUInt16();
                        tile.width = binreader.ReadUInt16();
                        tile.height = binreader.ReadUInt16();
                        tile.offsetX = binreader.ReadUInt16();
                        tile.offsetY = binreader.ReadUInt16();
                        tile.originalWidth = binreader.ReadUInt16();
                        tile.originalHeight = binreader.ReadUInt16();
                        tile.texName = text;
                        list.Add(tile);
                    }
                }
            }
            return tiles;
        }


    }

