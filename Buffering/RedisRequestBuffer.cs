using StackExchange.Redis;

namespace Buffering;

public class RedisRequestBuffer : RequestBuffer<string, byte[]>
{
    private readonly IDatabase _db;


    public RedisRequestBuffer(IDatabase db, int maxConcurrency = 10 , int batchSize=1000, TimeSpan? batchTime= null):base( maxConcurrency, batchSize, batchTime)
    {
        _db = db;
    }

    protected override async Task RunBatch(IList<RequestPair> batch)
    {
        var keys = batch.Select(b => new RedisKey(b.Request)).ToArray();
        //read from redis
        var data = await _db.StringGetAsync(keys);

        for (var i = 0; i < data.Length; i++)
        {
            var v = (byte[])data[i]!;
            //set results from redis
            batch[i].Response.SetResult(v);
        }
    }
}