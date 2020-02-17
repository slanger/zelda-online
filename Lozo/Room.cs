using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lozo
{
	public class Room
	{
		public const int NumTilesWidth = 16;
		public const int NumTilesHeight = 11;
		public const int TileWidth = 16 * World.Scale;
		public const int TileHeight = 16 * World.Scale;
		public const int Width = NumTilesWidth * TileWidth;
		public const int Height = NumTilesHeight * TileHeight;

		// First index is in the y direction with 0 at the top, and second index is in the x
		// direction with 0 on the left.
		Tile[][] tiles;
		List<Rectangle> immovables;

		public Room(Tile[][] tiles, List<Rectangle> immovables)
		{
			Debug.Assert(tiles.Length == NumTilesHeight && tiles[0].Length == NumTilesWidth);
			this.tiles = tiles;
			this.immovables = immovables;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			for (int y = 0; y < this.tiles.Length; ++y)
			{
				for (int x = 0; x < this.tiles[y].Length; ++x)
				{
					this.tiles[y][x].Draw(spriteBatch);
				}
			}
		}

		public List<Rectangle> CollidingWith(Rectangle collider)
		{
			var colliding = new List<Rectangle>();
			foreach (Rectangle immovable in this.immovables)
			{
				if (collider.Intersects(immovable))
				{
					colliding.Add(immovable);
				}
			}
			return colliding;
		}
	}
}
