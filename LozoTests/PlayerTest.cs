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
		public void MovementWithZeroButtonsPressed()
		{
			var player = new Player();
			Direction? direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
		}

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
			var player = new Player();

			// Buttons cancel each other out.
			Direction? direction = player.GetMovementDirection(new KeyboardState(Keys.Down, Keys.Up));
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Right));
			Assert.Null(direction);

			// Zero buttons pressed, then two buttons pressed--horizontal keys take precedence.
			direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Down));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right, Keys.Up));
			Assert.Equal(Direction.Right, direction);
			direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right, Keys.Down));
			Assert.Equal(Direction.Right, direction);

			// One button pressed, then two buttons pressed.
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right));
			Assert.Equal(Direction.Right, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right, Keys.Up));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right, Keys.Up));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right));
			Assert.Equal(Direction.Right, direction);

			direction = player.GetMovementDirection(new KeyboardState(Keys.Down));
			Assert.Equal(Direction.Down, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Down));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Down));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Down));
			Assert.Equal(Direction.Down, direction);

			direction = player.GetMovementDirection(new KeyboardState(Keys.Left));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Up));
			Assert.Equal(Direction.Up, direction);

			// Two buttons pressed, then two other buttons pressed.
			direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right, Keys.Down));
			Assert.Equal(Direction.Right, direction);

			direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Down));
			Assert.Equal(Direction.Down, direction);

			direction = player.GetMovementDirection(new KeyboardState());
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Left, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Right, Keys.Up));
			Assert.Equal(Direction.Right, direction);

			// Three buttons pressed, then two buttons pressed.
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up, Keys.Right));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Left, direction);

			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up, Keys.Right));
			Assert.Equal(Direction.Up, direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Down, Keys.Right));
			Assert.Equal(Direction.Right, direction);

			// Four buttons pressed, then two buttons pressed.
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up, Keys.Right, Keys.Down));
			Assert.Null(direction);
			direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up));
			Assert.Equal(Direction.Left, direction);
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
			var player = new Player();
			Direction? direction = player.GetMovementDirection(new KeyboardState(Keys.Left, Keys.Up, Keys.Right, Keys.Down));
			Assert.Null(direction);  // All four directions cancel each other out.
		}
	}
}
