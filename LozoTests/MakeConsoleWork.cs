using System;
using System.IO;
using Xunit.Abstractions;

namespace LozoTests
{
	// This class was copy+pasted from
	// https://github.com/Microsoft/vstest/issues/799#issuecomment-379458980
	public class MakeConsoleWork : IDisposable
	{
		private readonly ITestOutputHelper _output;
		private readonly TextWriter _originalOut;
		private readonly TextWriter _textWriter;

		public MakeConsoleWork(ITestOutputHelper output)
		{
			_output = output;
			_originalOut = Console.Out;
			_textWriter = new StringWriter();
			Console.SetOut(_textWriter);
		}

		public void Dispose()
		{
			_output.WriteLine(_textWriter.ToString());
			Console.SetOut(_originalOut);
		}
	}
}
