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
		const int Width = SpriteWidth * World.Scale;
		const int Height = SpriteHeight * World.Scale;
		const int WalkSpeed = 4; // Based on Scale = 3
		const int WalkAnimationSpeed = 6; // Frames per animation key frame

		static readonly int[] AttackFramesPerKeyFrame = new[] { 4, 8, 1, 1 };

		Room currentRoom;
		Rectangle collider;
		int dX;
		int dY;
		Direction direction;
		bool attemptedMoving;
		bool attacking;
		bool attackButtonPressed;
		Sprite[] walkingRightSprites;
		Sprite[] walkingUpSprites;
		Sprite[] walkingDownSprites;
		Sprite[] attackingRightSprites;
		Sprite[] attackingUpSprites;
		Sprite[] attackingDownSprites;
		Sprite[] currentSprites;
		int currentSpriteIndex;
		int numAnimationFrames;

		public Player(Room currentRoom, Point center)
		{
			this.currentRoom = currentRoom;
			this.collider = new Rectangle(center.X - (Width / 2), center.Y - (Height / 2), Width, Height);
			this.direction = Direction.Down;
		}

		public void AddSpriteSheet(Texture2D spritesheet)
		{
			// When facing left, we use the right textures, but in the Draw() call, we flip the
			// sprites horizontally.
			// TODO: Consider flipping the sprites in the sprite sheet instead of at runtime.
			this.walkingRightSprites = new[] { new Sprite(spritesheet, new Rectangle(35, 11, SpriteWidth, SpriteHeight)), new Sprite(spritesheet, new Rectangle(52, 11, SpriteWidth, SpriteHeight)) };
			this.walkingUpSprites = new[] { new Sprite(spritesheet, new Rectangle(69, 11, SpriteWidth, SpriteHeight)), new Sprite(spritesheet, new Rectangle(86, 11, SpriteWidth, SpriteHeight)) };
			this.walkingDownSprites = new[] { new Sprite(spritesheet, new Rectangle(1, 11, SpriteWidth, SpriteHeight)), new Sprite(spritesheet, new Rectangle(18, 11, SpriteWidth, SpriteHeight)) };
			this.attackingRightSprites = new[] { new Sprite(spritesheet, new Rectangle(1, 77, SpriteWidth, SpriteHeight)), new Sprite(spritesheet, new Rectangle(18, 77, 27, SpriteHeight)), new Sprite(spritesheet, new Rectangle(46, 77, 23, SpriteHeight)), new Sprite(spritesheet, new Rectangle(70, 77, 19, SpriteHeight)) };
			this.attackingUpSprites = new[] { new Sprite(spritesheet, new Rectangle(1, 109, SpriteWidth, SpriteHeight)), new Sprite(spritesheet, new Rectangle(18, 97, SpriteWidth, 28)), new Sprite(spritesheet, new Rectangle(35, 98, SpriteWidth, 27)), new Sprite(spritesheet, new Rectangle(52, 106, SpriteWidth, 19)) };
			this.attackingDownSprites = new[] { new Sprite(spritesheet, new Rectangle(1, 47, SpriteWidth, SpriteHeight)), new Sprite(spritesheet, new Rectangle(18, 47, SpriteWidth, 27)), new Sprite(spritesheet, new Rectangle(35, 47, SpriteWidth, 23)), new Sprite(spritesheet, new Rectangle(52, 47, SpriteWidth, 19)) };
			this.currentSprites = this.walkingDownSprites;
		}

		public void Update(KeyboardState state)
		{
			if (this.attacking)
			{
				this.numAnimationFrames++;
				if (this.numAnimationFrames >= AttackFramesPerKeyFrame[this.currentSpriteIndex])
				{
					this.numAnimationFrames = 0;
					this.currentSpriteIndex++;
					if (this.currentSpriteIndex >= this.currentSprites.Length)
					{
						this.attacking = false;
						this.currentSpriteIndex = 0;
						this.numAnimationFrames = 0;
						switch (this.direction)
						{
							case Direction.Left:
							case Direction.Right:
								this.currentSprites = this.walkingRightSprites;
								break;
							case Direction.Up:
								this.currentSprites = this.walkingUpSprites;
								break;
							case Direction.Down:
								this.currentSprites = this.walkingDownSprites;
								break;
						}
					}
				}
			}
			else if (!this.attackButtonPressed && state.IsKeyDown(Keys.X))
			{
				this.attacking = true;
				this.attackButtonPressed = true;
				this.currentSpriteIndex = 0;
				this.numAnimationFrames = 0;
				switch (this.direction)
				{
					case Direction.Left:
					case Direction.Right:
						this.currentSprites = this.attackingRightSprites;
						break;
					case Direction.Up:
						this.currentSprites = this.attackingUpSprites;
						break;
					case Direction.Down:
						this.currentSprites = this.attackingDownSprites;
						break;
				}
			}
			else
			{
				if (!state.IsKeyDown(Keys.X))
				{
					this.attackButtonPressed = false;
				}

				Direction oldDirection = this.direction;
				switch (this.GetMovementDirection(state))
				{
					case null: // Not moving
						break;
					case Direction.Left:
						this.collider.X -= WalkSpeed;
						this.currentSprites = this.walkingRightSprites;
						break;
					case Direction.Right:
						this.collider.X += WalkSpeed;
						this.currentSprites = this.walkingRightSprites;
						break;
					case Direction.Up:
						this.collider.Y -= WalkSpeed;
						this.currentSprites = this.walkingUpSprites;
						break;
					case Direction.Down:
						this.collider.Y += WalkSpeed;
						this.currentSprites = this.walkingDownSprites;
						break;
				}
				List<Rectangle> collided = this.currentRoom.CollidingWith(this.collider);
				switch (this.direction)
				{
					case Direction.Left:
						foreach (Rectangle c in collided)
							this.collider.X = MathHelper.Max(this.collider.X, c.Right);
						break;
					case Direction.Right:
						int rightX = this.collider.Right;
						foreach (Rectangle c in collided)
							rightX = MathHelper.Min(rightX, c.X);
						this.collider.X = rightX - this.collider.Width;
						break;
					case Direction.Up:
						foreach (Rectangle c in collided)
							this.collider.Y = MathHelper.Max(this.collider.Y, c.Bottom);
						break;
					case Direction.Down:
						int bottomY = this.collider.Bottom;
						foreach (Rectangle c in collided)
							bottomY = MathHelper.Min(bottomY, c.Y);
						this.collider.Y = bottomY - this.collider.Height;
						break;
				}
				if (this.attemptedMoving)
				{
					if (this.direction != oldDirection)
					{
						this.currentSpriteIndex = 0;
						this.numAnimationFrames = 0;
					}
					else
					{
						++this.numAnimationFrames;
						if (this.numAnimationFrames >= WalkAnimationSpeed)
						{
							this.numAnimationFrames = 0;
							++this.currentSpriteIndex;
							if (this.currentSpriteIndex >= this.currentSprites.Length)
							{
								this.currentSpriteIndex = 0;
							}
						}
					}
				}
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			Sprite currentSprite = this.currentSprites[this.currentSpriteIndex];
			int originX = SpriteWidth / 2;
			int originY = SpriteHeight / 2;
			if (this.direction == Direction.Left)
			{
				originX = currentSprite.Source.Width - (SpriteWidth / 2);
			}
			else if (this.direction == Direction.Up)
			{
				originY = currentSprite.Source.Height - (SpriteHeight / 2);
			}
			currentSprite.Draw(
				spriteBatch,
				this.collider.Center.X,
				this.collider.Center.Y,
				originX,
				originY,
				(this.direction == Direction.Left) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
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
