using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace Lozo
{
	public class LozoGame : Game
	{
		public const int ScreenWidth = Room.Width;
		public const int ScreenHeight = Room.Height;

		GraphicsDeviceManager graphics;
		BoxingViewportAdapter viewportAdapter;
		SpriteBatch spriteBatch;
		OrthographicCamera camera;
		TiledMapRenderer mapRenderer;
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
		}

		protected override void LoadContent()
		{
			this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
			Sprite.AddSpriteSheet("overworld", this.Content.Load<Texture2D>("Overworld"));
			Sprite.AddSpriteSheet("link", this.Content.Load<Texture2D>("Link"));
			this.world = new World();
			this.viewportAdapter = new BoxingViewportAdapter(this.Window, this.GraphicsDevice, ScreenWidth, ScreenHeight);
			this.camera = new OrthographicCamera(this.viewportAdapter);
			this.world.Camera = this.camera;
			var map = this.Content.Load<TiledMap>("Dungeon");
			this.mapRenderer = new TiledMapRenderer(this.GraphicsDevice, map);
			var dungeon = new Dungeon(map, this.mapRenderer);
			this.world.Dungeons.Add(dungeon);
			var startingPoint = new Point(1920, 3000);
			this.player = new Player(dungeon, startingPoint, Direction.Down);
			this.camera.LookAt(new Vector2(startingPoint.X, startingPoint.Y));
			this.world.Player = this.player;
			this.debugFont = this.Content.Load<SpriteFont>("Debug");
			this.debugRect = new Texture2D(this.GraphicsDevice, 1, 1);
			this.debugRect.SetData(new[] { Color.White });
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			this.graphics.Dispose();
			this.spriteBatch?.Dispose();
			this.viewportAdapter?.Dispose();
			this.mapRenderer?.Dispose();
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

			if (state.IsKeyDown(Keys.A))
			{
				this.camera.ZoomIn(0.01f);
			}
			if (state.IsKeyDown(Keys.Z))
			{
				this.camera.ZoomOut(0.01f);
			}

			this.world.Update(this.state);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this.GraphicsDevice.Clear(new Color(252, 216, 168));

			// Draw the world first so that the HUD is drawn on top of anything in the world.
			this.world.Draw(this.spriteBatch);

			// HUD
			if (this.debugMode)
			{
				this.DrawDebugInfo(gameTime);
			}

			base.Draw(gameTime);
		}

		private void DrawDebugInfo(GameTime gameTime)
		{
			double drawFramerate = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;
			this.spriteBatch.Begin(transformMatrix: this.camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.BackToFront);

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
				$"FPS: {this.framerate:0.00}, {drawFramerate:0.00}",
				new Vector2(5, 5),
				new Color(31, 246, 31),
				0f,
				new Vector2(),
				1f,
				SpriteEffects.None,
				1f);

			/*
			// Player's walking collider
			Rectangle walkingCollider = this.player.WalkingCollider();
			Point relLocation = this.world.RelativeToCurrentRoom(walkingCollider.Location);
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(relLocation, walkingCollider.Size),
				new Color(Color.Red, 0.5f));
			*/

			this.spriteBatch.End();
		}
	}
}
