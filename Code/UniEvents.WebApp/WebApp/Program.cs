using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace UniEvents.WebApp {
	public class Program {

		public static void Main(string[] args) {
         var key = "DerekZiemba";
         var pair = Core.Crypto.CreateAPIKey256(key);
         var f1 = Core.Crypto.VerifyHashMatch(key, pair.Item2, pair.Item1);
         var r1 = Core.Crypto.VerifyHashMatch(pair.Item2, key, pair.Item1);

         var key2 = "TestNumber2";
         var pair2 = Core.Crypto.CreateAPIKey256(key2);
         var f2 = Core.Crypto.VerifyHashMatch(key2, pair2.Item2, pair2.Item1);
         var r2 = Core.Crypto.VerifyHashMatch(pair2.Item2, key2, pair2.Item1);

         var key3 = "testuser2";
         var pair3 = Core.Crypto.CreateAPIKey256(key3);
         var f3 = Core.Crypto.VerifyHashMatch(key3, pair3.Item2, pair3.Item1);
         var r3 = Core.Crypto.VerifyHashMatch(pair3.Item2, key3, pair3.Item1);


         var pair4 = Core.Crypto.CreateAPIKey256("wtf is going on here");
         var r4 = Core.Crypto.VerifyHashMatch(pair4.Item2, "wtf is going on here", pair4.Item1);

         var pair5 = Core.Crypto.CreateAPIKey256("This Doesn't make any sense");
         var r5 = Core.Crypto.VerifyHashMatch(pair5.Item2, "This Doesn't make any sense", pair5.Item1);




         BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			 WebHost.CreateDefaultBuilder(args)
				  .UseStartup<Startup>()
				  .Build();
   }
}
