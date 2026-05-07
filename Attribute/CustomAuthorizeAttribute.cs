using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace FilmMaker.Attribute
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        public AuthorizeAdminAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "Admin";
        }
    }

    public class AuthorizeLocationOwnerAttribute : AuthorizeAttribute
    {
        public AuthorizeLocationOwnerAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "LocationOwner";
        }
    }

    public class AuthorizeLocationManagerAttribute : AuthorizeAttribute
    {
        public AuthorizeLocationManagerAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "LocationManager";
        }
    }
    public class AuthorizeProductionCompanyAttribute : AuthorizeAttribute
    {
        public AuthorizeProductionCompanyAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "ProductionCompany";
        }
    }
    public class AuthorizeServiceProviderAttribute : AuthorizeAttribute
    {
        public AuthorizeServiceProviderAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "ServiceProvider";
        }
    }
}
