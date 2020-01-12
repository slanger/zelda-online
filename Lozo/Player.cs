using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lozo
{
	public class Player
	{
		const int Speed = 4;
		const int Width = 16;
		const int Height = 16;
		const int AnimationSpeed = 6; // Frames per animation key frame

		Texture2D spritesheet;
		int x;
		int y;
		int dX;
		int dY;
		Direction direction;
		bool attemptedMoving;
		Rectangle[] leftTextures;
		Rectangle[] rightTextures;
		Rectangle[] upTextures;
		Rectangle[] downTextures;
		Rectangle[] curTextures;
		int curTextureIndex;
		int numAnimationFrames;

		public Player()
		{
			this.x = 256;
			this.y = 256;
			this.direction = Direction.Down;
			// The coordinates for the left textures are very different from the other coordinates
			// because the original sprite sheet did not have left textures, so I added them in a
			// different spot.
			this.leftTextures = new[] { new Rectangle(297, 224, Width, Height), new Rectangle(314, 224, Width, Width) };
			this.rightTextures = new[] { new Rectangle(35, 11, Width, Height), new Rectangle(52, 11, Width, Width) };
			this.upTextures = new[] { new Rectangle(69, 11, Width, Height), new Rectangle(86, 11, Width, Width) };
			this.downTextures = new[] { new Rectangle(1, 11, Width, Height), new Rectangle(18, 11, Width, Width) };
			this.curTextures = this.downTextures;
		}

		public void AddSpriteSheet(Texture2D spritesheet)
		{
			this.spritesheet = spritesheet;
		}

		public void Update(KeyboardState state)
		{
			int oldX = this.x;
			int oldY = this.y;
			Direction oldDirection = this.direction;
			switch (this.GetMovementDirection(state))
			{
				case null: // Not moving
					break;
				case Direction.Left:
					this.x -= Speed;
					this.curTextures = this.leftTextures;
					break;
				case Direction.Right:
					this.x += Speed;
					this.curTextures = this.rightTextures;
					break;
				case Direction.Up:
					this.y -= Speed;
					this.curTextures = this.upTextures;
					break;
				case Direction.Down:
					this.y += Speed;
					this.curTextures = this.downTextures;
					break;
			}
			this.x = MathHelper.Clamp(this.x, (Width / 2) * LozoGame.Scale, LozoGame.ScreenWidth - ((Width / 2) * LozoGame.Scale));
			this.y = MathHelper.Clamp(this.y, (Height / 2) * LozoGame.Scale, LozoGame.ScreenHeight - ((Height / 2) * LozoGame.Scale));
			if (this.direction != oldDirection)
			{
				this.curTextureIndex = 0;
				this.numAnimationFrames = 0;
			}
			if (this.x != oldX || this.y != oldY)
			{
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
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(
				this.spritesheet,
				new Vector2(this.x, this.y),
				this.curTextures[this.curTextureIndex],
				Color.White,
				0.0f,
				new Vector2(Width / 2, Height / 2),
				LozoGame.Scale,
				SpriteEffects.None,
				0.0f);
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
