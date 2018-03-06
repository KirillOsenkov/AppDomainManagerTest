# AppDomainManagerTest
A test app for bugs I've encountered in the AppDomainManager/AppDomainInitializer

From @marklio:

This appears to work fine if you don’t set AppDomainInitializer on the AppDomainSetup. I haven’t dug in to understand why. In any case, you don’t need this. InitializeNewDomain already happens in the new domain, so you can just do whatever you need there. If the manager looks like the following it works:
 
       public class CustomAppDomainManager : AppDomainManager
       {
              public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
              {
                     Console.WriteLine($"Initializing new AppDomain in {AppDomain.CurrentDomain.Id}");
           
                     base.InitializeNewDomain(appDomainInfo);
            //appDomainInfo.AppDomainInitializer = new AppDomainInitializer(InitializeNewAppDomain);
        }
 
              public static void InitializeNewAppDomain(string[] args)
              {
            //Console.WriteLine("AppDomain id: " + AppDomain.CurrentDomain.Id);
            //Console.WriteLine(string.Join(" ", args));
        }
       }
 
And produces:
 
Initializing new AppDomain in 1
Default domain: 1
Initializing new AppDomain in 2
Running in appdomain: 2

This seems like a bug, but unless we could attach security implications to it, or show it’s a regression, it is unlikely to meet the bar for a fix, and the fix is likely just a better diagnostic failure rather than making it work.

As near as I can tell the AppDomainManager stuff works fine (at least in the library case), but when the executable attempts to create another AppDomain, during SetupFusionStore, we end up doing some serialization I don’t yet understand which attempts to load ClassLibrary1, which fails because fusion setup isn’t complete.

I don’t think you can get the entrypoint assembly scenario to work without significant effort. The failure there is very interesting because we actually successfully instantiate the ADM, and run InitializeNewDomain, and then it’s attempt to load/run the entrypoint that fails. From a NetFX perspective, this would possibly meet the feature bar for vNext if we had a fix in hand, but the runtime team is likely to not want to invest resources there. We apparently don’t even know who owns AppDomains ;)
 
As to the failure with AppDomainInitializer, curiousity got the best of me. Looks like that delegate to InitializeNewAppDomain is the source of the odd serialization that Daniel observed. There are some assumptions that are violated when this not a delegate from another AppDomain.
 
In general, during InitializeNewDomain, the AppDomainSetup is informational (with the exception of some trust and grant set control stuff added in v4). If you want to affect the AppDomainSetup, override CreateDomain (think of this as hooking the user call to AppDomain.CreateDomain). This gets called in the creating appdomain (and doesn’t get called at all for the default AD) before the created AD comes to life. However, in general, you shouldn’t touch the initializer because user code may be using that to do their own initialization (which occurs after InitializeDomain). If you touch it, you should wrap and call any existing delegate.
 
So, if the ADM looks like:
 
       public class CustomAppDomainManager : AppDomainManager
       {
        public override AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
        {
            Console.WriteLine($"CreateDomain in {AppDomain.CurrentDomain.Id}");
            appDomainInfo.AppDomainInitializer = InitializeNewAppDomain;
            return base.CreateDomain(friendlyName, securityInfo, appDomainInfo);
        }
        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
              {
                     Console.WriteLine($"Initializing new AppDomain in {AppDomain.CurrentDomain.Id}");
        }
 
              public static void InitializeNewAppDomain(string[] args)
              {
            Console.WriteLine($"InitializeNewAppDomain implementation in {AppDomain.CurrentDomain.Id}");
        }
    }
 
 
Then this works and produces:
Initializing new AppDomain in 1
Default domain: 1
CreateDomain in 1
Initializing new AppDomain in 2
InitializeNewAppDomain implementation in 2
Running in appdomain: 2
 
Mark
