using Newtonsoft.Json;

namespace Client;

internal class Program
{
    static void Main()
    {
        _ = new Timer(async _ => await TimerTask(), null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(60));

        Console.ReadLine();
    }
    
    private static async Task TimerTask()
    {
        var httpClient = new HttpClient();
        var host = new Uri("http://localhost:5001/");
        var allWords = new[] {"television", "is", "TOXIC", "God"};
        var word = allWords[Random.Shared.Next(4)];

        var response = await httpClient.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(host, "api/server/get/" + word)
        });

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonConvert.DeserializeObject<List<string>>(content);

        if (result is null || !result.Any())
        {
            Console.WriteLine($"Not found {word} in files.");
            return;
        }

        Console.WriteLine($"Found {word} in {result.Count} files:");

        foreach (var fileName in result)
        {
            Console.WriteLine(fileName);
        }
    }
}