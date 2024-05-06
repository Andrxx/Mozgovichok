using System.Security.Cryptography;
using System.Text;

namespace mozgovichok.Infrastructure
{
    public static class HashHandler
    {
        const int keySize = 64;
        const int iterations = 100;
        static  HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
        private static byte[] salt = BitConverter.GetBytes(89182023198);        //тестовая соль, придумать защищенный вариант

        /// <summary>
        /// получаем хэш пароля, возврат пустой строки при null  в пароле
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static string HashPasword(string password)
        {
            //salt = RandomNumberGenerator.GetBytes(keySize);
            if (password == null) { return ""; }
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                hashAlgorithm,
                keySize);

            return Convert.ToHexString(hash);
        }
    }
}
