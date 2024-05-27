using System.ComponentModel.DataAnnotations;

namespace UTCert.Model.Web.User;

public class RegisterDto
{
    [Required]
    public string StakeId { get; set; }

    [Required]
    public string Name { get; set; }

    public string? AvatarUri { get; set; }
}