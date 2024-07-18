using System.Text.Json;
using System.Text.RegularExpressions;

partial class Solution
{
    public static async Task Main(string[] args)
    {
        string smallFileUrl =
            "https://gist.githubusercontent.com/bss/6dbc7d4d6d2860c7ecded3d21098076a/raw/244045d24337e342e35b85ec1924bca8425fce2e/sample.small.log";
        string largeFileUrl = "https://gist.githubusercontent.com/bss/1d7b8024451dd45feb5f17e148dacee5/raw/b02adc43edb43a44b6c9c9c34626243fd8171d4e/sample.log";

        try
        {
            var logEntries = new List<LogEntry>();

            using (HttpClient client = new HttpClient())
            {
                // Send a GET request to the URL
                HttpResponseMessage response = await client.GetAsync(largeFileUrl);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                using (Stream inputStream = await response.Content.ReadAsStreamAsync())
                {
                    using (StreamReader reader = new StreamReader(inputStream))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // path içerisinden endpoint bilgisi extract edilecek.

                            // endpointe ve metoda göre gruplanacak

                            // ilgili array listinde belli başlı endpointler alınacak

                            // gruplama yaparken küçükten büyüğe sıralama yapılacak

                            // gruplanan değerlerden mean değeri hesaplanacak

                            // gruplanan değerlerden median değeri hesaplanması

                            // gruplanan veri için kaç defa çağrıldığına dair count alınması

                            // response objesinin içeriğinin doldurulması  

                            // tüm endpointler için işlem tamamlandığında liste olarak dönülmesi

                            var logEntry = ParseLogLine(line);
                            
                            if (logEntry == null)
                            {
                                continue;
                            }

                            if (EndpointPatterns.Any(pattern => Regex.IsMatch(logEntry.Path, pattern)))
                            {
                                logEntries.Add(logEntry);
                            }
                        }
                    }
                }
                
                var groupedEntries = logEntries
                    .GroupBy(e => new { e.Method, NormalizedPath = NormalizePath(e.Path) })
                    .Select(g => new EndpointStats
                    {
                        RequestIdentifier = $"{g.Key.Method} {g.Key.NormalizedPath}",
                        Called = g.Count(),
                        ResponseTimeMean = g.Average(e => e.ConnectionTime + e.ProcessingTime),
                        ResponseTimeMedian = CalculateMedian(g.Select(e => e.ConnectionTime + e.ProcessingTime).ToList())
                    })
                    .OrderBy(es => es.RequestIdentifier)
                    .ToList();

                var jsonResult = JsonSerializer.Serialize(groupedEntries, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(jsonResult);
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request exception: {e.Message}");
        }
    }

    private static LogEntry? ParseLogLine(string logLine)
    {
        var regex = LogEntryRegex();
        var match = regex.Match(logLine);

        if (!match.Success)
        {
            return null; // If the log line doesn't match the expected format, return null
        }

        if (!int.TryParse(match.Groups["connect"].Value.TrimEnd('m', 's'), out int connectionTime))
        {
            return null;
        }

        if (!int.TryParse(match.Groups["service"].Value.TrimEnd('m', 's'), out int processingTime))
        {
            return null;
        }

        return new LogEntry
        {
            Method = match.Groups["method"].Value,
            Path = match.Groups["path"].Value,
            ConnectionTime = connectionTime,
            ProcessingTime = processingTime
        };
    }
    
    private static double CalculateMedian(List<int> values)
    {
        values.Sort();
        
        var count = values.Count;
        
        if (count % 2 == 0)
        {
            return (values[count / 2 - 1] + values[count / 2]) / 2.0;
        }
        
        return values[count / 2];
    }
    
    private static string NormalizePath(string path)
    {
        return NormalizePathRegex().Replace(path, "/{user_id}/");
    }

    private static readonly List<string> EndpointPatterns =
    [
        @"^/api/users/\d+/count_pending_messages$",
        @"^/api/users/\d+/get_messages$",
        @"^/api/users/\d+/get_friends_progress$",
        @"^/api/users/\d+/get_friends_score$",
        @"^/api/users/\d+$"
    ];
    
    [GeneratedRegex(
        @"^(?<timestamp>[^\s]+) heroku\[router\]: at=(?<loglevel>\w+) method=(?<method>\w+) path=(?<path>[^\s]+) host=(?<host>[^\s]+) fwd=""(?<fwd>[^\s]+)"" dyno=(?<dyno>[^\s]+) connect=(?<connect>[^\s]+) service=(?<service>[^\s]+) status=(?<status>\d+) bytes=(?<bytes>\d+)$")]
    private static partial Regex LogEntryRegex();
    
    [GeneratedRegex(@"/\d+")]
    private static partial Regex NormalizePathRegex();
}

class LogEntry
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int ConnectionTime { get; set; }
    public int ProcessingTime { get; set; }
}

public class EndpointStats
{
    public string RequestIdentifier { get; set; } = string.Empty;
    public int Called { get; set; }
    public double ResponseTimeMean { get; set; }
    public double ResponseTimeMedian { get; set; }
}