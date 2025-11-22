namespace StorageService.Application.Interfaces;

public interface ISignatureService
{
    string Sign(object data);
    bool Verify(object data, string signature);
}