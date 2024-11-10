using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UTCert.Model.Shared.Common;
using UTCert.Service.Helper.Interface;

namespace UTCert.Service.Helper;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    
    public CloudinaryService(IConfiguration configuration)
    {
        _cloudinary = new Cloudinary(new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]));
    }
    
    //TODO: resize https://cloudinary.com/documentation/image_optimization
    public async Task<string> Upload(string filePath, string folderName = Constants.CertificateFolderName)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return string.Empty;

            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var publicId = Guid.NewGuid().ToString();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(Path.GetFileName(filePath) + "_" + publicId, stream),
                Folder = folderName ?? Constants.CertificateFolderName,
                Transformation = new Transformation().Quality(80)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.StatusCode == HttpStatusCode.OK ? uploadResult.SecureUrl.AbsoluteUri : string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return string.Empty;
        }
    }

    public async Task<bool> Delete(string imageUrl)
    {
        try
        {
            var uri = new Uri(imageUrl);
            var pathSegments = uri.AbsolutePath.Split('/');
            var publicId = pathSegments[^1].Split('.').First(); 

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result == "ok")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<string> UploadFromFile(IFormFile? file, string folderName = Constants.CertificateFolderName)
    {
        try
        {
            if (file == null || file.Length == 0) return string.Empty;

            await using var stream = file.OpenReadStream();
            var publicId = Guid.NewGuid().ToString();
             var uploadParams = new ImageUploadParams()
            {
                 File = new FileDescription(file.FileName + "_" + publicId, stream),
                 Folder = folderName ?? Constants.CertificateFolderName,
                 Transformation = new Transformation().Quality(80)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.AbsoluteUri;
            }
            return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return String.Empty;
        }
    }


}