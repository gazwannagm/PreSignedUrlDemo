using StorageService.Domain.Entities;

namespace StorageService.Domain.Repositories;

public interface IUploadSessionRepository
{
    Task<UploadSession?> GetByIdAsync(string uploadId);
    Task AddAsync(UploadSession session);
    Task RemoveAsync(string uploadId);
}