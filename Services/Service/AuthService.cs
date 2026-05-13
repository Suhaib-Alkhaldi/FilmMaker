using FilmMaker.Common;
using FilmMaker.DTO.Auth.Request;
using FilmMaker.DTO.Auth.Response;
using FilmMaker.Entities;
using FilmMaker.Helper.Hashing;
using FilmMaker.Helper.Token;
using FilmMaker.Helper.Validation;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FilmMaker.Services.Service
{
    public class AuthService : IAuthService
    {
        private readonly FilmMakerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(FilmMakerDbContext context, IConfiguration configuration , ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<ApiResponse<RegisterResponseDto>> RegisterLocationOwner(RegisterLocationOwnerRequestDto request)
        {

            var validationError = ValidateBasicRegisterRequest(request);
            if (validationError != null)
            {
                _logger.LogWarning(
                    "Register location owner stopped due to validation error. Email: {Email}",
                    request?.Email
                );

                return validationError;
            }

            var emailError = await EnsureEmailIsUnique(request.Email);
            if (emailError != null)
            {
                _logger.LogWarning(
                    "Register location owner stopped because email is already used. Email: {Email}",
                    request.Email
                );

                return emailError;
            }

            var role = await GetRoleByName("Location Owner");
            if (role == null)
            {
                _logger.LogError(
                    "Register location owner failed because LocationOwner role was not found. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "LocationOwner role was not found.",
                    "دور صاحب الموقع غير موجود."
                );
            }


            try
            {
                var user = CreateUser(request, role.Id);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "User created successfully for location owner. UserId: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );

                var ownerProfile = new LocationOwnerProfile
                {
                    UserId = user.Id,
                    RegisterDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.LocationOwnerProfiles.Add(ownerProfile);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Location owner profile created successfully. UserId: {UserId}, ProfileId: {ProfileId}",
                    user.Id,
                    ownerProfile.Id
                );


                var response = CreateRegisterResponse(user, role.Name);

                _logger.LogInformation(
                    "Register location owner process completed successfully. UserId: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );

                return ApiResponse<RegisterResponseDto>.SuccessResponse(
                    response,
                    "Location owner account created successfully.",
                    "تم إنشاء حساب صاحب الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {


                _logger.LogError(
                    ex,
                    "Register location owner failed due to unexpected error. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "An unexpected error occurred while creating the location owner account.",
                    "حدث خطأ غير متوقع أثناء إنشاء حساب صاحب الموقع."
                );
            }
        }
        public async Task<ApiResponse<RegisterResponseDto>> RegisterLocationManager(RegisterLocationManagerRequestDto request)
        {
            var validationError = ValidateBasicRegisterRequest(request);
            if (validationError != null)
            {
                _logger.LogWarning(
                    "Register location manager stopped due to validation error. Email: {Email}",
                    request?.Email
                );

                return validationError;
            }

            var emailError = await EnsureEmailIsUnique(request.Email);
            if (emailError != null)
            {
                _logger.LogWarning(
                    "Register location manager stopped because email is already used. Email: {Email}",
                    request.Email
                );

                return emailError;
            }

            var role = await GetRoleByName("Location Manager");
            if (role == null)
            {
                _logger.LogError(
                    "Register location manager failed because LocationManager role was not found. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "LocationManager role was not found.",
                    "دور مدير الموقع غير موجود."
                );
            }


            try
            {
                var user = CreateUser(request, role.Id);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var managerProfile = new LocationManagerProfile
                {
                    UserId = user.Id,
                    Rate = 0,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.LocationManagerProfiles.Add(managerProfile);
                await _context.SaveChangesAsync();


                var response = CreateRegisterResponse(user, role.Name);

                _logger.LogInformation(
                    "Register location manager process completed successfully. UserId: {UserId}, Email: {Email}, ProfileId: {ProfileId}",
                    user.Id,
                    user.Email,
                    managerProfile.Id
                );

                return ApiResponse<RegisterResponseDto>.SuccessResponse(
                    response,
                    "Location manager account created successfully.",
                    "تم إنشاء حساب مدير الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Register location manager failed due to unexpected error. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "An unexpected error occurred while creating the location manager account.",
                    "حدث خطأ غير متوقع أثناء إنشاء حساب مدير الموقع."
                );
            }
        }
        public async Task<ApiResponse<LoginResponseDTO>> Login(LoginRequestDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("Login failed: request is null.");

                return ApiResponse<LoginResponseDTO>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Login failed: email is required.");
                return ApiResponse<LoginResponseDTO>.FailureResponse(
                    "Email is required.",
                    "البريد الإلكتروني مطلوب."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login failed: password is required. Identifier: {Identifier}",
                        request.Email);
                return ApiResponse<LoginResponseDTO>.FailureResponse(
                    "Password is required.",
                    "كلمة المرور مطلوبة."
                );
            }

            var identifier = request.Email.Trim().ToLower();

            var user = await _context.Users
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    (
                        x.Email.ToLower() == identifier ||
                        x.PhoneNumber == request.Email.Trim()
                    )
                );

            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found or inactive. Identifier: {Identifier}",
                        request.Email);
                return ApiResponse<LoginResponseDTO>.FailureResponse(
                    "Invalid email/phone or password.",
                    "البريد الإلكتروني/رقم الهاتف أو كلمة المرور غير صحيحة."
                );
            }

            var hashedPassword = HashingHelper.HashValueWith384(request.Password);

            if (user.Password != hashedPassword)
            {
                _logger.LogWarning(
                        "Login failed: invalid password. UserId: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email);

                return ApiResponse<LoginResponseDTO>.FailureResponse(
                    "Invalid email/phone or password.",
                    "البريد الإلكتروني/رقم الهاتف أو كلمة المرور غير صحيحة."
                );
            }

            var roleName = user.Roles.Name;

            var token = TokenHelper.GenerateJWTToken(
                            user.Id,
                            user.Name,
                            user.Roles.Name,
                            _configuration);


            var response = new LoginResponseDTO
            {
                Token = token,
                Expiration = DateTime.Now.AddHours(2)
            };

            return ApiResponse<LoginResponseDTO>.SuccessResponse(
                response,
                "Logged in successfully.",
                "تم تسجيل الدخول بنجاح."
            );
        }
        public async Task<ApiResponse<RegisterResponseDto>> RegisterProductionCompany(RegisterProductionCompanyRequestDto request)
        {
            var validationError = ValidateBasicRegisterRequest(request);
            if (validationError != null)
                return validationError;

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Company name is required.",
                    "اسم الشركة مطلوب."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Country))
            {
                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Country is required.",
                    "الدولة مطلوبة."
                );
            }

            if (string.IsNullOrWhiteSpace(request.City))
            {
                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "City is required.",
                    "المدينة مطلوبة."
                );
            }

            var emailError = await EnsureEmailIsUnique(request.Email);
            if (emailError != null)
                return emailError;


            var role = await GetRoleByName("Production Company");
            if (role == null)
            {
                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "ProductionCompany role was not found.",
                    "دور شركة الإنتاج غير موجود."
                );
            }


            try
            {
                var user = CreateUser(request, role.Id);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var profile = new ProductionCompanyProfile
                {
                    UserId = user.Id,
                    Country = request.Country.Trim(),
                    City = request.City.Trim(),
                    Bio = request.Bio,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.ProductionCompanyProfiles.Add(profile);
                await _context.SaveChangesAsync();

                if (request.ProductionTypeId != null && request.ProductionTypeId.Any())
                {
                    var productionTypes = request.ProductionTypeId
                        .Distinct()
                        .Select(typeId => new ProductionCompanyProductionType
                        {
                            ProductionCompanyProfileId = profile.Id,
                            ProductionTypeId = typeId,
                            IsActive = true,
                            IsDeleted = false
                        })
                        .ToList();

                    _context.ProductionCompanyProductionTypes.AddRange(productionTypes);
                    await _context.SaveChangesAsync();
                }


                var response = CreateRegisterResponse(user, role.Name);

                return ApiResponse<RegisterResponseDto>.SuccessResponse(
                    response,
                    "Production company account created successfully.",
                    "تم إنشاء حساب شركة الإنتاج بنجاح."
                );
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Register production company failed. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "An unexpected error occurred while creating the production company account.",
                    "حدث خطأ غير متوقع أثناء إنشاء حساب شركة الإنتاج."
                );
            }
        }
        public async Task<ApiResponse<RegisterResponseDto>> RegisterServiceProvider(RegisterServiceProviderRequestDto request)
        {
            var validationError = ValidateBasicRegisterRequest(request);
            if (validationError != null)
                return validationError;

            var emailError = await EnsureEmailIsUnique(request.Email);
            if (emailError != null)
                return emailError;

            var role = await GetRoleByName("Service Provider");
            if (role == null)
            {
                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "ServiceProvider role was not found.",
                    "دور مزود الخدمة غير موجود."
                );
            }

            try
            {
                var user = CreateUser(request, role.Id);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var profile = new ServiceProviderProfile
                {
                    UserId = user.Id,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.ServiceProviderProfiles.Add(profile);
                await _context.SaveChangesAsync();

                

                var cities = request.CitiesIds == null
                    ? new List<ServiceProviderCities>()
                    : request.CitiesIds
                        .Distinct()
                        .Select(cityId => new ServiceProviderCities
                        {
                            ServiceProviderId = profile.Id,
                            CityId = cityId,
                            IsActive = true,
                            IsDeleted = false
                        })
                        .ToList();

                var selectedServiceTypes = request.ServiceTypeIds == null
                    ? new List<ServiceProviderServiceType>()
                    : request.ServiceTypeIds
                        .Distinct()
                        .Select(serviceTypeId => new ServiceProviderServiceType
                        {
                            ServiceProviderId = profile.Id,
                            ServiceTypeId = serviceTypeId,
                            IsCustom = false,
                            IsActive = true,
                            IsDeleted = false
                        })
                        .ToList();

                var customServiceTypes = request.CustomServiceTypes == null
                    ? new List<ServiceProviderServiceType>()
                    : request.CustomServiceTypes
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .Distinct()
                        .Select(customName => new ServiceProviderServiceType
                        {
                            ServiceProviderId = profile.Id,
                            CustomServiceTypeName = customName,
                            IsCustom = true,
                            IsActive = true,
                            IsDeleted = false
                        })
                        .ToList();

                var allServiceTypes = selectedServiceTypes
                    .Concat(customServiceTypes)
                    .ToList();

                if (allServiceTypes.Any())
                {
                    _context.ServiceProviderServiceTypes.AddRange(allServiceTypes);
                    await _context.SaveChangesAsync();
                }

                if (cities.Any())
                {
                    _context.ServiceProviderCities.AddRange(cities);
                    await _context.SaveChangesAsync();
                }

                var response = CreateRegisterResponse(user, role.Name);

                return ApiResponse<RegisterResponseDto>.SuccessResponse(
                    response,
                    "Service provider account created successfully.",
                    "تم إنشاء حساب مزود الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Register service provider failed. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "An unexpected error occurred while creating the service provider account.",
                    "حدث خطأ غير متوقع أثناء إنشاء حساب مزود الخدمة."
                );
            }
        }


        private ApiResponse<RegisterResponseDto>? ValidateBasicRegisterRequest(BaseRegisterDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("Basic registration validation failed: request is null.");

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning(
                    "Basic registration validation failed: name is required. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Name is required.",
                    "الاسم مطلوب."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning(
                    "Basic registration validation failed: email is required. PhoneNumber: {PhoneNumber}",
                    request.PhoneNumber
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Email is required.",
                    "البريد الإلكتروني مطلوب."
                );
            }

            if (!EmailValidation.IsValidEmail(request.Email))
            {
                _logger.LogWarning(
                    "Basic registration validation failed: invalid email format. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Invalid email format.",
                    "صيغة البريد الإلكتروني غير صحيحة."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning(
                    "Basic registration validation failed: password is required. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Password is required.",
                    "كلمة المرور مطلوبة."
                );
            }

            if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                _logger.LogWarning(
                    "Basic registration validation failed: confirm password is required. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Confirm password is required.",
                    "تأكيد كلمة المرور مطلوب."
                );
            }

            if (request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning(
                    "Basic registration validation failed: password and confirm password do not match. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Password and confirm password do not match.",
                    "كلمة المرور وتأكيد كلمة المرور غير متطابقين."
                );
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                _logger.LogWarning(
                    "Basic registration validation failed: phone number is required. Email: {Email}",
                    request.Email
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Phone number is required.",
                    "رقم الهاتف مطلوب."
                );
            }

            if (!PhoneNumberValidation.IsValidPhoneNumber(request.PhoneNumber))
            {
                _logger.LogWarning(
                    "Basic registration validation failed: invalid phone number format. Email: {Email}, PhoneNumber: {PhoneNumber}",
                    request.Email,
                    request.PhoneNumber
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Invalid phone number format.",
                    "صيغة رقم الهاتف غير صحيحة."
                );
            }

            _logger.LogInformation(
                "Basic registration validation passed. Email: {Email}, PhoneNumber: {PhoneNumber}",
                request.Email,
                request.PhoneNumber
            );

            return null;
        }
        private async Task<ApiResponse<RegisterResponseDto>?> EnsureEmailIsUnique(string email)
        {
            var normalizedEmail = email.Trim().ToLower();

            _logger.LogInformation(
                "Checking email uniqueness. Email: {Email}",
                normalizedEmail
            );

            var emailExists = await _context.Users
                .AnyAsync(x => x.Email.ToLower() == normalizedEmail);

            if (emailExists)
            {
                _logger.LogWarning(
                    "Email uniqueness validation failed. Email already exists: {Email}",
                    normalizedEmail
                );

                return ApiResponse<RegisterResponseDto>.FailureResponse(
                    "Email is already used.",
                    "البريد الإلكتروني مستخدم مسبقًا."
                );
            }

            _logger.LogInformation(
                "Email uniqueness validation passed. Email: {Email}",
                normalizedEmail
            );

            return null;
        }
        private async Task<Role?> GetRoleByName(string roleName)
        {
            return await _context.Roles.Where(x => x.Name == roleName).FirstOrDefaultAsync();
        }
        private User CreateUser(BaseRegisterDto request, int roleId)
        {
            return new User
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLower(),
                Password = HashingHelper.HashValueWith384(request.Password),
                PhoneNumber = request.PhoneNumber.Trim(),
                IBAN = request.IBAN,
                RoleId = roleId,
                IsActive = true,
                IsDeleted = false
            };
        }
        private RegisterResponseDto CreateRegisterResponse(User user, string roleName)
        {
            return new RegisterResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = roleName
            };
        }

        
    }
}
