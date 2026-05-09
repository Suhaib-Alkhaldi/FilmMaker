using System.Text.RegularExpressions;

namespace FilmMaker.Helper.Validation
{
    public class PhoneNumberValidation
    {
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            var normalizedPhone = phoneNumber.Trim();

            return Regex.IsMatch(normalizedPhone, @"^\+?[0-9]{8,15}$");
        }
    }
}
