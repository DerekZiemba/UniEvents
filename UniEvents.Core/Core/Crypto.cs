using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZMBA;

namespace UniEvents.Core {

	public static class Crypto {
		private static RNGCryptoServiceProvider _rngCryptoProvider = new RNGCryptoServiceProvider();
		private static SHA256CryptoServiceProvider _sha256CryptoProvider = new SHA256CryptoServiceProvider();

      public static string GenerateRandomString(int length) {
         byte[] bytes = new byte[length];
         _rngCryptoProvider.GetBytes(bytes, 0, length);
         string salt = Convert.ToBase64String(bytes, 0, length);
         if(salt.Length >= length) { salt = salt.Substring(0, length); }
         return salt;
      }

		public static (byte[], string) HashPassword256(string password, string salt = null) {
			if(salt.IsEmpty()) { salt = GenerateRandomString(20); }		
			return (_sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password + salt)), salt);
		}

      public static (byte[], string) CreateAPIKey256(string username) {
         string salt = GenerateRandomString(50);
         return (_sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(salt + username)), salt);
      }

      public static bool VerifyHashMatch(string password, string salt, byte[] hash) {
			return _sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password + salt)).SequenceEqual(hash);
		}






   }


}
