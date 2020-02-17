using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lozo
{
	public class World
	{
		public const int Scale = 3;
		public const int Width = Room.Width;
		public const int Height = Room.Height;

		Player player;
		Room[] rooms;
		Sprite floorSprite;
		Sprite rockSprite;

		public World()
		{
			// The sprites get their actual values in the AddSpriteSheet method.
			this.floorSprite = new Sprite();
			this.rockSprite = new Sprite();

			Tile[][] tiles = new Tile[Room.NumTilesHeight][];
			for (int y = 0; y < tiles.Length; ++y)
			{
				tiles[y] = new Tile[Room.NumTilesWidth];
				for (int x = 0; x < tiles[y].Length; ++x)
				{
					tiles[y][x] = new Tile(this.floorSprite, new Rectangle(x * Room.TileWidth, y * Room.TileHeight, Room.TileWidth, Room.TileHeight));
				}
			}
			var immovables = new List<Rectangle>();
			tiles[3][3].Sprite = this.rockSprite;
			immovables.Add(new Rectangle(3 * Room.TileWidth, 3 * Room.TileHeight, Room.TileWidth, Room.TileHeight));
			tiles[3][12].Sprite = this.rockSprite;
			immovables.Add(new Rectangle(12 * Room.TileWidth, 3 * Room.TileHeight, Room.TileWidth, Room.TileHeight));
			tiles[7][3].Sprite = this.rockSprite;
			immovables.Add(new Rectangle(3 * Room.TileWidth, 7 * Room.TileHeight, Room.TileWidth, Room.TileHeight));
			tiles[7][12].Sprite = this.rockSprite;
			immovables.Add(new Rectangle(12 * Room.TileWidth, 7 * Room.TileHeight, Room.TileWidth, Room.TileHeight));
			this.rooms = new[] { new Room(tiles, immovables) };
			this.player = new Player(this.rooms[0], new Point(Room.Width / 2, Room.Height / 2));
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

		public void Update(KeyboardState state)
		{
			this.player.Update(state);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (Room r in this.rooms)
			{
				r.Draw(spriteBatch);
			}
			// Draw the player last so that the player is drawn on top of everything else in the
			// world.
			this.player.Draw(spriteBatch);
		}
	}
}
