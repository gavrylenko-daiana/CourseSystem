using System.Text.RegularExpressions;

namespace Core.Helpers;

public static class ValidationHelpers
{
    public static bool IsValidEmail(string email)
    {
        string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        Regex regex = new Regex(pattern);
        
        return regex.IsMatch(email);
    }
}