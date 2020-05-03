using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Lozo
{
	public class Room
	{
		public const int NumTilesWidth = 16;
		public const int NumTilesHeight = 11;
		public const int TileWidth = 48;
		public const int TileHeight = 48;
		public const int Width = NumTilesWidth * TileWidth;
		public const int Height = NumTilesHeight * TileHeight;

		public Point DungeonIndex { get; }

		public List<Rectangle> Immovables { get; } = new List<Rectangle>();

		public Room(Point dungeonIndex)
		{
			this.DungeonIndex = dungeonIndex;
		}

		public Rectangle BoundingBox()
		{
			return new Rectangle(this.DungeonIndex.X * Width, this.DungeonIndex.Y * Height, Width, Height);
		}

		public bool Contains(Point point)
		{
			return this.BoundingBox().Contains(point);
		}

		public List<Rectangle> CollidingWith(Rectangle collider)
		{
			var colliding = new List<Rectangle>();
			foreach (Rectangle immovable in this.Immovables)
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
