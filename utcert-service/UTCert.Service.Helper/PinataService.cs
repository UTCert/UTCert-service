using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using UTCert.Service.Helper.Interface;

namespace UTCert.Service.Helper;

public class PinataService : IPinataService
{
    private readonly IConfiguration _configuration;
    
    public PinataService(IConfiguration configuration)
    {
        _configuration = configuration;    
    }
    
    public async Task<string> Upload(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return string.Empty;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetSection("PinataConfig")["Bearer"]);

            using var form = new MultipartFormDataContent();
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var fileContent = new StreamContent(fileStream)
            {
                Headers = { ContentType = new MediaTypeHeaderValue(GetMimeType(filePath)) }
            };
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var apiUrl = _configuration["PinataConfig:ApiUrl"];
            var response = await httpClient.PostAsync(apiUrl, form);
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);
        
            return jsonResponse["IpfsHash"]?.ToString() ?? string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return string.Empty;
        }
    }
    
    private string GetMimeType(string filePath)
    {
        var mimeType = "application/octet-stream";
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        if (extension != null)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out mimeType);
        }
        return mimeType ?? string.Empty;
    }
}