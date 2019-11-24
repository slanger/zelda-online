﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lozo
{
	public class LozoGame : Game
	{
		const int Scale = 3;
		const int NumTilesWidth = 16;
		const int NumTilesHeight = 11;
		const int TileWidth = 16;
		const int TileHeight = 16;
		const int ScreenWidth = NumTilesWidth * TileWidth * Scale;
		const int ScreenHeight = NumTilesHeight * TileHeight * Scale;
		const int Speed = 4;
		const int LinkWidth = 16;
		const int LinkHeight = 16;
		const int AnimationSpeed = 6; // frames per animation key frame

		enum Direction
		{
			Left,
			Right,
			Up,
			Down
		}

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D linkSpritesheet;
		int linkX;
		int linkY;
		Direction direction;
		Rectangle[] leftTextures;
		Rectangle[] rightTextures;
		Rectangle[] upTextures;
		Rectangle[] downTextures;
		Rectangle[] curTextures;
		int curTextureIndex;
		int numAnimationFrames;

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
			this.linkX = 256;
			this.linkY = 256;
			this.direction = Direction.Down;
			this.leftTextures = new[] { new Rectangle(35, 11, LinkWidth, LinkHeight), new Rectangle(52, 11, LinkWidth, LinkWidth) }; // TODO: Create left textures
			this.rightTextures = new[] { new Rectangle(35, 11, LinkWidth, LinkHeight), new Rectangle(52, 11, LinkWidth, LinkWidth) };
			this.upTextures = new[] { new Rectangle(69, 11, LinkWidth, LinkHeight), new Rectangle(86, 11, LinkWidth, LinkWidth) };
			this.downTextures = new[] { new Rectangle(1, 11, LinkWidth, LinkHeight), new Rectangle(18, 11, LinkWidth, LinkWidth) };
			this.curTextures = this.downTextures;
			this.curTextureIndex = 0;
			this.numAnimationFrames = 0;
			System.Console.WriteLine("Viewport bounds: " + GraphicsDevice.Viewport.Bounds);
			System.Console.WriteLine("Screen size: " + ScreenWidth + ", " + ScreenHeight);
			base.Initialize();
		}

		protected override void LoadContent()
		{
			this.spriteBatch = new SpriteBatch(GraphicsDevice);
			this.linkSpritesheet = Content.Load<Texture2D>("link"); // TODO: Use Sprite class
		}

		protected override void Update(GameTime gameTime)
		{
			KeyboardState state = Keyboard.GetState(); // TODO: GamePad support
			if (state.IsKeyDown(Keys.Escape)) Exit();

			int oldX = this.linkX;
			int oldY = this.linkY;
			Direction oldDirection = this.direction;
			if (state.IsKeyDown(Keys.Left))
			{
				this.linkX -= Speed;
				this.direction = Direction.Left;
				this.curTextures = this.leftTextures;
			}
			if (state.IsKeyDown(Keys.Right))
			{
				this.linkX += Speed;
				this.direction = Direction.Right;
				this.curTextures = this.rightTextures;
			}
			if (state.IsKeyDown(Keys.Up))
			{
				this.linkY -= Speed;
				this.direction = Direction.Up;
				this.curTextures = this.upTextures;
			}
			if (state.IsKeyDown(Keys.Down))
			{
				this.linkY += Speed;
				this.direction = Direction.Down;
				this.curTextures = this.downTextures;
			}
			this.linkX = MathHelper.Clamp(this.linkX, (LinkWidth / 2) * Scale, ScreenWidth - ((LinkWidth / 2) * Scale));
			this.linkY = MathHelper.Clamp(this.linkY, (LinkHeight / 2) * Scale, ScreenHeight - ((LinkHeight / 2) * Scale));
			if (this.direction != oldDirection)
			{
				this.curTextureIndex = 0;
				this.numAnimationFrames = 0;
			}
			if (this.linkX != oldX || this.linkY != oldY)
			{
				System.Console.WriteLine(this.linkX + ", " + this.linkY);
				this.numAnimationFrames++;
				if (this.numAnimationFrames >= AnimationSpeed)
				{
					this.numAnimationFrames = 0;
					this.curTextureIndex++;
					if (this.curTextureIndex >= this.curTextures.Length)
					{
						this.curTextureIndex = 0;
					}
				}
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			this.spriteBatch.Begin(samplerState: SamplerState.PointClamp);
			this.spriteBatch.Draw(this.linkSpritesheet, new Vector2(this.linkX, this.linkY), this.curTextures[this.curTextureIndex], Color.White, 0.0f, new Vector2(LinkWidth / 2, LinkHeight / 2), Scale, SpriteEffects.None, 0.0f);
			this.spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}