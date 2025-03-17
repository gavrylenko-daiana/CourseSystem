using System.Text.RegularExpressions;

namespace Core.Helpers;

public static class ValidationHelpers
{
    public static bool IsValidEmail(string email)
    {
        const string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        var regex = new Regex(pattern);
        
        return regex.IsMatch(email);
    }
}