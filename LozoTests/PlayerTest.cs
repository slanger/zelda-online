using Lozo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using System;
using Xunit;
using Xunit.Abstractions;

namespace LozoTests
{
	public class PlayerTest : MakeConsoleWork
	{
		public PlayerTest(ITestOutputHelper output) : base(output) { }

		private static Player MakeWorldAndPlayer()
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
			var collisionLayer = new TiledMapObjectLayer(Dungeon.CollisionLayerName, new TiledMapObject[0]);
			map.AddLayer(collisionLayer);
			var dungeon = new Dungeon(world, map, null);
			world.Dungeons.Add(dungeon);
			var player = new Player(dungeon, new Point(Room.Width / 2, Room.Height / 2), Direction.Down);
			world.Player = player;
			return player;
		}

		private static void UpdateAndAssert(Player player, KeyboardState state, Direction? expectedDirection)
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

		[Fact]
		public void MovementWithZeroButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();
			UpdateAndAssert(player, new KeyboardState(), null);
		}

		[Fact]
		public void MovementWithOneButtonPressed()
		{
			Player player = MakeWorldAndPlayer();
			UpdateAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			UpdateAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
		}

		[Fact]
		public void MovementWithTwoButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();

			// Buttons cancel each other out.
			UpdateAndAssert(player, new KeyboardState(Keys.Down, Keys.Up), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Right), null);

			// Zero buttons pressed, then two buttons pressed--horizontal keys take precedence.
			UpdateAndAssert(player, new KeyboardState(), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);
			UpdateAndAssert(player, new KeyboardState(), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			// One button pressed, then two buttons pressed.
			UpdateAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			UpdateAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			UpdateAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);
			UpdateAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			UpdateAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);
			UpdateAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			// One button pressed, then two buttons pressed again, but with more frames in between.
			UpdateAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Right), Direction.Right);

			UpdateAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Down), Direction.Down);

			UpdateAndAssert(player, new KeyboardState(Keys.Left), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Up), Direction.Up);

			// Two buttons pressed, then two other buttons pressed.
			UpdateAndAssert(player, new KeyboardState(), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			UpdateAndAssert(player, new KeyboardState(), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Down), Direction.Down);

			UpdateAndAssert(player, new KeyboardState(), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Up), Direction.Right);

			// Three buttons pressed, then two buttons pressed.
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);

			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Down), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Up);

			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Down), Direction.Left);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Down), Direction.Right);

			// Four buttons pressed, then two buttons pressed.
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right, Keys.Down), null);
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up), Direction.Left);
		}

		[Fact]
		public void MovementWithThreeButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right), Direction.Up);
			UpdateAndAssert(player, new KeyboardState(Keys.Up, Keys.Right, Keys.Down), Direction.Right);
			UpdateAndAssert(player, new KeyboardState(Keys.Right, Keys.Down, Keys.Left), Direction.Down);
			UpdateAndAssert(player, new KeyboardState(Keys.Down, Keys.Left, Keys.Up), Direction.Left);
		}

		[Fact]
		public void MovementWithFourButtonsPressed()
		{
			Player player = MakeWorldAndPlayer();
			// All four directions cancel each other out.
			UpdateAndAssert(player, new KeyboardState(Keys.Left, Keys.Up, Keys.Right, Keys.Down), null);
		}

		// TODO: Test collision.

		// TODO: Test trying to walk while attacking.

		// TODO: Test animation?
	}
}
