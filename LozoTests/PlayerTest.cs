using Lozo;
using Microsoft.Xna.Framework.Input;
using Xunit;
using Xunit.Abstractions;

namespace LozoTests
{
	public class PlayerTest : MakeConsoleWork
	{
		public PlayerTest(ITestOutputHelper output) : base(output) { }

		[Fact]
		public void MovementWithOneButtonPressed()
		{
			var player = new Player();
			Direction? direction = player.GetMovementDirection(new KeyboardState(Keys.Down));
			Assert.Equal(Direction.Down, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Up));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right));
			Assert.Equal(Direction.Right, direction);
		}

		[Fact]
		public void MovementWithTwoButtonsPressed()
		{
			// TODO
		}

		[Fact]
		public void MovementWithThreeButtonsPressed()
		{
			var player = new Player();
			Direction? direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up, Keys.Right));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Up, Keys.Right, Keys.Down));
			Assert.Equal(Direction.Right, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right, Keys.Down, Keys.Left));
			Assert.Equal(Direction.Down, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Down, Keys.Left, Keys.Up));
			Assert.Equal(Direction.Left, direction);
		}

		[Fact]
		public void MovementWithFourButtonsPressed()
		{
			// TODO
		}
	}
}
