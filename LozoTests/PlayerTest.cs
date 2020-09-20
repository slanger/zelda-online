using Lozo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using System;
using Xunit;
using Xunit.Abstractions;

namespace LozoTests
{
	public class PlayerTest : MakeConsoleWork
	{
		private const int ImmovableWidth = Player.Width;
		private const int ImmovableHeight = Player.Height;

		public PlayerTest(ITestOutputHelper output) : base(output) { }

		private static Player MakeWorldAndPlayer(params Rectangle[] immovables)
		{
			var world = new World();
			var map = new TiledMap("TestMap", Room.NumTilesWidth, Room.NumTilesHeight, Room.TileWidth, Room.TileHeight, TiledMapTileDrawOrder.RightDown, TiledMapOrientation.Orthogonal);
			var bottomLayer = new TiledMapTileLayer(Dungeon.BottomLayerName, Room.NumTilesWidth, Room.NumTilesHeight, Room.TileWidth, Room.TileHeight);
			for (int y = 0; y < bottomLayer.Height; ++y)
			{
				for (int x = 0; x < bottomLayer.Width; ++x)
				{
					bottomLayer.SetTile((ushort)x, (ushort)y, 1);
				}
			}
			map.AddLayer(bottomLayer);
			var collidables = new TiledMapRectangleObject[immovables.Length];
			int i = 0;
			foreach (Rectangle immovable in immovables)
			{
				// Note: Object IDs for Tiled maps start at 1, not 0.
				collidables[i] = new TiledMapRectangleObject(i + 1, "collidable" + (i + 1), new Size2(immovable.Width, immovable.Height), new Vector2(immovable.X, immovable.Y));
				++i;
			}
			var collisionLayer = new TiledMapObjectLayer(Dungeon.CollisionLayerName, collidables);
			map.AddLayer(collisionLayer);
			var dungeon = new Dungeon(world, map, null);
			world.Dungeons.Add(dungeon);
			var player = new Player(dungeon, new Point(Room.Width / 2, Room.Height / 2), Direction.Down);
			world.Player = player;
			return player;
		}

		// This method assumes that the player won't be stopped by any immovables.
		private static void MoveAndAssert(Player player, KeyboardState state, Direction? expectedDirection)
		{
			Point offset;
			switch (expectedDirection)
			{
				case Direction.Left:
					offset = new Point(-Player.WalkSpeed, 0);
					break;
				case Direction.Right:
					offset = new Point(Player.WalkSpeed, 0);
					break;
				case Direction.Up:
					offset = new Point(0, -Player.WalkSpeed);
					break;
				case Direction.Down:
					offset = new Point(0, Player.WalkSpeed);
					break;
				case null:
					offset = new Point(0, 0);
					break;
				default:
					throw new ArgumentException($"Invalid direction: {expectedDirection}");
			}
			Direction direction = expectedDirection == null ? player.Direction : expectedDirection.Value;
			MoveWithOffsetAndAssert(player, state, offset, direction);
		}

		// This method assumes that the player won't move due to immovables in the way.
		private static void HinderedMoveAndAssert(Player player, KeyboardState state, Direction expectedDirection)
		{
			MoveWithOffsetAndAssert(player, state, new Point(0, 0), expectedDirection);
		}

		private static void MoveWithOffsetAndAssert(Player player, KeyboardState state, Point expectedOffset, Direction expectedDirection)
		{
			Rectangle oldLocation = player.Location;
			player.Update(state);
			Assert.Equal(expectedDirection, player.Direction);
			Assert.Equal(new Rectangle(oldLocation.Location + expectedOffset, oldLocation.Size), player.Location);
		}

		[Fact]
		public void MovementWithZeroButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();
			MoveAndAssert(player, new KeyboardState(), null);
		}

		[Fact]
		public void MovementWithOneButtonPressed()
		{
			Player player = MakeWorldAndPlayer();
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
		}

		[Fact]
		public void MovementWithTwoButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();

