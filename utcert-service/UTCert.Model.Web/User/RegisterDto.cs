using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UTCert.Model.Web.User;

public class RegisterDto
{
    [Required]
    public string StakeId { get; set; } = null!;

    [Required]
    public string ReceiveAddress { get; set; } = null!; 

    [Required]
    public string Name { get; set; } = null!;

    public IFormFile? AvatarUri { get; set; }
}