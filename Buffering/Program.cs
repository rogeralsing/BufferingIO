// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Buffering;
using StackExchange.Redis;

var mp = await ConnectionMultiplexer.ConnectAsync("localhost");
var db = mp.GetDatabase();

//seed the db
// for (int i = 0; i < 1_000_000; i++)
// {
//     if (i % 100 == 0) {Console.Write(".");}
//     await db.StringSetAsync("key" + i, Guid.NewGuid().ToByteArray());
// }
await RunRawReader(db);
await RunBufferedReader(db);

Console.ReadLine();

async Task RunBufferedReader(IDatabase database)
{
    var tasks = new List<Task>();
    var rb = new RedisRequestBuffer(database, 10, 5000, TimeSpan.FromMilliseconds(300));
    rb.Run();

    var sw = Stopwatch.StartNew();
    for (var i = 0; i < 1_000_000; i++)
    {
        var t = rb.Request("key" + i);
        tasks.Add(t);
    }

    await Task.WhenAll(tasks);
    sw.Stop();
    Console.WriteLine(sw.Elapsed);
}

async Task RunRawReader(IDatabase database)
{
    var tasks = new List<Task>();

    var sw = Stopwatch.StartNew();
    for (var i = 0; i < 1_000_000; i++)
    {
        var t = db.StringGetAsync("key" + i);
        tasks.Add(t);
    }

    await Task.WhenAll(tasks);
    sw.Stop();
    Console.WriteLine(sw.Elapsed);
}

//
//

// //individual request, but batched behind the scenes
// while (true)
// {
//     rb.Request(Guid.NewGuid().ToString("n"));
//     await Task.Delay(1);
// }
//
