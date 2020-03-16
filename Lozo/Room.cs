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

		public Point DungeonLocation { get; }

		// First index is in the y direction with 0 at the top, and second index is in the x
		// direction with 0 on the left. Tile locations are relative to the upper-left corner of
		// the Room.
		Tile[,] tiles;
		// Immovable locations are *not* relative to the Room.
		List<Rectangle> immovables;

		public Room(Point dungeonLocation, Tile[,] tiles, List<Rectangle> immovables)
		{
			Debug.Assert(tiles.GetLength(0) == NumTilesHeight && tiles.GetLength(1) == NumTilesWidth);
			this.DungeonLocation = dungeonLocation;
			this.tiles = tiles;
			// Make a copy of the immovables and change their locations to be absolute locations
			// instead of being relative to the Room.
			this.immovables = new List<Rectangle>(immovables);
			for (int i = 0; i < this.immovables.Count; ++i)
			{
				Point absLocation = this.RelativeToAbsolute(this.immovables[i].Location);
				this.immovables[i] = new Rectangle(absLocation, this.immovables[i].Size);
			}
		}

		public Rectangle BoundingBox()
		{
			return new Rectangle(this.DungeonLocation.X * Width, this.DungeonLocation.Y * Height, Width, Height);
		}

		public Point RelativeToAbsolute(Point point)
		{
			return new Point(point.X + (this.DungeonLocation.X * Width), point.Y + (this.DungeonLocation.Y * Height));
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

		public void Draw(SpriteBatch spriteBatch)
		{
			for (int y = 0; y < this.tiles.GetLength(0); ++y)
			{
				for (int x = 0; x < this.tiles.GetLength(1); ++x)
				{
					this.tiles[y, x].Draw(spriteBatch);
				}
			}
		}
	}
}
