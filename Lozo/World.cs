using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lozo
{
	public class World
	{
		public const int Scale = 3;
		public const int NumTilesWidth = 16;
		public const int NumTilesHeight = 11;
		public const int TileWidth = 16 * Scale;
		public const int TileHeight = 16 * Scale;
		public const int Width = NumTilesWidth * TileWidth;
		public const int Height = NumTilesHeight * TileHeight;

		static readonly Rectangle RockSource = new Rectangle(1, 188, 16, 16);
		static readonly Rectangle[] Rocks = new[] {
			new Rectangle(TileWidth * 3, TileHeight * 3, TileWidth, TileHeight),
			new Rectangle(TileWidth * 12, TileHeight * 3, TileWidth, TileHeight),
			new Rectangle(TileWidth * 3, TileHeight * 7, TileWidth, TileHeight),
			new Rectangle(TileWidth * 12, TileHeight * 7, TileWidth, TileHeight),
		};

		Player player;
		Sprite rockSprite;

		public World() { }

		public void AddPlayer(Player player)
		{
			this.player = player;
		}

		public void AddSpriteSheet(Texture2D spritesheet)
		{
			this.rockSprite = new Sprite(spritesheet, RockSource);
		}

		public void Update(KeyboardState state)
		{
			this.player.Update(state);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (Rectangle rock in Rocks)
			{
				this.rockSprite.Draw(spriteBatch, rock.X, rock.Y);
			}
			// Draw the player last so that the player is drawn on top of everything else in the
			// world.
			this.player.Draw(spriteBatch);
		}

		public List<Rectangle> CollidingWith(Rectangle collider)
		{
			var colliding = new List<Rectangle>();
			foreach (Rectangle rock in Rocks)
			{
				if (collider.Intersects(rock))
				{
					colliding.Add(rock);
				}
			}
			return colliding;
		}
	}
}
