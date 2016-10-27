using System;
using System.Collections.Generic;
using System.IO;
using Facility.CSharp;
using Facility.Definition;
using Facility.Definition.Fsd;

namespace fsdgencsharp
{
	public sealed class FsdGenCSharpApp
	{
		public static int Main(string[] args)
		{
			return new FsdGenCSharpApp().Run(args);
		}

		public int Run(IReadOnlyList<string> args)
		{
			try
			{
				return RunCore(args);
			}
			catch (Exception exception) when (exception is ApplicationException || exception is ServiceDefinitionException)
			{
				Console.Error.WriteLine(exception.Message);
				return 1;
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine(exception.ToString());
				return 2;
			}
		}

		private int RunCore(IReadOnlyList<string> args)
		{
			string inputPath = null;
			string outputPath = null;
			var generator = new CSharpGenerator { GeneratorName = "fsdgencsharp" };

			using (var enumerator = args.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string arg = enumerator.Current;
					if (arg[0] == '-')
					{
						if (arg == "--namespace")
						{
							if (!enumerator.MoveNext())
								throw new ApplicationException("Argument missing: " + arg);
							generator.NamespaceName = enumerator.Current;
						}
						else
						{
							throw new ApplicationException("Unknown option: " + arg);
						}
					}
					else if (inputPath == null)
					{
						inputPath = arg;
					}
					else if (outputPath == null)
					{
						outputPath = arg;
					}
					else
					{
						throw new ArgumentException("Unused argument: " + arg);
					}
				}
			}

			if (outputPath == null)
				throw new ApplicationException("Usage: fsdgencsharp [options] input-file output-directory");

			var input = inputPath == "-" ?
				new ServiceTextSource("", Console.In.ReadToEnd()) :
				new ServiceTextSource(Path.GetFileName(inputPath), File.ReadAllText(inputPath));

			var parser = new FsdParser();
			var definition = parser.ParseDefinition(input);

			var outputs = generator.GenerateOutput(definition);

			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);

			foreach (var output in outputs)
			{
				string outputFile = Path.Combine(outputPath, output.Name);

				string outputFileDirectory = Path.GetDirectoryName(outputFile);
				if (outputFileDirectory != null && outputFileDirectory != outputPath && !Directory.Exists(outputFileDirectory))
					Directory.CreateDirectory(outputFileDirectory);

				File.WriteAllText(outputFile, output.Text);
			}

			return 0;
		}
	}
}
