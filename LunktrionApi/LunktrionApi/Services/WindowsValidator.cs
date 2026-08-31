using LunktrionApi.Services.Interfaces;

namespace LunktrionApi.Services
{
    public class WindowsValidator : ICommandValidator
    {
        private static readonly string[] ForbiddenWords = ["del", "rmdir", "shutdown", "powershell", "cmd", "dir", "cd"];
        private static readonly char[] ForbiddenChars = ['&', '|', '^'];

        public static bool IsSafe(string command)
        {
            string clean = command.ToLower();

            if (command.IndexOfAny(ForbiddenChars) != -1) 
                return false;

            foreach (var word in ForbiddenWords)
                if (clean.Contains(word)) return false;

            return true;
        }
    }
}
