using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Database;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string StakeId { get; set; }
    public string Name { get; set; }
    public string ReceiveAddress { get; set; }
    public string? AvatarUri { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    [NotMapped]
    public List<RefreshToken> RefreshTokens { get; set; }

    public bool OwnsToken(string token) 
    {
        return this.RefreshTokens?.Find(x => x.Token == token) != null;
    }
}