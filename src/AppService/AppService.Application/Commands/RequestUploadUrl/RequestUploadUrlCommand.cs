using MediatR;

namespace AppService.Application.Commands.RequestUploadUrl;

public class RequestUploadUrlCommand : IRequest<RequestUploadUrlResult>
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

