using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lozo
{
	public class LozoGame : Game
	{
		public const int ScreenWidth = World.Width;
		public const int ScreenHeight = World.Height;

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D debugRect;
		SpriteFont debugFont;
		double framerate;
		World world;
		Player player;
		KeyboardState state;

		public LozoGame()
		{
			this.graphics = new GraphicsDeviceManager(this);
			this.graphics.PreferredBackBufferWidth = ScreenWidth;
			this.graphics.PreferredBackBufferHeight = ScreenHeight;
			this.graphics.ApplyChanges();
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			this.world = new World();
			this.player = new Player(this.world);
			this.world.AddPlayer(this.player);
			base.Initialize();
		}

		protected override void LoadContent()
		{
			this.spriteBatch = new SpriteBatch(GraphicsDevice);
			this.world.AddSpriteSheet(Content.Load<Texture2D>("Overworld"));
			this.player.AddSpriteSheet(Content.Load<Texture2D>("Link"));
			this.debugFont = Content.Load<SpriteFont>("Debug");
			this.debugRect = new Texture2D(GraphicsDevice, 1, 1);
			this.debugRect.SetData(new[] { Color.White });
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			this.spriteBatch.Dispose();
			this.debugRect.Dispose();
		}

		protected override void Update(GameTime gameTime)
		{
			this.framerate = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;

			this.state = Keyboard.GetState(); // TODO: GamePad support
			if (this.state.IsKeyDown(Keys.Escape)) Exit();

			this.world.Update(this.state);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			double drawFramerate = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;

			GraphicsDevice.Clear(new Color(252, 216, 168));
			this.spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw the world first so that the HUD is drawn on top of anything in the world.
			this.world.Draw(this.spriteBatch);

			// HUD
			// Map lines
			for (int x = 0; x < World.Width; x += World.TileWidth)
			{
				this.spriteBatch.Draw(this.debugRect, new Rectangle(x, 0, 1, World.Height), new Color(Color.Gray, 0.5f));
			}
			for (int y = 0; y < World.Height; y += World.TileHeight)
			{
				this.spriteBatch.Draw(this.debugRect, new Rectangle(0, y, World.Width, 1), new Color(Color.Gray, 0.5f));
			}

			// Controller buttons
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 50, 5, 20, 20),
				new Color(Color.Gray, this.state.IsKeyDown(Keys.Up) ? 0.1f : 0.5f));
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 75, 30, 20, 20),
				new Color(Color.Gray, this.state.IsKeyDown(Keys.Left) ? 0.1f : 0.5f));
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 50, 30, 20, 20),
				new Color(Color.Gray, this.state.IsKeyDown(Keys.Down) ? 0.1f : 0.5f));
			this.spriteBatch.Draw(
				this.debugRect,
				new Rectangle(ScreenWidth - 25, 30, 20, 20),
				new Color(Color.Gray, this.state.IsKeyDown(Keys.Right) ? 0.1f : 0.5f));

			// Frame rate
			this.spriteBatch.DrawString(
				this.debugFont,
				string.Format("FPS: {0:0.00}, {1:0.00}", this.framerate, drawFramerate),
				new Vector2(5, 5),
				Color.White,
				0,
				new Vector2(),
				1.0f,
				SpriteEffects.None,
				0.5f);

			this.spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
