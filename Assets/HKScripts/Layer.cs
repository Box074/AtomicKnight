using System;
using UnityEngine;

namespace tk2dRuntime.TileMap
{
	[Serializable]
	public class Layer
	{
		public Layer(int hash, int width, int height, int divX, int divY)
		{
		}

		public int hash;
		public SpriteChannel spriteChannel;
		public int width;
		public int height;
		public int numColumns;
		public int numRows;
		public int divX;
		public int divY;
		public GameObject gameObject;
	}
}
