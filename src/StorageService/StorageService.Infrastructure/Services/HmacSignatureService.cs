using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StorageService.Application.Interfaces;

namespace StorageService.Infrastructure.Services;

public class HmacSignatureService(IConfiguration configuration) : ISignatureService
{
    private readonly string _secret = configuration["SigningSecret"] 
                                      ?? Environment.GetEnvironmentVariable("SIGNING_SECRET") 
                                      ?? "shared-secret-key-between-services";

    public string Sign(object data)
    {
        var json = JsonSerializer.Serialize(data);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hash);
    }

    public bool Verify(object data, string signature)
    {
        var computedSignature = Sign(data);
        return computedSignature == signature;
    }
}
