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
            Console.WriteLine($"Initializing new AppDomain (running in: {AppDomain.CurrentDomain.Id})");
            base.InitializeNewDomain(appDomainInfo);
        }
	}
}
