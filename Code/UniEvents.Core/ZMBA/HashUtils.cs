using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZMBA;

namespace ZMBA {

   public static class HashUtils {
      public const int APIKeyLength256 = 50;
      public const int PasswordSaltLength256 = 20;

      private static RNGCryptoServiceProvider _rngCryptoProvider = new RNGCryptoServiceProvider();
      private static SHA256CryptoServiceProvider _sha256CryptoProvider = new SHA256CryptoServiceProvider();

      public static readonly Func<int, int> GetPrimeNumber = RuntimeCompiler.CompileStaticFunctionCaller<Func<int, int>>("System.Collections.HashHelpers", "GetPrime", new[]{typeof(int) });

      public static readonly Func<int, bool> IsPrimeNumber = RuntimeCompiler.CompileStaticFunctionCaller<Func<int, bool>>("System.Collections.HashHelpers", "IsPrime", new[]{typeof(int) });


      public static string GenerateRandomString(int length) {
         byte[] bytes = new byte[length];
         _rngCryptoProvider.GetBytes(bytes, 0, length);
         string salt = Convert.ToBase64String(bytes, 0, length);
         if(salt.Length >= length) { salt = salt.Substring(0, length); }
         return salt;
      }

      public static (byte[], string) HashPassword256(string password, string salt = null) {
         if(salt.IsNullOrEmpty()) { salt = GenerateRandomString(PasswordSaltLength256); }		
         return (_sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password + salt)), salt);
      }

      public static (byte[], string) CreateAPIKey256(string username) {
         string salt = GenerateRandomString(APIKeyLength256);
         return (_sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(salt + username)), salt);
      }

      public static bool VerifyHashMatch256(string password, string salt, byte[] hash) {
         return _sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password + salt)).SequenceEqual(hash);
      }






   }


}
