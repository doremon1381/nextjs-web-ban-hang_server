using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Infrastructure.Services;

public class BackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<IBackgroundJob> _channel = Channel.CreateUnbounded<IBackgroundJob>();

    public void Enqueue(IBackgroundJob job) => _channel.Writer.TryWrite(job);

    public ValueTask<IBackgroundJob> DequeueAsync(CancellationToken ct) =>
        _channel.Reader.ReadAsync(ct);
}

public class BackgroundJobWorker(
    IBackgroundJobQueue queue,
    IServiceProvider serviceProvider,
    ILogger<BackgroundJobWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                await job.ExecuteAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background job failed: {JobType}", job.GetType().Name);
            }
        }
    }

    private async IAsyncEnumerable<IBackgroundJob> DequeueAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            IBackgroundJob job;
            try { job = await queue.DequeueAsync(ct); }
            catch (OperationCanceledException) { yield break; }
            yield return job;
        }
    }
}
