using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Facility.ServerTesting
{
	public sealed class ServerTester
	{
		public ServerTester(string basePath, HttpClient httpClient = null)
		{
			m_tests = new ServerTests(httpClient ?? s_httpClient, basePath?.TrimEnd('/') ?? "");
		}

		public static IReadOnlyList<string> GetTestNames()
		{
			return typeof(ServerTests).GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => typeof(Task).IsAssignableFrom(x.ReturnType))
				.Select(x => x.Name)
				.ToList();
		}

		public async Task<ServerTestRun> RunAllTests()
		{
			var results = new List<ServerTestResult>();

			foreach (var testName in GetTestNames())
				results.Add(await TryRunTestAsync(testName));

			return new ServerTestRun(results);
		}

		public async Task<ServerTestResult> TryRunTestAsync(string testName)
		{
			try
			{
				await RunTestAsync(testName);
				return new ServerTestResult(testName, ServerTestStatus.Pass);
			}
			catch (Exception exception)
			{
				return new ServerTestResult(testName, ServerTestStatus.Fail,
					(exception is TestFailedException ? "" : $"Unhandled exception {exception.GetType().FullName}: ") + exception.Message);
			}
		}

		public async Task RunTestAsync(string testName)
		{
			var method = typeof(ServerTests).GetMethod(testName, BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException();
			await (Task) method.Invoke(m_tests, null);
		}

		private static readonly HttpClient s_httpClient = new HttpClient();

		private readonly ServerTests m_tests;
	}
}
