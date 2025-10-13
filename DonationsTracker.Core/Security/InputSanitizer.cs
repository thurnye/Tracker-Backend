using System.Text.RegularExpressions;
using System.Web;

namespace DonationsTracker.Core.Security
{
    public static class InputSanitizer
    {
        // Removes HTML, scripts, encoded entities, and trims whitespace
        public static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Decode HTML entities
            input = HttpUtility.HtmlDecode(input);

            // Remove <script>, <iframe>, <object> tags
            input = Regex.Replace(input, "<(script|iframe|object)[^>]*?>.*?</\\1>", string.Empty, RegexOptions.IgnoreCase);

            // Remove all remaining HTML tags
            input = Regex.Replace(input, "<.*?>", string.Empty);

            // Remove any non-printable characters
            input = Regex.Replace(input, @"[^\u0020-\u007E]", string.Empty);

            return input.Trim();
        }
    }
}
