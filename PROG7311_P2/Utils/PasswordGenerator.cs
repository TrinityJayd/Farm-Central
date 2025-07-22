namespace PROG7311_P2.Utils
{
    public class PasswordGenerator
    {
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>/?";

        public string GenerateTemporaryPassword()
        {
            const int requiredLength = 8;

            var random = new Random();
            var passwordChars = new char[requiredLength];

            // Add at least one character from each category
            passwordChars[0] = GetRandomChar(UppercaseLetters, random);
            passwordChars[1] = GetRandomChar(LowercaseLetters, random);
            passwordChars[2] = GetRandomChar(Digits, random);
            passwordChars[3] = GetRandomChar(SpecialCharacters, random);

            // Fill the remaining characters with random characters
            for (int i = 4; i < requiredLength; i++)
            {
                passwordChars[i] = GetRandomChar(GetAllCharacters(), random);
            }

            // Shuffle the characters randomly
            for (int i = 0; i < requiredLength - 1; i++)
            {
                int j = random.Next(i, requiredLength);
                (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
            }

            return new string(passwordChars);
        }

        private char GetRandomChar(string characterSet, Random random)
        {
            int index = random.Next(characterSet.Length);
            return characterSet[index];
        }

        private string GetAllCharacters()
        {
            return UppercaseLetters + LowercaseLetters + Digits + SpecialCharacters;
        }
    }
} 