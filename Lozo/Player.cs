using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lozo
{
	public class Player
	{
		const int SpriteWidth = 16;
		const int SpriteHeight = 16;
		const int Width = SpriteWidth * LozoGame.Scale;
		const int Height = SpriteHeight * LozoGame.Scale;
		const int WalkSpeed = 4; // Based on Scale = 3
		const int WalkAnimationSpeed = 6; // Frames per animation key frame

		Texture2D spritesheet;
		int x;
		int y;
		int dX;
		int dY;
		Direction direction;
		bool attemptedMoving;
		bool attacking;
		bool attackButtonPressed;
		Rectangle[] rightTextures;
		Rectangle[] upTextures;
		Rectangle[] downTextures;
		Rectangle[] attackRightTextures;
		Rectangle[] attackUpTextures;
		Rectangle[] attackDownTextures;
		int[] attackFramesPerKeyFrame;
		Rectangle[] curTextures;
		int curTextureIndex;
		int numAnimationFrames;

		public Player()
		{
			this.x = LozoGame.ScreenWidth / 2;
			this.y = LozoGame.ScreenHeight / 2;
			this.direction = Direction.Down;
			// When facing left, we use the right textures, but in the Draw() call, we flip the
			// sprites horizontally.
			// TODO: Consider flipping the sprites in the sprite sheet instead of at runtime.
			this.rightTextures = new[] { new Rectangle(35, 11, SpriteWidth, SpriteHeight), new Rectangle(52, 11, SpriteWidth, SpriteHeight) };
			this.upTextures = new[] { new Rectangle(69, 11, SpriteWidth, SpriteHeight), new Rectangle(86, 11, SpriteWidth, SpriteHeight) };
			this.downTextures = new[] { new Rectangle(1, 11, SpriteWidth, SpriteHeight), new Rectangle(18, 11, SpriteWidth, SpriteHeight) };
			this.attackRightTextures = new[] { new Rectangle(1, 77, SpriteWidth, SpriteHeight), new Rectangle(18, 77, 27, SpriteHeight), new Rectangle(46, 77, 23, SpriteHeight), new Rectangle(70, 77, 19, SpriteHeight) };
			this.attackUpTextures = new[] { new Rectangle(1, 109, SpriteWidth, SpriteHeight), new Rectangle(18, 97, SpriteWidth, 28), new Rectangle(35, 98, SpriteWidth, 27), new Rectangle(52, 106, SpriteWidth, 19) };
			this.attackDownTextures = new[] { new Rectangle(1, 47, SpriteWidth, SpriteHeight), new Rectangle(18, 47, SpriteWidth, 27), new Rectangle(35, 47, SpriteWidth, 23), new Rectangle(52, 47, SpriteWidth, 19) };
			this.attackFramesPerKeyFrame = new[] { 4, 8, 1, 1 };
			this.curTextures = this.downTextures;
		}

		public void AddSpriteSheet(Texture2D spritesheet)
		{
			this.spritesheet = spritesheet;
		}

		public void Update(KeyboardState state)
		{
			if (this.attacking)
			{
				this.numAnimationFrames++;
				if (this.numAnimationFrames >= this.attackFramesPerKeyFrame[this.curTextureIndex])
				{
					this.numAnimationFrames = 0;
					this.curTextureIndex++;
					if (this.curTextureIndex >= this.curTextures.Length)
					{
						this.attacking = false;
						this.curTextureIndex = 0;
						this.numAnimationFrames = 0;
						switch (this.direction)
						{
							case Direction.Left:
							case Direction.Right:
								this.curTextures = this.rightTextures;
								break;
							case Direction.Up:
								this.curTextures = this.upTextures;
								break;
							case Direction.Down:
								this.curTextures = this.downTextures;
								break;
						}
					}
				}
			}
			else if (!this.attackButtonPressed && state.IsKeyDown(Keys.X))
			{
				this.attacking = true;
				this.attackButtonPressed = true;
				this.curTextureIndex = 0;
				this.numAnimationFrames = 0;
				switch (this.direction)
				{
					case Direction.Left:
					case Direction.Right:
						this.curTextures = this.attackRightTextures;
						break;
					case Direction.Up:
						this.curTextures = this.attackUpTextures;
						break;
					case Direction.Down:
						this.curTextures = this.attackDownTextures;
						break;
				}
			}
			else
			{
				if (!state.IsKeyDown(Keys.X))
				{
					this.attackButtonPressed = false;
				}

				int oldX = this.x;
				int oldY = this.y;
				Direction oldDirection = this.direction;
				switch (this.GetMovementDirection(state))
				{
					case null: // Not moving
						break;
					case Direction.Left:
						this.x -= WalkSpeed;
						this.curTextures = this.rightTextures;
						break;
					case Direction.Right:
						this.x += WalkSpeed;
						this.curTextures = this.rightTextures;
						break;
					case Direction.Up:
						this.y -= WalkSpeed;
						this.curTextures = this.upTextures;
						break;
					case Direction.Down:
						this.y += WalkSpeed;
						this.curTextures = this.downTextures;
						break;
				}
				this.x = MathHelper.Clamp(this.x, Width / 2, LozoGame.ScreenWidth - (Width / 2));
				this.y = MathHelper.Clamp(this.y, Height / 2, LozoGame.ScreenHeight - (Height / 2));
				if (this.direction != oldDirection)
				{
					this.curTextureIndex = 0;
					this.numAnimationFrames = 0;
				}
				if (this.x != oldX || this.y != oldY)
				{
					this.numAnimationFrames++;
					if (this.numAnimationFrames >= WalkAnimationSpeed)
					{
						this.numAnimationFrames = 0;
						this.curTextureIndex++;
						if (this.curTextureIndex >= this.curTextures.Length)
						{
							this.curTextureIndex = 0;
						}
					}
				}
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			int originX = SpriteWidth / 2;
			int originY = SpriteHeight / 2;
			if (this.direction == Direction.Left)
			{
				originX = this.curTextures[this.curTextureIndex].Width - (SpriteWidth / 2);
			}
			else if (this.direction == Direction.Up)
			{
				originY = this.curTextures[this.curTextureIndex].Height - (SpriteHeight / 2);
			}
			spriteBatch.Draw(
				this.spritesheet,
				new Vector2(this.x, this.y),
				this.curTextures[this.curTextureIndex],
				Color.White,
				0f,
				new Vector2(originX, originY),
				LozoGame.Scale,
				(this.direction == Direction.Left) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0f);
		}

		public Direction? GetMovementDirection(KeyboardState state)
		{
			List<Direction> directions = new List<Direction>(4);
			int oldDX = this.dX;
			int oldDY = this.dY;
			bool oldAttemptedMoving = this.attemptedMoving;
			this.dX = 0;
			this.dY = 0;
			this.attemptedMoving = false;
			if (state.IsKeyDown(Keys.Left)) { directions.Add(Direction.Left); --this.dX; }
			if (state.IsKeyDown(Keys.Right)) { directions.Add(Direction.Right); ++this.dX; }
			if (state.IsKeyDown(Keys.Up)) { directions.Add(Direction.Up); --this.dY; }
			if (state.IsKeyDown(Keys.Down)) { directions.Add(Direction.Down); ++this.dY; }

			if (directions.Count == 1)
			{
				this.attemptedMoving = true;
				this.direction = directions[0];
				return this.direction;
			}
			if (directions.Count == 2)
			{
				if (oldAttemptedMoving)
				{
					if (this.dX == oldDX && this.dY == oldDY)
					{
						// If the buttons pressed are the same as last update, then keep moving in
						// the same direction.
						this.attemptedMoving = true;
						return this.direction;
					}
					if (this.dX == 0 || this.dY == 0)
					{
						return null;
					}

					this.attemptedMoving = true;
					if (this.dX == oldDX)
					{
						this.direction = (this.dY == 1) ? Direction.Down : Direction.Up;
						return this.direction;
					}
					else /* this.dY == oldDY */
					{
						this.direction = (this.dX == 1) ? Direction.Right : Direction.Left;
						return this.direction;
					}
				}
				else
				{
					// This means that we pressed two movement buttons at the same time. Since we
					// only allow the player to move in one direction at a time, we need to pick
					// which directions to prioritize over the others. In this case, I've
					// prioritized the left and right directions over the up and down directions.
					if (this.dX != 0)
					{
						this.attemptedMoving = true;
						this.direction = (this.dX == 1) ? Direction.Right : Direction.Left;
						return this.direction;
					}
					return null; // dX == 0 && dY == 0
				}
			}
			if (directions.Count == 3)
			{
				// If 3 buttons are pressed, that means that 2 buttons cancel each other out,
				// leaving one button left to determine the movement direction.
				this.attemptedMoving = true;
				if (this.dX == 0)
				{
					this.direction = (this.dY == 1) ? Direction.Down : Direction.Up;
					return this.direction;
				}
				else /* dY == 0 */
				{
					this.direction = (this.dX == 1) ? Direction.Right : Direction.Left;
					return this.direction;
				}
			}
			// If no buttons are pressed, then we're not moving.
			// If all 4 buttons are pressed, that means that they all cancel each other out.
			return null;
		}
	}
}
