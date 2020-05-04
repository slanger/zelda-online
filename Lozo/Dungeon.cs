using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System;

namespace Lozo
{
	public class Dungeon
	{
		public const string BottomLayerName = "Bottom";
		public const string TopLayerName = "Top";
		public const string CollisionLayerName = "Collision";

		World world;
		TiledMap map;
		TiledMapRenderer mapRenderer;
		Room[,] rooms;

		public Dungeon(World world, TiledMap map, TiledMapRenderer mapRenderer)
		{
			this.world = world;
			this.map = map;
			this.mapRenderer = mapRenderer;
			if (map.Width % Room.NumTilesWidth != 0 || map.Height % Room.NumTilesHeight != 0)
			{
				throw new ArgumentException(
					$"Invalid dimensions ({map.Width} x {map.Height}) for map \"{map.Name}\". Map dimensions must be a multiple of ({Room.NumTilesWidth} x {Room.NumTilesHeight}).");
			}
			var bottomLayer = map.GetLayer<TiledMapTileLayer>(BottomLayerName);
			if (bottomLayer == null)
			{
				throw new ArgumentException($"Map \"{map.Name}\" doesn't contain a tile layer named \"{BottomLayerName}\"");
			}
			this.rooms = new Room[map.Width / Room.NumTilesWidth, map.Height / Room.NumTilesHeight];
			for (int j = 0; j < this.rooms.GetLength(1); ++j)
			{
				for (int i = 0; i < this.rooms.GetLength(0); ++i)
				{
					int x = i * Room.NumTilesWidth;
					int y = j * Room.NumTilesHeight;
					if (IsRoomEmpty(bottomLayer, x, y)) continue;
					this.rooms[i, j] = new Room(new Point(i, j));
				}
			}
			var objectLayer = map.GetLayer<TiledMapObjectLayer>(CollisionLayerName);
			if (objectLayer == null)
			{
				throw new ArgumentException($"Map \"{map.Name}\" doesn't contain an object layer named \"{CollisionLayerName}\"");
			}
			foreach (TiledMapObject obj in objectLayer.Objects)
			{
				Rectangle immovable = new Rectangle((int)obj.Position.X, (int)obj.Position.Y, (int)obj.Size.Width, (int)obj.Size.Height);
				Room room = this.RoomContains(immovable.Center);
				if (room == null)
				{
					throw new ArgumentException($"Couldn't find room containing object {obj}");
				}
				room.Immovables.Add(immovable);
			}
		}

		public void Update(KeyboardState state)
		{
			//this.mapRenderer.Update(this.map, gameTime);
		}

		public void DrawBottomLayer(OrthographicCamera camera)
		{
			this.mapRenderer.Draw(this.map.GetLayer(BottomLayerName), camera.GetViewMatrix());
		}

		public void DrawTopLayer(OrthographicCamera camera)
		{
			this.mapRenderer.Draw(this.map.GetLayer(TopLayerName), camera.GetViewMatrix());
		}

		public Room GetCurrentRoom(Room previous, Rectangle location)
		{
			Point center = location.Center;
			if (previous == null)
			{
				for (int j = 0; j < this.rooms.GetLength(1); ++j)
				{
					for (int i = 0; i < this.rooms.GetLength(0); ++i)
					{
						if (this.rooms[i, j] != null && this.rooms[i, j].Contains(center))
						{
							return this.rooms[i, j];
						}
					}
				}
				throw new ArgumentException($"Player location is invalid: {location}");
			}

			if (previous.Contains(center))
			{
				return previous;
			}
			Point dungeonIndex = previous.DungeonIndex;
			Room leftRoom = this.GetRoom(dungeonIndex.X - 1, dungeonIndex.Y);
			if (leftRoom != null && leftRoom.Contains(center))
			{
				return leftRoom;
			}
			Room upRoom = this.GetRoom(dungeonIndex.X, dungeonIndex.Y - 1);
			if (upRoom != null && upRoom.Contains(center))
			{
				return upRoom;
			}
			Room rightRoom = this.GetRoom(dungeonIndex.X + 1, dungeonIndex.Y);
			if (rightRoom != null && rightRoom.Contains(center))
			{
				return rightRoom;
			}
			Room downRoom = this.GetRoom(dungeonIndex.X, dungeonIndex.Y + 1);
			if (downRoom != null && downRoom.Contains(center))
			{
				return downRoom;
			}
			throw new ArgumentException($"Player location is invalid: {location}. Previous Room: {previous.DungeonIndex}");
		}

		private static bool IsRoomEmpty(TiledMapTileLayer layer, int startX, int startY)
		{
			for (int y = startY; y < startY + Room.NumTilesHeight; ++y)
			{
				for (int x = startX; x < startX + Room.NumTilesWidth; ++x)
				{
					// If one tile exists in this Room, then we consider it not empty.
					TiledMapTile? tile;
					if (layer.TryGetTile((ushort)x, (ushort)y, out tile))
					{
						if (!tile.Value.IsBlank) return false;
					}
				}
			}
			return true;
		}

		private Room RoomContains(Point point)
		{
			for (int j = 0; j < this.rooms.GetLength(1); ++j)
			{
				for (int i = 0; i < this.rooms.GetLength(0); ++i)
				{
					if (this.rooms[i, j] != null && this.rooms[i, j].Contains(point))
						return this.rooms[i, j];
				}
			}
			return null;
		}

		private Room GetRoom(int i, int j)
		{
			if (i < 0 || i >= this.rooms.GetLength(0) || j < 0 || j >= this.rooms.GetLength(1))
			{
				return null;
			}
			return this.rooms[i, j];
		}
	}
}
