using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lozo
{
	public class Sprite
	{
		public Texture2D SpriteSheet { get; set; }
		public Rectangle Source { get; set; }

		public Sprite() { }

		public Sprite(Texture2D spritesheet, Rectangle source)
		{
			this.SpriteSheet = spritesheet;
			this.Source = source;
		}

		public void Draw(SpriteBatch spriteBatch, int x, int y, int originX = 0, int originY = 0, SpriteEffects effects = SpriteEffects.None)
		{
			spriteBatch.Draw(
				this.SpriteSheet,
				new Vector2(x, y),
				this.Source,
				Color.White,
				0f,
				new Vector2(originX, originY),
				World.Scale,
				effects,
				0f);
		}
	}
}
