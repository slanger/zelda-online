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
			Rectangle startingCollider = player.WalkingCollider();
			int immovableWidth = startingCollider.Width;
			int immovableHeight = startingCollider.Height;

			// Put an immovable below the player and check that the immovable stops the player.
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.X, startingCollider.Bottom, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			// Check that the player can still move in other directions.
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			// Same as above, but with the immovable *above* the player. Note that because of the
			// shape of the walking collider, the immovable will actually be placed inside the
			// player's sprite (but outside the walking collider).
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.X, startingCollider.Top - immovableHeight, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);

			// Same as above, but with the immovable to the right of the player.
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right, startingCollider.Y, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);

			// Same as above, but with the immovable to the left of the player.
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - immovableWidth, startingCollider.Y, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			// Now check that the player can move in the opposite direction immediately after
			// colliding with an immovable.
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.X, startingCollider.Bottom, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			MoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.X, startingCollider.Top - immovableHeight, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			MoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right, startingCollider.Y, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			MoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - immovableWidth, startingCollider.Y, immovableWidth, immovableHeight));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			MoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			// Check that the player can be stopped by a tiny immovable.
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Center.X, startingCollider.Bottom, 1, 1));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Center.X, startingCollider.Top - 1, 1, 1));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right, startingCollider.Center.Y, 1, 1));
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - 1, startingCollider.Center.Y, 1, 1));
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
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.X, startingCollider.Bottom + offset, immovableWidth, immovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, offset), Direction.Down);
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.X, startingCollider.Top - immovableHeight - offset, immovableWidth, immovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, -offset), Direction.Up);
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.Right + offset, startingCollider.Y, immovableWidth, immovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(offset, 0), Direction.Right);
				player = MakeWorldAndPlayer(new Rectangle(startingCollider.Left - immovableWidth - offset, startingCollider.Y, immovableWidth, immovableHeight));
				MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(-offset, 0), Direction.Left);
			}
		}

		[Fact]
		public void MovementWithMultipleImmovables()
		{
			Player player = MakeWorldAndPlayer();
			Rectangle startingCollider = player.WalkingCollider();
			int immovableWidth = startingCollider.Width;
			int immovableHeight = startingCollider.Height;

			// Put two immovables together so that the player will collide with both simultaneously
			// and check that they stop the player.
			var immovableA = new Rectangle(startingCollider.Center.X - immovableWidth, startingCollider.Bottom + 1, immovableWidth, immovableHeight);
			var immovableB = new Rectangle(startingCollider.Center.X, startingCollider.Bottom + 1, immovableWidth, immovableHeight);
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

			immovableA = new Rectangle(startingCollider.Center.X - immovableWidth, startingCollider.Top - immovableHeight - 1, immovableWidth, immovableHeight);
			immovableB = new Rectangle(startingCollider.Center.X, startingCollider.Top - immovableHeight - 1, immovableWidth, immovableHeight);
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

			immovableA = new Rectangle(startingCollider.Right + 1, startingCollider.Center.Y - immovableHeight, immovableWidth, immovableHeight);
			immovableB = new Rectangle(startingCollider.Right + 1, startingCollider.Center.Y, immovableWidth, immovableHeight);
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

			immovableA = new Rectangle(startingCollider.Left - immovableWidth - 1, startingCollider.Center.Y - immovableHeight, immovableWidth, immovableHeight);
			immovableB = new Rectangle(startingCollider.Left - immovableWidth - 1, startingCollider.Center.Y, immovableWidth, immovableHeight);
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

		[Fact]
		public void MovementWithSliding()
		{
			Player player = MakeWorldAndPlayer();
			Rectangle startingCollider = player.WalkingCollider();

			// RIGHT
			// Test sliding with 1 immovable.
			var immovableA = new Rectangle(startingCollider.Right, startingCollider.Top, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 1), Direction.Right);
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, -1), Direction.Right);
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Center.Y, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 0), Direction.Right);
			// Test sliding with 2 immovables stacked on top of each other. Note that the player
			// moves just enough pixels to clear the immovables.
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Top, 1, 1);
			var immovableB = new Rectangle(startingCollider.Right, startingCollider.Top + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 2), Direction.Right);
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Bottom - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Right, startingCollider.Bottom - 2, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, -2), Direction.Right);
			// Stagger the 2 immovables and test sliding.
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Right + 1, startingCollider.Top + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 2), Direction.Right);
			immovableA = new Rectangle(startingCollider.Right + 1, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Right, startingCollider.Top + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 2), Direction.Right);
			// Test that the player doesn't slide when 2 immovables hit different areas of the
			// collider.
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Right, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			// Test sliding into another immovable.
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Right, startingCollider.Top + 1, 1, 1);
			var immovableStop = new Rectangle(startingCollider.Right - 1, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 0), Direction.Right);
			immovableStop = new Rectangle(startingCollider.Right - 1, startingCollider.Bottom + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 1), Direction.Right);
			immovableA = new Rectangle(startingCollider.Right, startingCollider.Bottom - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Right, startingCollider.Bottom - 2, 1, 1);
			immovableStop = new Rectangle(startingCollider.Right - 1, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, 0), Direction.Right);
			immovableStop = new Rectangle(startingCollider.Right - 1, startingCollider.Top - 2, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Right), new Point(0, -1), Direction.Right);

			// Do the same tests above, but in the other three directions.

			// LEFT
			// Test 1 immovable
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Top, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 1), Direction.Left);
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, -1), Direction.Left);
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Center.Y, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 0), Direction.Left);
			// Test 2 immovables stacked
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Left - 1, startingCollider.Top + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 2), Direction.Left);
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Bottom - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Left - 1, startingCollider.Bottom - 2, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, -2), Direction.Left);
			// Test staggered immovables
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Left - 2, startingCollider.Top + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 2), Direction.Left);
			immovableA = new Rectangle(startingCollider.Left - 2, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Left - 1, startingCollider.Top + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 2), Direction.Left);
			// Test separated immovables
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Left - 1, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			// Test sliding into another immovable
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Top, 1, 1);
			immovableB = new Rectangle(startingCollider.Left - 1, startingCollider.Top + 1, 1, 1);
			immovableStop = new Rectangle(startingCollider.Left, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 0), Direction.Left);
			immovableStop = new Rectangle(startingCollider.Left, startingCollider.Bottom + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 1), Direction.Left);
			immovableA = new Rectangle(startingCollider.Left - 1, startingCollider.Bottom - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Left - 1, startingCollider.Bottom - 2, 1, 1);
			immovableStop = new Rectangle(startingCollider.Left, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, 0), Direction.Left);
			immovableStop = new Rectangle(startingCollider.Left, startingCollider.Top - 2, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Left), new Point(0, -1), Direction.Left);

			// DOWN
			// Test 1 immovable
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(1, 0), Direction.Down);
			immovableA = new Rectangle(startingCollider.Right - 1, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(-1, 0), Direction.Down);
			immovableA = new Rectangle(startingCollider.Center.X, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, 0), Direction.Down);
			// Test 2 immovables stacked
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Bottom, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(2, 0), Direction.Down);
			immovableA = new Rectangle(startingCollider.Right - 1, startingCollider.Bottom, 1, 1);
			immovableB = new Rectangle(startingCollider.Right - 2, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(-2, 0), Direction.Down);
			// Test staggered immovables
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Bottom, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Bottom + 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(2, 0), Direction.Down);
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Bottom + 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(2, 0), Direction.Down);
			// Test separated immovables
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Bottom, 1, 1);
			immovableB = new Rectangle(startingCollider.Right - 1, startingCollider.Bottom, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			// Test sliding into another immovable
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Bottom, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Bottom, 1, 1);
			immovableStop = new Rectangle(startingCollider.Right, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, 0), Direction.Down);
			immovableStop = new Rectangle(startingCollider.Right + 1, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(1, 0), Direction.Down);
			immovableA = new Rectangle(startingCollider.Right - 1, startingCollider.Bottom, 1, 1);
			immovableB = new Rectangle(startingCollider.Right - 2, startingCollider.Bottom, 1, 1);
			immovableStop = new Rectangle(startingCollider.Left - 1, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(0, 0), Direction.Down);
			immovableStop = new Rectangle(startingCollider.Left - 2, startingCollider.Bottom - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Down), new Point(-1, 0), Direction.Down);

			// UP
			// Test 1 immovable
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(1, 0), Direction.Up);
			immovableA = new Rectangle(startingCollider.Right - 1, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(-1, 0), Direction.Up);
			immovableA = new Rectangle(startingCollider.Center.X, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, 0), Direction.Up);
			// Test 2 immovables stacked
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Top - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(2, 0), Direction.Up);
			immovableA = new Rectangle(startingCollider.Right - 1, startingCollider.Top - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Right - 2, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(-2, 0), Direction.Up);
			// Test staggered immovables
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Top - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Top - 2, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(2, 0), Direction.Up);
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Top - 2, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(2, 0), Direction.Up);
			// Test separated immovables
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Top - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Right - 1, startingCollider.Top - 1, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB);
			HinderedMoveAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			// Test sliding into another immovable
			immovableA = new Rectangle(startingCollider.Left, startingCollider.Top - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Left + 1, startingCollider.Top - 1, 1, 1);
			immovableStop = new Rectangle(startingCollider.Right, startingCollider.Top, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, 0), Direction.Up);
			immovableStop = new Rectangle(startingCollider.Right + 1, startingCollider.Top, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(1, 0), Direction.Up);
			immovableA = new Rectangle(startingCollider.Right - 1, startingCollider.Top - 1, 1, 1);
			immovableB = new Rectangle(startingCollider.Right - 2, startingCollider.Top - 1, 1, 1);
			immovableStop = new Rectangle(startingCollider.Left - 1, startingCollider.Top, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(0, 0), Direction.Up);
			immovableStop = new Rectangle(startingCollider.Left - 2, startingCollider.Top, 1, 1);
			player = MakeWorldAndPlayer(immovableA, immovableB, immovableStop);
			MoveWithOffsetAndAssert(player, new KeyboardState(Keys.Up), new Point(-1, 0), Direction.Up);
		}

		// TODO: Test trying to walk while attacking.

		// TODO: Test animation?
	}
}
