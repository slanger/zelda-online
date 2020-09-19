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
			Rectangle oldLocation = player.Location;
			Direction oldDirection = player.Direction;
			player.Update(state);
			switch (expectedDirection)
			{
				case Direction.Left:
					Assert.Equal(Direction.Left, player.Direction);
					Assert.Equal(new Rectangle(new Point(oldLocation.X - Player.WalkSpeed, oldLocation.Y), oldLocation.Size), player.Location);
					break;
				case Direction.Right:
					Assert.Equal(Direction.Right, player.Direction);
					Assert.Equal(new Rectangle(new Point(oldLocation.X + Player.WalkSpeed, oldLocation.Y), oldLocation.Size), player.Location);
					break;
				case Direction.Up:
					Assert.Equal(Direction.Up, player.Direction);
					Assert.Equal(new Rectangle(new Point(oldLocation.X, oldLocation.Y - Player.WalkSpeed), oldLocation.Size), player.Location);
					break;
				case Direction.Down:
					Assert.Equal(Direction.Down, player.Direction);
					Assert.Equal(new Rectangle(new Point(oldLocation.X, oldLocation.Y + Player.WalkSpeed), oldLocation.Size), player.Location);
					break;
				case null:
					Assert.Equal(oldDirection, player.Direction);
					Assert.Equal(oldLocation, player.Location);
					break;
				default:
					throw new ArgumentException($"Invalid direction: {expectedDirection}");
			}
		}

		// This method assumes that the player won't move due to immovables in the way.
		private static void HinderedMoveAndAssert(Player player, KeyboardState state, Direction expectedDirection)
		{
			Rectangle oldLocation = player.Location;
			player.Update(state);
			Assert.Equal(expectedDirection, player.Direction);
			Assert.Equal(oldLocation, player.Location);
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
		}

		// TODO: Add test for walking into multiple immovables.

		// TODO: Test trying to walk while attacking.

		// TODO: Test animation?
	}
}
