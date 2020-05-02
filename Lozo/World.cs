using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Collections.Generic;

namespace Lozo
{
	public class World
	{
		public const int Scale = 3;

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
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (Dungeon d in this.Dungeons)
			{
				d.DrawBottomLayer(this.Camera);
			}

			spriteBatch.Begin(transformMatrix: this.Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.BackToFront);
			this.Player.Draw(spriteBatch);
			spriteBatch.End();

			foreach (Dungeon d in this.Dungeons)
			{
				d.DrawTopLayer(this.Camera);
			}
		}
	}
}
