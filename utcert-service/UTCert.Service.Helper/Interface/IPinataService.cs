using Microsoft.AspNetCore.Http;

namespace UTCert.Service.Helper.Interface;

public interface IPinataService
{
    Task<string> Upload(string filePath);
}