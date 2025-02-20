using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DawnAccountGen.Utils
{
    internal static class PasswordGenerator
    {
        private static readonly string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string Numbers = "0123456789";
        private static readonly string SpecialCharacters = "!@#$%^&*()-_=+<>?";

        public static string GeneratePassword(int length = 12)
        {
            if (length < 4)
                throw new ArgumentException("Password length must be at least 4 characters.");

            Random random = new Random();

            // Ensure at least one character from each category
            char upper = UpperCase[random.Next(UpperCase.Length)];
            char lower = LowerCase[random.Next(LowerCase.Length)];
            char number = Numbers[random.Next(Numbers.Length)];
            char special = SpecialCharacters[random.Next(SpecialCharacters.Length)];

            // Fill the remaining characters randomly
            string allChars = UpperCase + LowerCase + Numbers + SpecialCharacters;
            char[] password = new char[length];
            password[0] = upper;
            password[1] = lower;
            password[2] = number;
            password[3] = special;

            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // Shuffle the password to avoid predictable patterns
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }
    }
}
