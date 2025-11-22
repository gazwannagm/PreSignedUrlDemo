namespace AppService.Application.Interfaces;

public interface ISignatureService
{
    string Sign(object data);
}