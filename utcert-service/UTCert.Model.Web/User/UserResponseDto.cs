using System.Text.Json.Serialization;
using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Web.User;

public class UserResponseDto
{
    public Guid Id { get; set; }

    public string? StakeId { get; set; }
    public string? Name { get; set; }
    public string? AvatarUri { get; set; }
    public Role Role { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string JwtToken { get; set; }

    [JsonIgnore] // refresh token is returned in http only cookie
    public string RefreshToken { get; set; }
}