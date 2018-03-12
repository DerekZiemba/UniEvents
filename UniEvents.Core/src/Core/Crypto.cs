using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Core {

	public static class Crypto {
		private static RNGCryptoServiceProvider _rngCryptoProvider = new RNGCryptoServiceProvider();
		private static SHA256CryptoServiceProvider _sha256CryptoProvider = new SHA256CryptoServiceProvider();

		public static (byte[], string) HashPassword256(string password, string salt = null) {
			if(salt == null) {
				byte[] bytes = new byte[20];
				_rngCryptoProvider.GetBytes(bytes, 0, 20);
				salt = Convert.ToBase64String(bytes, 0, 20);
				if (salt.Length >= 20) { salt = salt.Substring(0, 19); }
			}		
			return (_sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password + salt)), salt);
		}

		public static bool PasswordMatch(string password, string salt, byte[] hash) {
			return _sha256CryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password + salt)).SequenceEqual(hash);
		}

	}


}
