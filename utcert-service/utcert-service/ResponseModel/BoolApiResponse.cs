using System.Net;
using Newtonsoft.Json;

namespace utcert_service.ResponseModel;

public class BoolApiResponse
{
    public BoolApiResponse()
    {
    }

    public BoolApiResponse(bool success, HttpStatusCode code = HttpStatusCode.OK)
    {
        Success = success;
        Code = code;
    }

    public BoolApiResponse(bool success, string message, HttpStatusCode code = HttpStatusCode.OK)
    {
        Success = success;
        Message = message;
        Code = code;
    }

    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets message code
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Message { get; set; }

    /// <summary>
    /// Error code
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public HttpStatusCode Code { get; set; }
}