			// Buttons cancel each other out.
			MoveAndAssert(player, new KeyboardState(Keys.Down, Keys.Up), null);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Right), null);

			// Zero buttons pressed, then two buttons pressed--horizontal keys take precedence.
			MoveAndAssert(player, new KeyboardState(), null);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			MoveAndAssert(player, new KeyboardState(), null);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Left);
			MoveAndAssert(player, new KeyboardState(), null);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);
			MoveAndAssert(player, new KeyboardState(), null);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			// One button pressed, then two buttons pressed.
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			// One button pressed, then two buttons pressed again, but with more frames in between.
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);

			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			// Two buttons pressed, then two other buttons pressed.
			MoveAndAssert(player, new KeyboardState(), null);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			MoveAndAssert(player, new KeyboardState(), null);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Down);

			MoveAndAssert(player, new KeyboardState(), null);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);

			// Three buttons pressed, then two buttons pressed.
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);

			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Down), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Up);

			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Down), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			// Four buttons pressed, then two buttons pressed.
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right, Keys.Down), null);
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
		}

		[Fact]
		public void MovementWithThreeButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Up, Keys.Right, Keys.Down), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Right, Keys.Down, Keys.Left), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Down, Keys.Left, Keys.Up), Direction.Left);
		}

		[Fact]
		public void MovementWithFourButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();
			// All four directions cancel each other out.
			MoveAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right, Keys.Down), null);
		}

		[Fact]
		public void MovementWithOneImmovable()
		{
			Player player = MakeWorldAndPlayer();
			Rectangle startingLocation = player.Location;
			Rectangle startingCollider = player.WalkingCollider();

			// Put an immovable below the player and check that the immovable stops the player.
			player = MakeWorldAndPlayer(new Rectangle(startingLocation.X, startingCollider.Bottom, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			// Check that the player can still move in other directions.
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			// Same as above, but with the immovable *above* the player. Note that because of the
			// shape of the walking collider, the immovable will actually be placed inside the
			// player's sprite (but outside the walking collider).
			player = MakeWorldAndPlayer(new Rectangle(startingLocation.X, startingCollider.Top - ImmovableHeight, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);

			// Same as above, but with the immovable to the right of the player.
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right, startingLocation.Y, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);

			// Same as above, but with the immovable to the left of the player.
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - ImmovableWidth, startingLocation.Y, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			// Now check that the player can move in the opposite direction immediately after
			// colliding with an immovable.
			player = MakeWorldAndPlayer(new Rectangle(startingLocation.X, startingCollider.Bottom, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			player = MakeWorldAndPlayer(new Rectangle(startingLocation.X, startingCollider.Top - ImmovableHeight, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right, startingLocation.Y, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - ImmovableWidth, startingLocation.Y, ImmovableWidth, ImmovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			// Check that the player can be stopped by a tiny immovable.
			player = MakeWorldAndPlayer(new Rectangle(startingLocation.Center.X, startingCollider.Bottom, 1, 1));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			player = MakeWorldAndPlayer(new Rectangle(startingLocation.Center.X, startingCollider.Top - 1, 1, 1));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right, startingLocation.Center.Y, 1, 1));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - 1, startingLocation.Center.Y, 1, 1));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);

			// Check that the player can be stopped by a large immovable.
			player = MakeWorldAndPlayer(new Rectangle(0, startingCollider.Bottom, Room.Width, Room.Height - startingCollider.Bottom));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			player = MakeWorldAndPlayer(new Rectangle(0, 0, Room.Width, startingCollider.Top));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right, 0, Room.Width - startingCollider.Right, Room.Height));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			player = MakeWorldAndPlayer(new Rectangle(0, 0, startingCollider.Left, Room.Height));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);

			// Check that the player is stopped by an immovable 1 pixel away and by an immovable
			// Player.WalkSpeed-1 pixels away. Also, check that the player is *not* stopped by an
			// immovable Player.WalkSpeed pixels away (which is another way of saying that the
			// player should move the full Player.WalkSpeed pixels distance).
			foreach (int offset in new int[] { 1, Player.WalkSpeed - 1, Player.WalkSpeed })
			{
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left, startingCollider.Bottom + offset, ImmovableWidth, ImmovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, offset), Direction.Down);
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left, startingCollider.Top - ImmovableHeight - offset, ImmovableWidth, ImmovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, -offset), Direction.Up);
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right + offset, startingCollider.Top, ImmovableWidth, ImmovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(offset, 0), Direction.Right);
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - ImmovableWidth - offset, startingCollider.Top, ImmovableWidth, ImmovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(-offset, 0), Direction.Left);
			}
		}

		[Fact]
		public void MovementWithMultipleImmovables()
		{
			Player player = MakeWorldAndPlayer();
			Rectangle startingCollider = player.WalkingCollider();

			// Put two immovables together so that the player will collide with both simultaneously
			// and check that they stop the player.
			var immovableA = new Rectangle(startingCollider.Center.X - ImmovableWidth, startingCollider.Bottom + 1, ImmovableWidth, ImmovableHeight);
			var immovableB = new Rectangle(startingCollider.Center.X, startingCollider.Bottom + 1, ImmovableWidth, ImmovableHeight);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, 1), Direction.Down);
			// Check that the player can still move in other directions.
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			// Stagger the two immovables, and check that the player is stopped by the closer
			// immovable.
			immovableB.Y += 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, 1), Direction.Down);
			// Swap the order of the two immovables and check again.
			immovableB.Y -= 1;
			immovableA.Y += 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, 1), Direction.Down);

			// Do the same tests above, but in the other three directions.

			immovableA = new Rectangle(startingCollider.Center.X - ImmovableWidth, startingCollider.Top - ImmovableHeight - 1, ImmovableWidth, ImmovableHeight);
			immovableB = new Rectangle(startingCollider.Center.X, startingCollider.Top - ImmovableHeight - 1, ImmovableWidth, ImmovableHeight);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, -1), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			immovableB.Y -= 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, -1), Direction.Up);
			immovableB.Y += 1;
			immovableA.Y -= 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, -1), Direction.Up);

			immovableA = new Rectangle(startingCollider.Right + 1, startingCollider.Center.Y - ImmovableHeight, ImmovableWidth, ImmovableHeight);
			immovableB = new Rectangle(startingCollider.Right + 1, startingCollider.Center.Y, ImmovableWidth, ImmovableHeight);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(1, 0), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			immovableB.X += 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(1, 0), Direction.Right);
			immovableB.Y -= 1;
			immovableA.Y += 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(1, 0), Direction.Right);

			immovableA = new Rectangle(startingCollider.Left - ImmovableWidth - 1, startingCollider.Center.Y - ImmovableHeight, ImmovableWidth, ImmovableHeight);
			immovableB = new Rectangle(startingCollider.Left - ImmovableWidth - 1, startingCollider.Center.Y, ImmovableWidth, ImmovableHeight);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(-1, 0), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			immovableB.X -= 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(-1, 0), Direction.Left);
			immovableB.Y += 1;
			immovableA.Y -= 1;
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(-1, 0), Direction.Left);
		}

		// TODO: Test trying to walk while attacking.

		// TODO: Test animation?
	}
}
