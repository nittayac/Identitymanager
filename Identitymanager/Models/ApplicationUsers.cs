using Microsoft.AspNetCore.Identity;

namespace Identitymanager.Models;

public class ApplicationUsers :IdentityUser
{
    public string FullName { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
    public string PostCode { get; set; }
    public DateTime? BirthDate { get; set; }
    public string ProvinceId { get; set; }
    public string ImgUrl { get; set; }
}
