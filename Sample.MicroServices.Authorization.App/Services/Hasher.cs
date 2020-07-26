using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Sample.MicroServices.Authorization.App.Services
{
    public interface IHasher
    {
        string Hash(string plainText);

        bool Verify(string hashedValue, string plainText);
    }

    public class Hasher : IHasher
    {
        public string Hash(string plainText)
        {
            if (String.IsNullOrWhiteSpace(plainText))
            {
                throw new ArgumentException("the plainText is required.", $"{nameof(plainText)}");
            }

            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: plainText,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        public bool Verify(string hashedValue, string plainText)
        {
            if (String.IsNullOrWhiteSpace(hashedValue))
            {
                throw new ArgumentException("the hashedValue is required.", $"{nameof(hashedValue)}");
            }

            if (String.IsNullOrWhiteSpace(plainText))
            {
                throw new ArgumentException("the plainText is required.", $"{nameof(plainText)}");
            }

            var target = Hash(plainText);

            return hashedValue == target;
        }
    }
}
