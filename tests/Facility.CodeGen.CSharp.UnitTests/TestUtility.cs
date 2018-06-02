using System;
using System.IO;
using NCrunch.Framework;
using NUnit.Framework.Internal;

namespace Facility.CodeGen.CSharp.UnitTests
{
	internal static class TestUtility
	{
		public static string GetSolutionDirectory()
		{
			const string solutionName = "FacilityCSharp.sln";

			string solutionFilePath = NCrunchEnvironment.GetOriginalSolutionPath();
			if (solutionFilePath != null && Path.GetFileName(solutionFilePath) == solutionName)
				return Path.GetDirectoryName(solutionFilePath);

			string solutionDirectoryPath = AssemblyHelper.GetDirectoryName(typeof(TestUtility).Assembly);
			while (solutionDirectoryPath != null)
			{
				if (File.Exists(Path.Combine(solutionDirectoryPath, solutionName)))
					return solutionDirectoryPath;
				solutionDirectoryPath = Path.GetDirectoryName(solutionDirectoryPath);
			}

			solutionDirectoryPath = Environment.CurrentDirectory;
			if (File.Exists(Path.Combine(solutionDirectoryPath, solutionName)))
				return solutionDirectoryPath;

			throw new InvalidOperationException("Failed to locate solution directory.");
		}
	}
}
