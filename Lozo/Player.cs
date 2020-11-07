using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Lozo
{
	public class Player
	{
		public const int Width = 48;  // pixels
		public const int Height = 48;  // pixels
		public const int WalkSpeed = 4;  // pixels per frame

		const int HorizontalWiggleRoom = 9;  // extra buffer for player to squeeze through openings
		const int WalkingColliderOffsetX = HorizontalWiggleRoom;
		const int WalkingColliderOffsetY = Height / 2;
		const int WalkingColldierWidth = Width - (HorizontalWiggleRoom * 2);
		const int WalkingColliderHeight = Height / 2;

		const int WalkAnimationSpeed = 6;  // frames per animation key frame

		static readonly Sprite[] WalkingLeftSprites = new[] { new Sprite(SpriteID.WalkLeft1), new Sprite(SpriteID.WalkLeft2) };
		static readonly Sprite[] WalkingRightSprites = new[] { new Sprite(SpriteID.WalkRight1), new Sprite(SpriteID.WalkRight2) };
		static readonly Sprite[] WalkingUpSprites = new[] { new Sprite(SpriteID.WalkUp1), new Sprite(SpriteID.WalkUp2) };
		static readonly Sprite[] WalkingDownSprites = new[] { new Sprite(SpriteID.WalkDown1), new Sprite(SpriteID.WalkDown2) };
		static readonly Sprite[] AttackingLeftSprites = new[] { new Sprite(SpriteID.AttackLeft1), new Sprite(SpriteID.AttackLeft2), new Sprite(SpriteID.AttackLeft3), new Sprite(SpriteID.AttackLeft4) };
		static readonly Sprite[] AttackingRightSprites = new[] { new Sprite(SpriteID.AttackRight1), new Sprite(SpriteID.AttackRight2), new Sprite(SpriteID.AttackRight3), new Sprite(SpriteID.AttackRight4) };
		static readonly Sprite[] AttackingUpSprites = new[] { new Sprite(SpriteID.AttackUp1), new Sprite(SpriteID.AttackUp2), new Sprite(SpriteID.AttackUp3), new Sprite(SpriteID.AttackUp4) };
		static readonly Sprite[] AttackingDownSprites = new[] { new Sprite(SpriteID.AttackDown1), new Sprite(SpriteID.AttackDown2), new Sprite(SpriteID.AttackDown3), new Sprite(SpriteID.AttackDown4) };
		static readonly int[] AttackFramesPerKeyFrame = new[] { 4, 8, 1, 1 };

		public Dungeon CurrentDungeon { get; private set; }

		public Room CurrentRoom { get; private set; }

		public Rectangle Location { get; private set; }

		public Direction Direction { get; private set; }

		int dX;
		int dY;
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
						this.currentSprites = GetWalkingSprites(this.Direction);
					}
				}
			}
			else if (!this.attackButtonPressed && state.IsKeyDown(Keys.X))
			{
				this.attacking = true;
				this.attackButtonPressed = true;
				this.currentSpriteIndex = 0;
				this.numAnimationFrames = 0;
				switch (this.Direction)
				{
					case Direction.Left:
						this.currentSprites = AttackingLeftSprites;
						break;
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
				Direction oldDirection = this.Direction;
				switch (this.GetMovementDirection(state))
				{
					case null:  // not moving
						break;
					case Direction.Left:
						walkingCollider.X -= WalkSpeed;
						this.currentSprites = WalkingLeftSprites;
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
				List<Rectangle> collided = this.CurrentRoom.CollidingWith(walkingCollider);
				switch (this.Direction)
				{
					case Direction.Left:
						foreach (Rectangle c in collided)
							walkingCollider.X = Math.Max(walkingCollider.X, c.Right);
						break;
					case Direction.Right:
						int rightX = walkingCollider.Right;
						foreach (Rectangle c in collided)
							rightX = Math.Min(rightX, c.X);
						walkingCollider.X = rightX - walkingCollider.Width;
						break;
					case Direction.Up:
						foreach (Rectangle c in collided)
							walkingCollider.Y = Math.Max(walkingCollider.Y, c.Bottom);
						break;
					case Direction.Down:
						int bottomY = walkingCollider.Bottom;
						foreach (Rectangle c in collided)
							bottomY = Math.Min(bottomY, c.Y);
						walkingCollider.Y = bottomY - walkingCollider.Height;
						break;
				}
				this.UpdateLocationFromWalking(walkingCollider);

				if (this.attemptedMoving)
				{
					if (this.Direction != oldDirection)
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
			int originX = 0;
			int originY = 0;
			if (this.Direction == Direction.Left)
			{
				originX = currentSprite.Source.Width - Width;
			}
			else if (this.Direction == Direction.Up)
			{
				originY = currentSprite.Source.Height - Height;
			}
			currentSprite.Draw(
				spriteBatch,
				this.Location.X,
				this.Location.Y,
				originX,
				originY);
		}

		public Rectangle WalkingCollider()
		{
			return new Rectangle(
				this.Location.X + WalkingColliderOffsetX, this.Location.Y + WalkingColliderOffsetY,
				WalkingColldierWidth, WalkingColliderHeight);
		}

		private static Sprite[] GetWalkingSprites(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left:
					return WalkingLeftSprites;
				case Direction.Right:
					return WalkingRightSprites;
				case Direction.Up:
					return WalkingUpSprites;
				case Direction.Down:
					return WalkingDownSprites;
			}
			throw new ArgumentException($"Invalid direction: {direction}");
		}

		private Direction? GetMovementDirection(KeyboardState state)
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
				this.Direction = directions[0];
				return this.Direction;
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
						return this.Direction;
					}
					if (this.dX == 0 || this.dY == 0)
					{
						return null;
					}

					this.attemptedMoving = true;
					if (this.dX == oldDX)
					{
						this.Direction = (this.dY == 1) ? Direction.Down : Direction.Up;
						return this.Direction;
					}
					else /* this.dY == oldDY */
					{
						this.Direction = (this.dX == 1) ? Direction.Right : Direction.Left;
						return this.Direction;
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
						this.Direction = (this.dX == 1) ? Direction.Right : Direction.Left;
						return this.Direction;
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
					this.Direction = (this.dY == 1) ? Direction.Down : Direction.Up;
					return this.Direction;
				}
				else /* dY == 0 */
				{
					this.Direction = (this.dX == 1) ? Direction.Right : Direction.Left;
					return this.Direction;
				}
			}
			// If no buttons are pressed, then we're not moving.
			// If all 4 buttons are pressed, that means that they all cancel each other out.
			return null;
		}

		private void SetLocation(Dungeon dungeon, Point center, Direction direction)
		{
			this.CurrentDungeon = dungeon;
			this.Location = new Rectangle(center.X - (Width / 2), center.Y - (Height / 2), Width, Height);
			this.CurrentRoom = dungeon.GetCurrentRoom(this.CurrentRoom, this.Location);
			this.Direction = direction;
			this.currentSprites = GetWalkingSprites(direction);
		}

		private void UpdateLocationFromWalking(Rectangle walkingCollider)
		{
			var location = new Rectangle(
				walkingCollider.X - WalkingColliderOffsetX, walkingCollider.Y - WalkingColliderOffsetY, Width, Height);
			this.SetLocation(this.CurrentDungeon, location.Center, this.Direction);
		}
	}
}
