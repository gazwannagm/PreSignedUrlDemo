using StorageService.Domain.Entities;
using StorageService.Domain.Repositories;

namespace StorageService.Infrastructure.Repositories;

public class InMemoryUploadSessionRepository : IUploadSessionRepository
{
    private readonly Dictionary<string, UploadSession> _sessions = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<UploadSession?> GetByIdAsync(string uploadId)
    {
        await _lock.WaitAsync();
        try
        {
            return _sessions.GetValueOrDefault(uploadId);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(UploadSession session)
    {
        await _lock.WaitAsync();
        try
        {
            _sessions[session.UploadId] = session;
            await Task.CompletedTask;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RemoveAsync(string uploadId)
    {
        await _lock.WaitAsync();
        try
        {
            _sessions.Remove(uploadId);
            await Task.CompletedTask;
        }
        finally
        {
            _lock.Release();
        }
    }
}