// See https://aka.ms/new-console-template for more information

using Buffering;
using StackExchange.Redis;

var mp = await ConnectionMultiplexer.ConnectAsync("localhost");
var db = mp.GetDatabase();
var rb = new RedisRequestBuffer(db);
rb.Run();
//individual request, but batched behind the scenes
var res= await rb.Request("somekey");

Console.WriteLine(res);
