using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Facility.Core;

namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// Provides conformance tests from a directory of JSON files.
	/// </summary>
	public sealed class ConformanceTestProvider : IConformanceTestProvider
	{
		/// <summary>
		/// Creates a test provider from the specified directory path.
		/// </summary>
		public ConformanceTestProvider(string testsFilePath)
		{
			var conformanceTests = new Dictionary<string, ConformanceTestInfo>();
			var testsInfo = ServiceJsonUtility.FromJson<ConformanceTestsInfo>(File.ReadAllText(testsFilePath));
			foreach (var testInfo in testsInfo.Tests)
				conformanceTests.Add(testInfo.Test, testInfo);
			m_conformanceTests = conformanceTests;
		}

		/// <summary>
		/// The test names.
		/// </summary>
		public IReadOnlyList<string> GetTestNames() => m_conformanceTests.Keys.ToList();

		/// <summary>
		/// Gets information for the test with the specified name.
		/// </summary>
		public ConformanceTestInfo TryGetTestInfo(string testName) => m_conformanceTests.TryGetValue(testName, out var info) ? info : null;

		private readonly IReadOnlyDictionary<string, ConformanceTestInfo> m_conformanceTests;
	}
}
