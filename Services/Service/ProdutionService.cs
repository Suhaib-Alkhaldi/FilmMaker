using FilmMaker.Common;
using FilmMaker.DTOs.ProductionCompany;
using FilmMaker.Entities;
using FilmMaker.Helper.Hashing;
using FilmMaker.Helper.Token;
using FilmMaker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services
{
    public class ProductionCompanyService : IProductionCompanyService
    {
        private readonly FilmMakerDbContext _db;
        private readonly IConfiguration _config; 

        private const string RoleName = "Production Company";

        public ProductionCompanyService(FilmMakerDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        
        public async Task<ApiResponse<RegisterProductionCompanyResponse>> RegisterAsync(
            RegisterProductionCompanyRequest request)
        {
            bool emailExists = await _db.Users
                .AnyAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (emailExists)
                return ApiResponse<RegisterProductionCompanyResponse>.FailureResponse(
                    "An account with this email already exists.",
                    "يوجد حساب مسجل بهذا البريد الإلكتروني مسبقاً.");

            bool phoneExists = await _db.Users
                .AnyAsync(u => u.PhoneNumber == request.PhoneNumber && !u.IsDeleted);

            if (phoneExists)
                return ApiResponse<RegisterProductionCompanyResponse>.FailureResponse(
                    "An account with this phone number already exists.",
                    "يوجد حساب مسجل بهذا الرقم مسبقاً.");

            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.Name == RoleName);

            if (role is null)
                return ApiResponse<RegisterProductionCompanyResponse>.FailureResponse(
                    "System configuration error: role not found.",
                    "خطأ في إعدادات النظام: الدور غير موجود.");

            List<ProductionCompanyProductionType> productionTypes = new();

            if (request.ProductionTypeIds is { Count: > 0 })
            {
                var (valid, types, errorEn, errorAr) = await ValidateProductionTypeIdsAsync(request.ProductionTypeIds, profileId: 0);
                if (!valid)
                    return ApiResponse<RegisterProductionCompanyResponse>.FailureResponse(errorEn!, errorAr!);

                productionTypes = types!;
            }

            var user = new User
            {
                Name        = request.CompanyName,
                Email       = request.Email,
                Password    = HashingHelper.HashValueWith384(request.Password),
                PhoneNumber = request.PhoneNumber,
                RoleId      = role.Id,
                CreatedBy   = request.Email,
                IsActive    = true,
                IsDeleted   = false
            };

            var profile = new ProductionCompanyProfile
            {
                Country         = request.Country.Trim(),
                City            = request.City.Trim(),
                Bio             = request.Bio?.Trim(),
                RegisterDate    = DateTime.UtcNow,
                ProductionTypes = productionTypes,
                CreatedBy       = request.Email,
                IsActive        = true,
                IsDeleted       = false
            };

            user.ProductionCompanyProfile = profile;

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return ApiResponse<RegisterProductionCompanyResponse>.SuccessResponse(
                new RegisterProductionCompanyResponse
                {
                    UserId      = user.Id,
                    CompanyName = user.Name,
                    Email       = user.Email
                },
                messageEn: "Registration successful.",
                messageAr: "تم التسجيل بنجاح.");
        }

       
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) &&
                string.IsNullOrWhiteSpace(request.PhoneNumber))
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Provide either an email address or phone number.",
                    "يرجى إدخال البريد الإلكتروني أو رقم الهاتف.");

            User? user = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user = await _db.Users
                    .Include(u => u.Roles)
                    .Include(u => u.ProductionCompanyProfile)
                    .FirstOrDefaultAsync(u =>
                        u.Email == request.Email &&
                        u.Roles.Name == RoleName &&
                        !u.IsDeleted);
            else
                user = await _db.Users
                    .Include(u => u.Roles)
                    .Include(u => u.ProductionCompanyProfile)
                    .FirstOrDefaultAsync(u =>
                        u.PhoneNumber == request.PhoneNumber &&
                        u.Roles.Name == RoleName &&
                        !u.IsDeleted);

            if (user is null || HashingHelper.HashValueWith384(request.Password) != user.Password)
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Invalid credentials.",
                    "بيانات الدخول غير صحيحة.");

            if (!user.IsActive)
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Your account has been deactivated. Please contact support.",
                    "تم تعطيل حسابك. يرجى التواصل مع الدعم.");

            var token = TokenHelper.GenerateJWTToken(user.Id, user.Name, user.Roles.Name, _config);

            return ApiResponse<LoginResponse>.SuccessResponse(
                new LoginResponse
                {
                    Token       = token,
                    CompanyName = user.Name,
                    Email       = user.Email,
                    TokenExpiry = DateTime.UtcNow.AddHours(2)
                },
                messageEn: "Login successful.",
                messageAr: "تم تسجيل الدخول بنجاح.");
        }

        
        public async Task<ApiResponse<ProductionCompanyProfileResponse>> GetProfileAsync(int userId)
        {
            var user = await _db.Users
                .Include(u => u.ProductionCompanyProfile)
                    .ThenInclude(p => p.ProductionTypes)
                        .ThenInclude(pt => pt.ProductionType)   
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    u.Roles.Name == RoleName &&
                    !u.IsDeleted);

            if (user?.ProductionCompanyProfile is null)
                return ApiResponse<ProductionCompanyProfileResponse>.FailureResponse(
                    "Production company profile not found.",
                    "لم يتم العثور على ملف الشركة.");

            var profile = user.ProductionCompanyProfile;

            return ApiResponse<ProductionCompanyProfileResponse>.SuccessResponse(
                new ProductionCompanyProfileResponse
                {
                    UserId          = user.Id,
                    CompanyName     = user.Name,
                    Email           = user.Email,
                    PhoneNumber     = user.PhoneNumber,
                    Country         = profile.Country,
                    City            = profile.City,
                    Bio             = profile.Bio,
                    RegisterDate    = profile.RegisterDate,
                    ProductionTypes = profile.ProductionTypes
                        .Where(pt => !pt.IsDeleted)
                        .Select(pt => new ProductionTypeDto
                        {
                            Id   = pt.ProductionType.Id,
                            Name = pt.ProductionType.Name
                        })
                        .ToList()
                });
        }

        
        public async Task<ApiResponse<UpdateProductionCompanyProfileResponse>> UpdateProfileAsync(
            int userId, UpdateProductionCompanyProfileRequest request)
        {
            var user = await _db.Users
                .Include(u => u.ProductionCompanyProfile)
                    .ThenInclude(p => p.ProductionTypes)
                        .ThenInclude(pt => pt.ProductionType)  
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    u.Roles.Name == RoleName &&
                    !u.IsDeleted);

            if (user?.ProductionCompanyProfile is null)
                return ApiResponse<UpdateProductionCompanyProfileResponse>.FailureResponse(
                    "Production company profile not found.",
                    "لم يتم العثور على ملف الشركة.");

            bool phoneTaken = await _db.Users
                .AnyAsync(u =>
                    u.PhoneNumber == request.PhoneNumber &&
                    u.Id != userId &&
                    !u.IsDeleted);

            if (phoneTaken)
                return ApiResponse<UpdateProductionCompanyProfileResponse>.FailureResponse(
                    "This phone number is already used by another account.",
                    "رقم الهاتف مستخدم من قبل حساب آخر.");

            List<ProductionCompanyProductionType> newTypes = new();

            if (request.ProductionTypeIds.Count > 0)
            {
                var (valid, types, errorEn, errorAr) = await ValidateProductionTypeIdsAsync(
                    request.ProductionTypeIds, user.ProductionCompanyProfile.Id);
                if (!valid)
                    return ApiResponse<UpdateProductionCompanyProfileResponse>.FailureResponse(errorEn!, errorAr!);

                newTypes = types!;
            }

            var profile = user.ProductionCompanyProfile;

            user.Name        = request.CompanyName.Trim();
            user.PhoneNumber = request.PhoneNumber;
            user.UpdatedBy   = user.Email;
            user.UpdatedAt   = DateTime.UtcNow;

            profile.Country   = request.Country.Trim();
            profile.City      = request.City.Trim();
            profile.Bio       = request.Bio?.Trim();
            profile.UpdatedBy = user.Email;
            profile.UpdatedAt = DateTime.UtcNow;

            profile.ProductionTypes.Clear();
            foreach (var t in newTypes)
                profile.ProductionTypes.Add(t);

            await _db.SaveChangesAsync();

            return ApiResponse<UpdateProductionCompanyProfileResponse>.SuccessResponse(
                new UpdateProductionCompanyProfileResponse
                {
                    CompanyName     = user.Name,
                    PhoneNumber     = user.PhoneNumber,
                    Country         = profile.Country,
                    City            = profile.City,
                    Bio             = profile.Bio,
                    ProductionTypes = profile.ProductionTypes
                        .Select(pt => new ProductionTypeDto
                        {
                            Id   = pt.ProductionType.Id,
                            Name = pt.ProductionType.Name
                        })
                        .ToList()
                },
                messageEn: "Profile updated successfully.",
                messageAr: "تم تحديث الملف بنجاح.");
        }

       
        public async Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(
            int userId, ChangePasswordRequest request)
        {
            var user = await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    u.Roles.Name == RoleName &&
                    !u.IsDeleted);

            if (user is null)
                return ApiResponse<ChangePasswordResponse>.FailureResponse(
                    "User not found.",
                    "المستخدم غير موجود.");

            if (HashingHelper.HashValueWith384(request.CurrentPassword) != user.Password)
                return ApiResponse<ChangePasswordResponse>.FailureResponse(
                    "Current password is incorrect.",
                    "كلمة المرور الحالية غير صحيحة.");

            if (HashingHelper.HashValueWith384(request.NewPassword) == user.Password)
                return ApiResponse<ChangePasswordResponse>.FailureResponse(
                    "New password must be different from your current password.",
                    "يجب أن تكون كلمة المرور الجديدة مختلفة عن الحالية.");

            user.Password  = HashingHelper.HashValueWith384(request.NewPassword);
            user.UpdatedBy = user.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var token = TokenHelper.GenerateJWTToken(user.Id, user.Name, user.Roles.Name, _config);

            return ApiResponse<ChangePasswordResponse>.SuccessResponse(
                new ChangePasswordResponse
                {
                    NewToken    = token,
                    TokenExpiry = DateTime.UtcNow.AddHours(2)
                },
                messageEn: "Password changed successfully.",
                messageAr: "تم تغيير كلمة المرور بنجاح.");
        }

       

        
        private async Task<(bool Valid,
                             List<ProductionCompanyProductionType>? Types,
                             string? ErrorEn,
                             string? ErrorAr)>
            ValidateProductionTypeIdsAsync(List<int> lookupItemIds, int profileId = 0)
        {
            var distinctIds = lookupItemIds.Distinct().ToList();

            var foundItems = await _db.LookupItems
                .Where(li => distinctIds.Contains(li.Id) && !li.IsDeleted)
                .Select(li => li.Id)
                .ToListAsync();

            if (foundItems.Count != distinctIds.Count)
            {
                var missing = string.Join(", ", distinctIds.Except(foundItems));
                return (false, null,
                    $"Invalid production type ID(s): {missing}",
                    $"معرفات نوع الإنتاج غير صالحة: {missing}");
            }

            // Build junction rows (ProfileId will be set by EF via the navigation
            // property when profileId == 0 on register; on update it is set explicitly)
            var junctionRows = distinctIds.Select(id => new ProductionCompanyProductionType
            {
                ProductionCompanyProfileId = profileId,
                ProductionTypeId           = id,
                IsActive                   = true,
                IsDeleted                  = false
            }).ToList();

            return (true, junctionRows, null, null);
        }

    }
}