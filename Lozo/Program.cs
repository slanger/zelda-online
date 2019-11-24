using System;

namespace Lozo
{
	public static class Program
	{
		[STAThread]
		static void Main()
		{
			using (var game = new LozoGame())
				game.Run();
		}
	}
}
