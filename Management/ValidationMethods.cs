using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Management
{
    public class ValidationMethods
    {
        private Regex check;

        public bool IsDecimalValid(string value)
        {
            //Check if decimal is valid
            bool isValid;
            string decimalPattern = @"^\d*\.?\d*$";
            check = new Regex(decimalPattern);
            isValid = check.IsMatch(value);
            value = value.Replace(null, null);

            return isValid;
        }

        public bool OnlyLetters(String text)
        {
            bool isValid;
            //Check if only letters
            string lettersPattern = @"^[a-zA-Z\s]*$";
            check = new Regex(lettersPattern);
            isValid = check.IsMatch(text);

            return isValid;
        }

        public string IsNull(string value)
        {
            char[] temp = value.ToCharArray();
            //remove spaces
            value = value.Replace(null, null);
            //Start from beginnig and remove 0s till you find a value greater > 0
            if (value == null || value == null)
            {
                value = null;
            }
            else if (value.EndsWith(null))
            {
                //If the value ends with a dot, remove it
                value = value.Remove(value.Length - 1, 1);
            }
            else if (value.StartsWith(null) && value.Length > 1 && temp[1] > 0)
            {
                //Remove zeros
                value = value.Remove(0, 1);
            }
            else if (value.StartsWith(null))
            {
                //if the value starts with a . add a zero in the beginning
                value = value.Insert(0, null);
            }
            return value;
        }

        public bool LettersNumbersWhiteSpace(string value)
        {
            bool isValid;
            //Regex patter only allows letters, digits, underscores and whitespace
            string lettersPattern = null;
            check = new Regex(lettersPattern);
            isValid = check.IsMatch(value);

            return isValid;
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

            //Check if password meets requirements
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";
            check = new Regex(passwordPattern);
            bool isValid = check.IsMatch(password);
            return isValid;

        }

        public bool IsEmailValid(string email)
        {
            //Check if email is valid
            bool isValid;
            string emailPattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
            check = new Regex(emailPattern);
            isValid = check.IsMatch(email);
            return isValid;
        }

        public bool IsPhoneNumberValid(string phoneNumber)
        {
            //Check if phone number is valid
            bool isValid;
            string phoneNumberPattern = @"^(0\d{9})$";
            check = new Regex(phoneNumberPattern);
            isValid = check.IsMatch(phoneNumber);
            return isValid;
        }

        public bool IsEmailInOrganization(string email)
        {
            //Check if email is in organization
            bool isValid;
            //check if after the emailcontains the organization domain
            string emailPattern = @"@farmcentral\.com$";
            check = new Regex(emailPattern);
            isValid = check.IsMatch(email);
            return isValid;
        }
    }
}

