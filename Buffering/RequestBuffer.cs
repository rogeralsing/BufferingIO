using System.Threading.Channels;

namespace Buffering;

public abstract class RequestBuffer<TRequest,TResponse>
{
    private readonly int _batchSize;
    private readonly TimeSpan _batchTime;
    private readonly Channel<RequestPair> _channel = Channel.CreateUnbounded<RequestPair>();
    private SemaphoreSlim _semaphore;
    protected RequestBuffer(int maxConcurrency, int batchSize, TimeSpan? batchTime)
    {
        _batchSize = batchSize;
        _batchTime = batchTime ?? TimeSpan.FromMilliseconds(500);
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    protected record RequestPair(TRequest Request, TaskCompletionSource<TResponse> Response);
    public async Task<TResponse> Request(TRequest request)
    {
        var pair = new RequestPair(request, new TaskCompletionSource<TResponse>());
        await _channel.Writer.WriteAsync(pair);

        var res = await pair.Response.Task;
        return res;
    }

    public void Run()
    {
        _ = Task.Run(async () =>
        {
            await foreach(var batch in _channel.Reader.ReadAllAsync().Chunk(_batchTime,_batchSize))
            {
                //TODO: could probably do this in parallel, with max concurrency x
                await _semaphore.WaitAsync();
                await RunBatch(batch);
                _semaphore.Release();
            }
        });
    }

    protected abstract Task RunBatch(IList<RequestPair> batch);
    
}