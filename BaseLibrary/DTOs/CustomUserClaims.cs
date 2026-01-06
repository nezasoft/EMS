
namespace BaseLibrary.DTOs
{
    public class CustomUserClaims
    {
        public record CustomUserClaims(string Id=null!, string Name=null!, string Email = null!, string Role = null! );

    }
}
