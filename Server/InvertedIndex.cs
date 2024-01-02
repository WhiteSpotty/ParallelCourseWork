using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Server;

public class InvertedIndex
{
    private readonly ServerOptions _serverOptions;
    private ConcurrentDictionary<string, HashSet<string>> _concurrentDictionary = new();

    public InvertedIndex(IOptions<ServerOptions> serverOptions)
    {
        _serverOptions = serverOptions.Value;
    }
    
    public void Start()
    {
        var files = Directory.EnumerateFiles("Data").Select(file => file).ToList();
        var filesCount = files.Count;
        Console.WriteLine("Files count: {0}", filesCount);
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var rowsPerThread = filesCount / _serverOptions.ThreadsCount;
        var remainder = filesCount % _serverOptions.ThreadsCount;
        var tasks = new Task[_serverOptions.ThreadsCount];
        
        for (var i = 0; i < _serverOptions.ThreadsCount; i++)
        {
            var step = i;
            var start = i * rowsPerThread + Math.Min(step, remainder);
            var end = start + rowsPerThread + (step < remainder ? 1 : 0);
            var filesBlock = files.GetRange(start, end - start);
            tasks[i] = Task.Run(() => ProcessFilesBlock(filesBlock));
        }
        Task.WaitAll(tasks);
        
        stopwatch.Stop();
        Console.WriteLine($"Milliseconds: {stopwatch.ElapsedMilliseconds}"); 
    }

    public List<string> GetInvertedIndex(string word)
    {
        if (!_concurrentDictionary.TryGetValue(word, out var result))
        {
            return new List<string>();
        }

        return result.ToList();
    }

    private void ProcessFilesBlock(List<string> filesBlock)
    {
        foreach (var file in filesBlock)
        {
            var text = File.ReadAllText(file);
            var fileName = file.Replace("Data\\", "").Replace(".txt", "");
            var wordArray = text.Split(' ');
            
            foreach (var word in wordArray)
            {
                var processedWord = Regex.Replace(word, "[^0-9A-Za-z _-]", "");
                
                _concurrentDictionary.AddOrUpdate(
                    processedWord, 
                    new HashSet<string> { fileName }, 
                    (_, hashSet) => 
                    {
                        lock (hashSet)
                        {
                            hashSet.Add(fileName);
                            
                            return hashSet;
                        }
                    });
            }
        }
    }
}