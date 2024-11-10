using Microsoft.AspNetCore.Http;
using UTCert.Model.Shared.Common;

namespace UTCert.Service.Helper.Interface;

public interface ICloudinaryService
{
    Task<string> Upload(string filePath, string folderName = Constants.CertificateFolderName);
    Task<string> UploadFromFile(IFormFile? file, string folderName = Constants.CertificateFolderName);
    Task<bool> Delete(string imageUrl);

}