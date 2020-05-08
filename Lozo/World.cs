using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Collections.Generic;

namespace Lozo
{
	public class World
	{
		public List<Dungeon> Dungeons { get; set; } = new List<Dungeon>();

		public Player Player { get; set; }

		public OrthographicCamera Camera { get; set; }

		public void Update(KeyboardState state)
		{
			this.Player.Update(state);
			foreach (Dungeon d in this.Dungeons)
			{
				d.Update(state);
			}
			this.Camera.LookAt(this.Player.CurrentRoom.BoundingBox().Center.ToVector2());
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(
				transformMatrix: this.Camera.GetViewMatrix(),
				samplerState: SamplerState.PointClamp);
			this.Player.CurrentDungeon.DrawBottomLayer(this.Camera);
			spriteBatch.End();

			spriteBatch.Begin(
				transformMatrix: this.Camera.GetViewMatrix());
				//samplerState: SamplerState.PointClamp);
			this.Player.Draw(spriteBatch);
			spriteBatch.End();

			spriteBatch.Begin(
				transformMatrix: this.Camera.GetViewMatrix(),
				samplerState: SamplerState.PointClamp);
			this.Player.CurrentDungeon.DrawTopLayer(this.Camera);
			spriteBatch.End();
		}
	}
}
