using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Globalization;

namespace Lozo
{
	public class LozoGame : Game
	{
		public const int ScreenWidth = World.Width;
		public const int ScreenHeight = World.Height;

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		World world;
		Player player;
		KeyboardState state;
		bool debugMode;
		bool debugButtonPressed;
		Texture2D debugRect;
		SpriteFont debugFont;
		double framerate;

		public LozoGame()
		{
			this.graphics = new GraphicsDeviceManager(this);
			this.graphics.PreferredBackBufferWidth = ScreenWidth;
			this.graphics.PreferredBackBufferHeight = ScreenHeight;
			this.graphics.ApplyChanges();
			this.Content.RootDirectory = "Content";
			this.IsMouseVisible = true;

			this.CreateWorld();
			var location = new Point(Room.Width + (Room.Width / 2), Room.Height / 2);
			this.player = new Player(this.world, location);
			this.world.AddPlayer(this.player);
		}

		protected override void LoadContent()
		{
			this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
			Sprite.AddSpriteSheet("overworld", this.Content.Load<Texture2D>("Overworld"));
			Sprite.AddSpriteSheet("link", this.Content.Load<Texture2D>("Link"));
			this.debugFont = this.Content.Load<SpriteFont>("Debug");
			this.debugRect = new Texture2D(this.GraphicsDevice, 1, 1);
			this.debugRect.SetData(new[] { Color.White });
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			this.spriteBatch?.Dispose();
			this.debugRect?.Dispose();
		}

		protected override void Update(GameTime gameTime)
		{
			this.framerate = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;

			this.state = Keyboard.GetState(); // TODO: GamePad support
			if (this.state.IsKeyDown(Keys.Escape)) Exit();
			if (this.debugButtonPressed)
			{
				if (this.state.IsKeyUp(Keys.D))
				{
					this.debugButtonPressed = false;
				}
			}
			else
			{
				if (this.state.IsKeyDown(Keys.D))
				{
					this.debugMode = !this.debugMode;
					this.debugButtonPressed = true;
				}
			}

			this.world.Update(this.state);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(new Color(252, 216, 168));
			this.spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw the world first so that the HUD is drawn on top of anything in the world.
			this.world.Draw(this.spriteBatch);

			// HUD
			if (this.debugMode)
			{
				this.DrawDebugInfo(gameTime);
			}

			this.spriteBatch.End();
			base.Draw(gameTime);
		}

		private void DrawDebugInfo(GameTime gameTime)
		{
			double drawFramerate = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;

			// Map lines
			for (int x = 0; x < Room.Width; x += Room.TileWidth)
			{
				this.spriteBatch.Draw(this.debugRect, new Rectangle(x, 0, 1, Room.Height), new Color(Color.Gray, 0.5f));
			}
			for (int y = 0; y < Room.Height; y += Room.TileHeight)
			{
				this.spriteBatch.Draw(this.debugRect, new Rectangle(0, y, Room.Width, 1), new Color(Color.Gray, 0.5f));
			}

			// Controller buttons
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 50, 5, 20, 20),
				new Color(Color.LightGray, this.state.IsKeyDown(Keys.Up) ? 1f : 0.7f));
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 75, 30, 20, 20),
				new Color(Color.LightGray, this.state.IsKeyDown(Keys.Left) ? 1f : 0.7f));
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 50, 30, 20, 20),
				new Color(Color.LightGray, this.state.IsKeyDown(Keys.Down) ? 1f : 0.7f));
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 25, 30, 20, 20),
				new Color(Color.LightGray, this.state.IsKeyDown(Keys.Right) ? 1f : 0.7f));

			// Frame rate
			this.spriteBatch.DrawString(
				this.debugFont,
				string.Format(CultureInfo.InvariantCulture, "FPS: {0:0.00}, {1:0.00}", this.framerate, drawFramerate),
				new Vector2(5, 5),
				new Color(31, 246, 31),
				0f,
				new Vector2(),
				1f,
				SpriteEffects.None,
				1f);

			// Player's walking collider
			Rectangle walkingCollider = this.player.WalkingCollider();
			Point relLocation = this.world.RelativeToCurrentRoom(walkingCollider.Location);
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(relLocation, walkingCollider.Size),
				new Color(Color.Red, 0.5f));
		}

		private void CreateWorld()
		{
			Tile[,] tiles;
			List<Rectangle> immovables;
			void createEmptyRoom()
			{
				tiles = new Tile[Room.NumTilesHeight, Room.NumTilesWidth];
				for (int y = 0; y < tiles.GetLength(0); ++y)
				{
					for (int x = 0; x < tiles.GetLength(1); ++x)
					{
						tiles[y, x] = new Tile(new Sprite(SpriteID.Floor), new Rectangle(x * Room.TileWidth, y * Room.TileHeight, Room.TileWidth, Room.TileHeight));
					}
				}
				immovables = new List<Rectangle>();
			}
			void addRock(int x, int y)
			{
				tiles[y, x].Sprite = new Sprite(SpriteID.Rock);
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

			var rooms = new Room[2, 2];
			rooms[room1.DungeonLocation.Y, room1.DungeonLocation.X] = room1;
			rooms[room2.DungeonLocation.Y, room2.DungeonLocation.X] = room2;
			rooms[room3.DungeonLocation.Y, room3.DungeonLocation.X] = room3;
			this.world = new World(rooms);
		}
	}
}
