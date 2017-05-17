using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDomainManagerTest
{
	public class CustomAppDomainManager : AppDomainManager
	{
		public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
		{
			Console.WriteLine($"Initializing new AppDomain");
			base.InitializeNewDomain(appDomainInfo);
            appDomainInfo.AppDomainInitializer = new AppDomainInitializer(InitializeNewAppDomain);
        }

		public static void InitializeNewAppDomain(string[] args)
		{
            //Console.WriteLine("AppDomain id: " + AppDomain.CurrentDomain.Id);
            //Console.WriteLine(string.Join(" ", args));
        }
	}
}
