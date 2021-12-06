using System.Globalization;
using Facility.Core;

namespace Facility.Benchmarks;

public class UserRepository
{
	public UserRepository(ServiceSerializer serializer)
	{
		m_serializer = serializer;
	}

	public async Task<IReadOnlyList<UserDto>> GetUsersAsync()
	{
		return m_users.Select(x => ServiceDataUtility.Clone(x, m_serializer)).ToList();
	}

#pragma warning disable CA5394
	private static readonly Random Random = new Random(1337);

	private static List<UserDto> CreateUsers()
	{
		var users = new List<UserDto>();
		for (var i = 0; i < 1000; i++)
		{
			var user = new UserDto()
			{
				Id = i + 100_000,
				Name = GenerateName() + " " + GenerateName(),
				Email = GenerateWord() + "@example.com",
				Phone = GeneratePhoneNumber(),
				About = GenerateSentence(),
			};
			users.Add(user);
		}

		return users;
	}

	private static string GenerateString(char min, char max, int length) => new string(Enumerable.Range(0, length).Select(_ => (char) Random.Next(min, max + 1)).ToArray());

	private static string GenerateName() => GenerateString('a', 'z', 1).ToUpper(CultureInfo.InvariantCulture) + GenerateWord();

	private static string GenerateWord() => GenerateString('a', 'z', Random.Next(4, 13));

	private static string GenerateSentence() => string.Join(" ", Enumerable.Range(0, Random.Next(2, 7)).Select(_ => GenerateString('a', 'z', Random.Next(4, 13)))).ToUpper(CultureInfo.InvariantCulture) + ".";

	private static string GeneratePhoneNumber() => GenerateString('0', '9', 3) + "-" + GenerateString('0', '9', 3) + "-" + GenerateString('0', '9', 4);

	private readonly List<UserDto> m_users = CreateUsers();
	private readonly ServiceSerializer m_serializer;
}
