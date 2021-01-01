using System;

namespace TestContainers.Containers
{
    internal class Base58
    {
        private static readonly char[] ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"
            .ToCharArray();

        private static readonly Random RANDOM = new Random();

        public static string RandomString(int length)
        {
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                char pick = ALPHABET[RANDOM.Next(ALPHABET.Length)];
                result[i] = pick;
            }

            return new string(result);
        }
    }
}
