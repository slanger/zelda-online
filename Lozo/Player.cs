using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Lozo
{
	public class Player
	{
		public const int SpriteWidth = 16;
		public const int SpriteHeight = 16;
		public const int Width = SpriteWidth * World.Scale;
		public const int Height = SpriteHeight * World.Scale;
		const int WalkSpeed = 4; // Based on Scale = 3
		const int WalkAnimationSpeed = 6; // Frames per animation key frame

		// When facing left, we use the right textures, but in the Draw() call, we flip the sprites
		// horizontally.
		// TODO: Consider flipping the sprites in the sprite sheet instead of at runtime.
		static readonly Sprite[] WalkingRightSprites = new[] { new Sprite(SpriteID.WalkRight1), new Sprite(SpriteID.WalkRight2) };
		static readonly Sprite[] WalkingUpSprites = new[] { new Sprite(SpriteID.WalkUp1), new Sprite(SpriteID.WalkUp2) };
		static readonly Sprite[] WalkingDownSprites = new[] { new Sprite(SpriteID.WalkDown1), new Sprite(SpriteID.WalkDown2) };
		static readonly Sprite[] AttackingRightSprites = new[] { new Sprite(SpriteID.AttackRight1), new Sprite(SpriteID.AttackRight2), new Sprite(SpriteID.AttackRight3), new Sprite(SpriteID.AttackRight4) };
		static readonly Sprite[] AttackingUpSprites = new[] { new Sprite(SpriteID.AttackUp1), new Sprite(SpriteID.AttackUp2), new Sprite(SpriteID.AttackUp3), new Sprite(SpriteID.AttackUp4) };
		static readonly Sprite[] AttackingDownSprites = new[] { new Sprite(SpriteID.AttackDown1), new Sprite(SpriteID.AttackDown2), new Sprite(SpriteID.AttackDown3), new Sprite(SpriteID.AttackDown4) };
		static readonly int[] AttackFramesPerKeyFrame = new[] { 4, 8, 1, 1 };

		Dungeon dungeon;
		Rectangle location;
		int dX;
		int dY;
		Direction direction;
		bool attemptedMoving;
		bool attacking;
		bool attackButtonPressed;
		Sprite[] currentSprites;
		int currentSpriteIndex;
		int numAnimationFrames;

		public Player(Dungeon currentDungeon, Point center, Direction facingDirection)
		{
			this.SetLocation(currentDungeon, center, facingDirection);
		}

		public void SetLocation(Dungeon dungeon, Point center, Direction direction)
		{
			this.dungeon = dungeon;
			this.location = new Rectangle(center.X - (Width / 2), center.Y - (Height / 2), Width, Height);
			//this.dungeon.UpdateCurrentRoom(this.location);
			this.direction = direction;
			this.currentSprites = GetWalkingSprites(direction);
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
								this.currentSprites = WalkingRightSprites;
								break;
							case Direction.Up:
								this.currentSprites = WalkingUpSprites;
								break;
							case Direction.Down:
								this.currentSprites = WalkingDownSprites;
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
						this.currentSprites = AttackingRightSprites;
						break;
					case Direction.Up:
						this.currentSprites = AttackingUpSprites;
						break;
					case Direction.Down:
						this.currentSprites = AttackingDownSprites;
						break;
				}
			}
			else
			{
				if (!state.IsKeyDown(Keys.X))
				{
					this.attackButtonPressed = false;
				}

				Rectangle walkingCollider = this.WalkingCollider();
				Direction oldDirection = this.direction;
				switch (this.GetMovementDirection(state))
				{
					case null: // Not moving
						break;
					case Direction.Left:
						walkingCollider.X -= WalkSpeed;
						this.currentSprites = WalkingRightSprites;
						break;
					case Direction.Right:
						walkingCollider.X += WalkSpeed;
						this.currentSprites = WalkingRightSprites;
						break;
					case Direction.Up:
						walkingCollider.Y -= WalkSpeed;
						this.currentSprites = WalkingUpSprites;
						break;
					case Direction.Down:
						walkingCollider.Y += WalkSpeed;
						this.currentSprites = WalkingDownSprites;
						break;
				}
				/*
				List<Rectangle> collided = this.dungeon.CollidingWith(walkingCollider);
				switch (this.direction)
				{
					case Direction.Left:
						foreach (Rectangle c in collided)
							walkingCollider.X = MathHelper.Max(walkingCollider.X, c.Right);
						break;
					case Direction.Right:
						int rightX = walkingCollider.Right;
						foreach (Rectangle c in collided)
							rightX = MathHelper.Min(rightX, c.X);
						walkingCollider.X = rightX - walkingCollider.Width;
						break;
					case Direction.Up:
						foreach (Rectangle c in collided)
							walkingCollider.Y = MathHelper.Max(walkingCollider.Y, c.Bottom);
						break;
					case Direction.Down:
						int bottomY = walkingCollider.Bottom;
						foreach (Rectangle c in collided)
							bottomY = MathHelper.Min(bottomY, c.Y);
						walkingCollider.Y = bottomY - walkingCollider.Height;
						break;
				}
				*/
				this.UpdateLocationFromWalking(walkingCollider);

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
			//Point center = this.dungeon.RelativeToCurrentRoom(this.location.Center);
			Point center = this.location.Center;
			currentSprite.Draw(
				spriteBatch,
				center.X,
				center.Y,
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

		public Rectangle WalkingCollider()
		{
			return new Rectangle(this.location.X + (3 * World.Scale), this.location.Y + (Height / 2), Width - (6 * World.Scale), Height / 2);
		}

		private void UpdateLocationFromWalking(Rectangle walkingCollider)
		{
			this.location = new Rectangle(walkingCollider.X - (3 * World.Scale), walkingCollider.Y - (Height / 2), Width, Height);
			//this.dungeon.UpdateCurrentRoom(this.location);
		}

		private static Sprite[] GetWalkingSprites(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left:
				case Direction.Right:
					return WalkingRightSprites;
				case Direction.Up:
					return WalkingUpSprites;
				case Direction.Down:
					return WalkingDownSprites;
			}
			throw new ArgumentException("Invalid direction: " + direction);
		}
	}
}
