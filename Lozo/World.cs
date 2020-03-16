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

		public Player player;
		Room[,] rooms;
		Room currentRoom;
		Sprite floorSprite;
		Sprite rockSprite;

		public World()
		{
			// The sprites get their actual values in the AddSpriteSheet method.
			this.floorSprite = new Sprite();
			this.rockSprite = new Sprite();

			Tile[,] tiles;
			List<Rectangle> immovables;
			void createEmptyRoom()
			{
				tiles = new Tile[Room.NumTilesHeight, Room.NumTilesWidth];
				for (int y = 0; y < tiles.GetLength(0); ++y)
				{
					for (int x = 0; x < tiles.GetLength(1); ++x)
					{
						tiles[y, x] = new Tile(this.floorSprite, new Rectangle(x * Room.TileWidth, y * Room.TileHeight, Room.TileWidth, Room.TileHeight));
					}
				}
				immovables = new List<Rectangle>();
			}
			void addRock(int x, int y)
			{
				tiles[y, x].Sprite = this.rockSprite;
				immovables.Add(new Rectangle(x * Room.TileWidth, y * Room.TileHeight, Room.TileWidth, Room.TileHeight));
			}

			// Room 1
			createEmptyRoom();
			addRock(7, 5);
			addRock(6, 5);
			addRock(7, 4);
			addRock(8, 5);
			addRock(7, 6);
			for (int x = 0; x < tiles.GetLength(1); ++x)
			{
				addRock(x, 0);
				addRock(x, 10);
			}
			for (int y = 1; y < tiles.GetLength(0) - 1; ++y)
			{
				addRock(0, y);
				if (y != 5) addRock(15, y);
			}
			Room room1 = new Room(new Point(0, 0), tiles, immovables);

			// Room 2
			createEmptyRoom();
			addRock(3, 3);
			addRock(12, 3);
			addRock(3, 7);
			addRock(12, 7);
			for (int x = 0; x < tiles.GetLength(1); ++x)
			{
				addRock(x, 0);
				if (x != 7 && x != 8) addRock(x, 10);
			}
			for (int y = 1; y < tiles.GetLength(0) - 1; ++y)
			{
				if (y != 5) addRock(0, y);
				addRock(15, y);
			}
			Room room2 = new Room(new Point(1, 0), tiles, immovables);

			// Room 3
			createEmptyRoom();
			addRock(7, 5);
			addRock(8, 5);
			for (int x = 0; x < tiles.GetLength(1); ++x)
			{
				if (x != 7 && x != 8) addRock(x, 0);
				addRock(x, 10);
			}
			for (int y = 1; y < tiles.GetLength(0) - 1; ++y)
			{
				addRock(0, y);
				addRock(15, y);
			}
			Room room3 = new Room(new Point(1, 1), tiles, immovables);

			this.rooms = new Room[2, 2];
			this.rooms[room1.DungeonLocation.Y, room1.DungeonLocation.X] = room1;
			this.rooms[room2.DungeonLocation.Y, room2.DungeonLocation.X] = room2;
			this.rooms[room3.DungeonLocation.Y, room3.DungeonLocation.X] = room3;
			var location = new Point(Room.Width + (Room.Width / 2), Room.Height / 2);
			this.player = new Player(this, location);
		}

		public void AddSpriteSheet(Texture2D spritesheet)
		{
			this.floorSprite.SpriteSheet = spritesheet;
			this.floorSprite.Source = new Rectangle(1, 154, 16, 16);
			this.rockSprite.SpriteSheet = spritesheet;
			this.rockSprite.Source = new Rectangle(1, 188, 16, 16);
		}

		public void AddPlayerSpriteSheet(Texture2D spritesheet)
		{
			this.player.AddSpriteSheet(spritesheet);
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
