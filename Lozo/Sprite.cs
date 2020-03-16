using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Lozo
{
	public enum SpriteID : int
	{
		Floor = 0,
		Rock,
		WalkRight1,
		WalkRight2,
		WalkUp1,
		WalkUp2,
		WalkDown1,
		WalkDown2,
		AttackRight1,
		AttackRight2,
		AttackRight3,
		AttackRight4,
		AttackUp1,
		AttackUp2,
		AttackUp3,
		AttackUp4,
		AttackDown1,
		AttackDown2,
		AttackDown3,
		AttackDown4,
	}

	public class Sprite
	{
		static readonly Dictionary<SpriteID, Tuple<string, Rectangle>> SpriteMap = new Dictionary<SpriteID, Tuple<string, Rectangle>>
		{
			[SpriteID.Floor] = new Tuple<string, Rectangle>("overworld", new Rectangle(1, 154, 16, 16)),
			[SpriteID.Rock] = new Tuple<string, Rectangle>("overworld", new Rectangle(1, 188, 16, 16)),
			[SpriteID.WalkRight1] = new Tuple<string, Rectangle>("link", new Rectangle(35, 11, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.WalkRight2] = new Tuple<string, Rectangle>("link", new Rectangle(52, 11, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.WalkUp1] = new Tuple<string, Rectangle>("link", new Rectangle(69, 11, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.WalkUp2] = new Tuple<string, Rectangle>("link", new Rectangle(86, 11, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.WalkDown1] = new Tuple<string, Rectangle>("link", new Rectangle(1, 11, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.WalkDown2] = new Tuple<string, Rectangle>("link", new Rectangle(18, 11, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.AttackRight1] = new Tuple<string, Rectangle>("link", new Rectangle(1, 77, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.AttackRight2] = new Tuple<string, Rectangle>("link", new Rectangle(18, 77, 27, Player.SpriteHeight)),
			[SpriteID.AttackRight3] = new Tuple<string, Rectangle>("link", new Rectangle(46, 77, 23, Player.SpriteHeight)),
			[SpriteID.AttackRight4] = new Tuple<string, Rectangle>("link", new Rectangle(70, 77, 19, Player.SpriteHeight)),
			[SpriteID.AttackUp1] = new Tuple<string, Rectangle>("link", new Rectangle(1, 109, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.AttackUp2] = new Tuple<string, Rectangle>("link", new Rectangle(18, 97, Player.SpriteWidth, 28)),
			[SpriteID.AttackUp3] = new Tuple<string, Rectangle>("link", new Rectangle(35, 98, Player.SpriteWidth, 27)),
			[SpriteID.AttackUp4] = new Tuple<string, Rectangle>("link", new Rectangle(52, 106, Player.SpriteWidth, 19)),
			[SpriteID.AttackDown1] = new Tuple<string, Rectangle>("link", new Rectangle(1, 47, Player.SpriteWidth, Player.SpriteHeight)),
			[SpriteID.AttackDown2] = new Tuple<string, Rectangle>("link", new Rectangle(18, 47, Player.SpriteWidth, 27)),
			[SpriteID.AttackDown3] = new Tuple<string, Rectangle>("link", new Rectangle(35, 47, Player.SpriteWidth, 23)),
			[SpriteID.AttackDown4] = new Tuple<string, Rectangle>("link", new Rectangle(52, 47, Player.SpriteWidth, 19)),
		};

		static Dictionary<string, Texture2D> SpriteSheetMap = new Dictionary<string, Texture2D>();

		public static void AddSpriteSheet(string key, Texture2D spritesheet)
		{
			SpriteSheetMap.Add(key, spritesheet);
		}

		SpriteID ID;

		public Rectangle Source
		{
			get
			{
				(string key, Rectangle source) = SpriteMap[this.ID];
				return source;
			}
		}

		public Sprite(SpriteID id)
		{
			this.ID = id;
		}

		public void Draw(SpriteBatch spriteBatch, int x, int y, int originX = 0, int originY = 0, SpriteEffects effects = SpriteEffects.None)
		{
			(string key, Rectangle source) = SpriteMap[this.ID];
			Texture2D spritesheet = SpriteSheetMap[key];
			spriteBatch.Draw(
				spritesheet,
				new Vector2(x, y),
				source,
				Color.White,
				0f,
				new Vector2(originX, originY),
				World.Scale,
				effects,
				0f);
		}
	}
}
