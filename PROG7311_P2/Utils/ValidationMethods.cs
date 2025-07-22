using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PROG7311_P2.Utils
{
    public class ValidationMethods
    {
        private Regex check;

        public bool IsDecimalValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
                
            //Check if decimal is valid
            string decimalPattern = @"^\d*\.?\d*$";
            check = new Regex(decimalPattern);
            return check.IsMatch(value);
        }

        public bool OnlyLetters(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
                
            //Check if only letters
            string lettersPattern = @"^[a-zA-Z\s]*$";
            check = new Regex(lettersPattern);
            return check.IsMatch(text);
        }

        public string? IsNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
                
            // Remove leading/trailing whitespace
            value = value.Trim();
            
            // Handle decimal formatting
            if (value.EndsWith("."))
            {
                value = value.TrimEnd('.');
            }
            
            if (value.StartsWith("0") && value.Length > 1 && value[1] != '.')
            {
                value = value.TrimStart('0');
            }
            
            if (value.StartsWith("."))
            {
                value = "0" + value;
            }
            
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public bool LettersNumbersWhiteSpace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
                
            //Regex pattern only allows letters, digits, underscores and whitespace
            string lettersPattern = @"^[a-zA-Z0-9_\s]*$";
            check = new Regex(lettersPattern);
            return check.IsMatch(value);
        }


        public string HashPassword(string password)
        {
            SHA256 hash = SHA256.Create();
            var passwordBytes = Encoding.Default.GetBytes(password);
            var hashedPassword = hash.ComputeHash(passwordBytes);
            return Convert.ToHexString(hashedPassword);
        }

        public bool PasswordRequirements(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            //Check if password meets requirements - minimum 8 characters, max 128
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,128}$";
            check = new Regex(passwordPattern);
            return check.IsMatch(password);
        }

        public bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
                
            //Check if email is valid
            string emailPattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
            check = new Regex(emailPattern);
            return check.IsMatch(email);
        }

        public bool IsPhoneNumberValid(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;
                
            //Check if phone number is valid
            string phoneNumberPattern = @"^(0\d{9})$";
            check = new Regex(phoneNumberPattern);
            return check.IsMatch(phoneNumber);
        }

        public bool IsEmailInOrganization(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
                
            //Check if email is in organization
            //check if after the email contains the organization domain
            string emailPattern = @"@farmcentral\.com$";
            check = new Regex(emailPattern);
            return check.IsMatch(email);
        }
    }
} 