using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lozo
{
	public class Tile
	{
		public Sprite Sprite { get; set; }
		public Rectangle Location { get; set; }

		public Tile(Sprite sprite, Rectangle location)
		{
			this.Sprite = sprite;
			this.Location = location;
		}

		// This constructor is for code that doesn't render the tile (like tests) or code that will
		// set the Sprite later.
		public Tile(Rectangle location)
		{
			this.Location = location;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			this.Sprite.Draw(spriteBatch, this.Location.X, this.Location.Y);
		}
	}
}
