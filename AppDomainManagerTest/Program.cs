using System;
using System.Diagnostics;
using System.Security.Policy;

namespace AppDomainManagerTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine($"Default domain: {AppDomain.CurrentDomain.Id}");
			var newDomain = AppDomain.CreateDomain("New Domain", new Evidence(AppDomain.CurrentDomain.Evidence), AppDomain.CurrentDomain.BaseDirectory, null, false);
			newDomain.DoCallBack(RunInAnotherDomain);
		}

		static void RunInAnotherDomain()
		{
			Console.WriteLine($"Running in appdomain: {AppDomain.CurrentDomain.Id}");
		}
	}

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
