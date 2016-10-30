using System;
using System.Collections.Generic;
using System.IO;
using Facility.CSharp;
using Facility.Definition;
using Facility.Definition.Console;
using Facility.Definition.Fsd;

namespace fsdgencsharp
{
	public sealed class FsdGenCSharpApp
	{
		public static int Main(string[] args)
		{
			try
			{
				var argsReader = new ArgsReader(args);
				if (argsReader.ReadHelpFlag())
				{
					foreach (string line in s_usageText)
						Console.WriteLine(line);
					return 0;
				}
				else
				{
					var app = new FsdGenCSharpApp(argsReader);
					argsReader.VerifyComplete();
					return app.Run();
				}
			}
			catch (ArgsReaderException exception)
			{
				Console.Error.WriteLine(exception.Message);
				Console.Error.WriteLine();
				foreach (string line in s_usageText)
					Console.Error.WriteLine(line);
				return 1;
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

		public FsdGenCSharpApp(ArgsReader args)
		{
			m_generator = new CSharpGenerator
			{
				GeneratorName = "fsdgencsharp",
				NamespaceName = args.ReadOption("namespace"),
				IndentText = args.ReadIndentOption(),
				NewLine = args.ReadNewLineOption(),
			};

			m_inputFilePath = args.ReadArgument();
			if (m_inputFilePath == null)
				throw new ArgsReaderException("Missing input file.");

			m_outputDirectoryPath = args.ReadArgument();
			if (m_outputDirectoryPath == null)
				throw new ArgsReaderException("Missing output directory.");
		}

		public int Run()
		{
			var input = m_inputFilePath == "-" ?
				new ServiceTextSource(Console.In.ReadToEnd()) :
				new ServiceTextSource(File.ReadAllText(m_inputFilePath)).WithName(Path.GetFileName(m_inputFilePath));

			var parser = new FsdParser();
			var service = parser.ParseDefinition(input);

			var outputs = m_generator.GenerateOutput(service);

			if (!Directory.Exists(m_outputDirectoryPath))
				Directory.CreateDirectory(m_outputDirectoryPath);

			foreach (var output in outputs)
			{
				string outputFilePath = Path.Combine(m_outputDirectoryPath, output.Name);

				string outputFileDirectoryPath = Path.GetDirectoryName(outputFilePath);
				if (outputFileDirectoryPath != null && outputFileDirectoryPath != m_outputDirectoryPath && !Directory.Exists(outputFileDirectoryPath))
					Directory.CreateDirectory(outputFileDirectoryPath);

				File.WriteAllText(outputFilePath, output.Text);
			}

			return 0;
		}

		static readonly IReadOnlyList<string> s_usageText = new[]
		{
			"Usage: fsdgencsharp input-file output-directory [options]",
			"",
			"   input-file",
			"      The input FSD file. (Standard input if \"-\".)",
			"   output-directory",
			"      The output directory.",
			"",
			"   --namespace <name>",
			"      The namespace used by the generated C#.",
			"   --indent (tab|1|2|3|4|5|6|7|8)",
			"      The indent used in the output: a tab or a number of spaces.",
			"   --newline (auto|lf|crlf)",
			"      The newline used in the output.",
		};

		readonly CSharpGenerator m_generator;
		readonly string m_inputFilePath;
		readonly string m_outputDirectoryPath;
	}
}
