using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Lozo
{
	public enum SpriteID : int
	{
		WalkLeft1 = 0,
		WalkLeft2,
		WalkRight1,
		WalkRight2,
		WalkUp1,
		WalkUp2,
		WalkDown1,
		WalkDown2,
		AttackLeft1,
		AttackLeft2,
		AttackLeft3,
		AttackLeft4,
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
			[SpriteID.WalkLeft1] = new Tuple<string, Rectangle>("link", new Rectangle(309, 3, Player.Width, Player.Height)),
			[SpriteID.WalkLeft2] = new Tuple<string, Rectangle>("link", new Rectangle(360, 3, Player.Width, Player.Height)),
			[SpriteID.WalkRight1] = new Tuple<string, Rectangle>("link", new Rectangle(105, 3, Player.Width, Player.Height)),
			[SpriteID.WalkRight2] = new Tuple<string, Rectangle>("link", new Rectangle(156, 3, Player.Width, Player.Height)),
			[SpriteID.WalkUp1] = new Tuple<string, Rectangle>("link", new Rectangle(207, 3, Player.Width, Player.Height)),
			[SpriteID.WalkUp2] = new Tuple<string, Rectangle>("link", new Rectangle(258, 3, Player.Width, Player.Height)),
			[SpriteID.WalkDown1] = new Tuple<string, Rectangle>("link", new Rectangle(3, 3, Player.Width, Player.Height)),
			[SpriteID.WalkDown2] = new Tuple<string, Rectangle>("link", new Rectangle(54, 3, Player.Width, Player.Height)),
			[SpriteID.AttackLeft1] = new Tuple<string, Rectangle>("link", new Rectangle(3, 105, Player.Width, Player.Height)),
			[SpriteID.AttackLeft2] = new Tuple<string, Rectangle>("link", new Rectangle(54, 105, 81, Player.Height)),
			[SpriteID.AttackLeft3] = new Tuple<string, Rectangle>("link", new Rectangle(138, 105, 69, Player.Height)),
			[SpriteID.AttackLeft4] = new Tuple<string, Rectangle>("link", new Rectangle(210, 105, 57, Player.Height)),
			[SpriteID.AttackRight1] = new Tuple<string, Rectangle>("link", new Rectangle(3, 54, Player.Width, Player.Height)),
			[SpriteID.AttackRight2] = new Tuple<string, Rectangle>("link", new Rectangle(54, 54, 81, Player.Height)),
			[SpriteID.AttackRight3] = new Tuple<string, Rectangle>("link", new Rectangle(138, 54, 69, Player.Height)),
			[SpriteID.AttackRight4] = new Tuple<string, Rectangle>("link", new Rectangle(210, 54, 57, Player.Height)),
			[SpriteID.AttackUp1] = new Tuple<string, Rectangle>("link", new Rectangle(207, 192, Player.Width, Player.Height)),
			[SpriteID.AttackUp2] = new Tuple<string, Rectangle>("link", new Rectangle(258, 156, Player.Width, 84)),
			[SpriteID.AttackUp3] = new Tuple<string, Rectangle>("link", new Rectangle(309, 159, Player.Width, 81)),
			[SpriteID.AttackUp4] = new Tuple<string, Rectangle>("link", new Rectangle(360, 183, Player.Width, 57)),
			[SpriteID.AttackDown1] = new Tuple<string, Rectangle>("link", new Rectangle(3, 156, Player.Width, Player.Height)),
			[SpriteID.AttackDown2] = new Tuple<string, Rectangle>("link", new Rectangle(54, 156, Player.Width, 81)),
			[SpriteID.AttackDown3] = new Tuple<string, Rectangle>("link", new Rectangle(105, 156, Player.Width, 69)),
			[SpriteID.AttackDown4] = new Tuple<string, Rectangle>("link", new Rectangle(156, 156, Player.Width, 57)),
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
				1f,
				effects,
				0f);
		}
	}
}
