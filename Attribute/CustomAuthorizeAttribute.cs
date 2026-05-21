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
            Roles = "Location Owner";
        }
    }

    public class AuthorizeLocationManagerAttribute : AuthorizeAttribute
    {
        public AuthorizeLocationManagerAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "Location Manager";
        }
    }
    public class AuthorizeProductionCompanyAttribute : AuthorizeAttribute
    {
        public AuthorizeProductionCompanyAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "Production Company";
        }
    }
    public class AuthorizeProductionCompanyOrLocationManagerAttribute : AuthorizeAttribute
    {
        public AuthorizeProductionCompanyOrLocationManagerAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "Production Company,Location Manager";
        }
    }
    public class AuthorizeServiceProviderAttribute : AuthorizeAttribute
    {
        public AuthorizeServiceProviderAttribute()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
            Roles = "Service Provider";
        }
    }
}
