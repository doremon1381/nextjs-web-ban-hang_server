namespace NhanViet.Application.Common.Interfaces;

public interface IBackgroundJob
{
    Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken ct);
}

public interface IBackgroundJobQueue
{
    void Enqueue(IBackgroundJob job);
    ValueTask<IBackgroundJob> DequeueAsync(CancellationToken ct);
}
