using System.Text.RegularExpressions;

namespace FilmMaker.Helper.Validation
{
    public static class EmailValidation
    {
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase
            );
        }
    }
}
