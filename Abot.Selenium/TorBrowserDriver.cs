using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Abot.Selenium
{
	/// <summary>
	/// Firefox wrapper with tor proxy configured
	/// </summary>
	/// <remarks>Could possibly work with other browsers as well</remarks>
	public class TorBrowserDriver : FirefoxDriver
	{
		public TorBrowserDriver()
			: base(MakeProfile(ProxyConfig.Settings))
		{ }
		public TorBrowserDriver(TorProcess tor) 
			: base(MakeProfile(tor))
		{ }
		public TorBrowserDriver(SocksWebProxy prxy)
			: base(MakeProfile(prxy))
		{ }
		public TorBrowserDriver(ProxyConfig cfg)
			: base(MakeProfile(cfg))
		{ }

		static FirefoxProfile MakeProfile(TorProcess tor)
		{
			if (tor == null)
				throw new ArgumentNullException("tor");

			return MakeProfile(tor.ClientProxy);
		}
		static FirefoxProfile MakeProfile(SocksWebProxy prxy)
		{
			if (prxy == null)
				throw new ArgumentNullException("prxy");

			return MakeProfile(prxy.Config);
		}
		static FirefoxProfile MakeProfile(IProxyConfig cfg)
		{
			if (cfg == null)
				throw new ArgumentNullException("cfg");

			var ff = new FirefoxProfile();
			ff.SetPreference("network.proxy.type", 1);
			ff.SetPreference("network.proxy.socks", cfg.SocksAddress);
			ff.SetPreference("network.proxy.socks_port", cfg.SocksPort);
			return ff;
		}
	}
}
