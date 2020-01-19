using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lozo
{
	public class LozoGame : Game
	{
		public const int Scale = 3;
		public const int NumTilesWidth = 16;
		public const int NumTilesHeight = 11;
		public const int TileWidth = 16 * Scale;
		public const int TileHeight = 16 * Scale;
		public const int ScreenWidth = NumTilesWidth * TileWidth;
		public const int ScreenHeight = NumTilesHeight * TileHeight;

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D debugRect;
		SpriteFont debugFont;
		double framerate;
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
			this.player = new Player();
			base.Initialize();
		}

		protected override void LoadContent()
		{
			this.spriteBatch = new SpriteBatch(GraphicsDevice);
			Texture2D spritesheet = Content.Load<Texture2D>("Link"); // TODO: Use Sprite class
			this.player.AddSpriteSheet(spritesheet);
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

			this.player.Update(this.state);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			double drawFramerate = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;

			GraphicsDevice.Clear(new Color(252, 216, 168));
			this.spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			this.player.Draw(this.spriteBatch);

			// HUD
			// Controller buttons
			if (this.state.IsKeyDown(Keys.Up))
			{
				this.spriteBatch.Draw(
					this.debugRect,
					new Rectangle(ScreenWidth - 50, 5, 20, 20),
					new Color(Color.Red, 0.1f));
			}
			if (this.state.IsKeyDown(Keys.Left))
			{
				this.spriteBatch.Draw(
					this.debugRect,
					new Rectangle(ScreenWidth - 75, 30, 20, 20),
					new Color(Color.Red, 0.1f));
			}
			if (this.state.IsKeyDown(Keys.Down))
			{
				this.spriteBatch.Draw(
					this.debugRect,
					new Rectangle(ScreenWidth - 50, 30, 20, 20),
					new Color(Color.Red, 0.1f));
			}
			if (this.state.IsKeyDown(Keys.Right))
			{
				this.spriteBatch.Draw(
					this.debugRect,
					new Rectangle(ScreenWidth - 25, 30, 20, 20),
					new Color(Color.Red, 0.1f));
			}

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
