using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Lozo
{
	public class World
	{
		public const int Scale = 3;
		public const int Width = Room.Width;
		public const int Height = Room.Height;

		Player player;
		Room[,] rooms;
		Room currentRoom;

		public World(Room[,] rooms)
		{
			this.rooms = rooms;
		}

		public void AddPlayer(Player player)
		{
			this.player = player;
		}

		public void UpdateCurrentRoom(Rectangle location)
		{
			Point center = location.Center;
			if (this.currentRoom == null)
			{
				for (int y = 0; y < this.rooms.GetLength(0); ++y)
				{
					for (int x = 0; x < this.rooms.GetLength(1); ++x)
					{
						if (this.rooms[y, x].BoundingBox().Contains(center))
						{
							this.currentRoom = this.rooms[y, x];
							return;
						}
					}
				}
				throw new InvalidOperationException("Player start location is invalid: " + location);
			}
			Rectangle boundingBox = this.currentRoom.BoundingBox();
			if (!boundingBox.Contains(center))
			{
				Point dungeonLocation = this.currentRoom.DungeonLocation;
				Room leftRoom = this.GetRoom(new Point(dungeonLocation.X - 1, dungeonLocation.Y));
				if (leftRoom != null && leftRoom.BoundingBox().Contains(center))
				{
					this.currentRoom = leftRoom;
					return;
				}
				Room upRoom = this.GetRoom(new Point(dungeonLocation.X, dungeonLocation.Y - 1));
				if (upRoom != null && upRoom.BoundingBox().Contains(center))
				{
					this.currentRoom = upRoom;
					return;
				}
				Room rightRoom = this.GetRoom(new Point(dungeonLocation.X + 1, dungeonLocation.Y));
				if (rightRoom != null && rightRoom.BoundingBox().Contains(center))
				{
					this.currentRoom = rightRoom;
					return;
				}
				Room downRoom = this.GetRoom(new Point(dungeonLocation.X, dungeonLocation.Y + 1));
				if (downRoom != null && downRoom.BoundingBox().Contains(center))
				{
					this.currentRoom = downRoom;
					return;
				}
				// If we can't find a room that contains the new location, then just keep
				// this.currentRoom the same.
			}
		}

		public List<Rectangle> CollidingWith(Rectangle collider)
		{
			return this.currentRoom.CollidingWith(collider);
		}

		public Point RelativeToCurrentRoom(Point point)
		{
			Rectangle boundingBox = this.currentRoom.BoundingBox();
			return new Point(point.X - boundingBox.X, point.Y - boundingBox.Y);
		}

		public void Update(KeyboardState state)
		{
			this.player.Update(state);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			this.currentRoom.Draw(spriteBatch);
			// Draw the player last so that the player is drawn on top of everything else in the
			// world.
			this.player.Draw(spriteBatch);
		}

		// GetRoom gets the room in the Dungeon based on the given Point, or returns null if one doesn't
		// exist at the given Point.
		private Room GetRoom(Point dungeonLocation)
		{
			if (dungeonLocation.X < 0 || dungeonLocation.X >= this.rooms.GetLength(1) || dungeonLocation.Y < 0 || dungeonLocation.Y >= this.rooms.GetLength(0))
			{
				return null;
			}
			return this.rooms[dungeonLocation.Y, dungeonLocation.X];
		}
	}
}
