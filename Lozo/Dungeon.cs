using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System;
using System.Collections.Generic;

namespace Lozo
{
	public class Dungeon
	{
		Room[,] rooms;
		Room currentRoom;
		TiledMap map;
		TiledMapRenderer mapRenderer;

		public Dungeon(TiledMap map, TiledMapRenderer mapRenderer)
		{
			this.map = map;
			this.mapRenderer = mapRenderer;
		}

		public void Update(KeyboardState state)
		{
			//this.mapRenderer.Update(this.map, gameTime);
		}

		public void DrawBottomLayer(OrthographicCamera camera)
		{
			//this.currentRoom.Draw(spriteBatch);
			//Matrix.CreateScale(World.Scale)
			this.mapRenderer.Draw(this.map.GetLayer("Bottom"), camera.GetViewMatrix());
		}

		public void DrawTopLayer(OrthographicCamera camera)
		{
			this.mapRenderer.Draw(this.map.GetLayer("Top"), camera.GetViewMatrix());
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
				throw new ArgumentException("Player start location is invalid: " + location);
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
