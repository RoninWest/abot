using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Diagnostics;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using com.LandonKey.SocksWebProxy;

namespace Abot.Selenium
{
	[Category("Selenium.Tor")]
	[TestFixture(typeof(string), @"C:\Tor Browser\")]
	public class TestMe : IDisposable
	{
		readonly IWebDriver _w;
		readonly FileInfo _ffPath;
		readonly Process _tor;
		readonly bool _existingTor = false;

		public TestMe()
		{
			_ffPath = new FileInfo(@"C:\Tor Browser\Browser\firefox.exe");
			Assert.IsTrue(_ffPath.Exists);
			if(!(_existingTor = TorCheck(_ffPath)))
				_tor = TorStart(_ffPath);

			TorInitWait();

			var ff = new FirefoxProfile();
			ff.SetPreference("network.proxy.type", 1);
			ff.SetPreference("network.proxy.socks", "127.0.0.1");
			ff.SetPreference("network.proxy.socks_port", 9150);
			_w = new FirefoxDriver(ff);
			_w.Navigate().GoToUrl("http://condenast.avature.net/careers");
		}

		static readonly Regex TOR_OK = new Regex(@"<h1[^>]*>\s*congratulations", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex FF_TOR = new Regex(@"^\s*Firefox", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex BRW_TOR = new Regex(@"\WTor\s*Browser\W", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		bool TorCheck(FileInfo path)
		{
			Process[] processes = Process.GetProcesses();
			CollectionAssert.IsNotEmpty(processes);
			Process[] ffs = (from p in processes
							 where FF_TOR.IsMatch(p.ProcessName) 
								&& p.MainModule != null
								&& !string.IsNullOrWhiteSpace(p.MainModule.FileName)
								&& (string.Compare(p.MainModule.FileName, path.FullName, true) == 0 || BRW_TOR.IsMatch(p.MainModule.FileName))
							 select p).ToArray();
			return ffs != null && ffs.Count() > 0;
		}

		Process TorStart(FileInfo path)
		{
			var tor = new Process();
			tor.StartInfo.FileName = path.FullName;
			tor.StartInfo.Arguments = "-n";
			tor.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
			Assert.IsTrue(tor.Start());
			return tor;
		}

		void TorInitWait()
		{
			using (WebClient client = new WebClient())
			{
				client.Proxy = new SocksWebProxy();
				const string url = "https://check.torproject.org/";
				string html;
				int count = 0;
				do
				{
					if(count > 0)
						Thread.Sleep(TimeSpan.FromSeconds(5));

					html = client.DownloadString(url);
					count++;
				}
				while (!TOR_OK.IsMatch(html));
			}
		}

		[Test]
		public void ClickLink()
		{
			IEnumerable<IWebElement> links = _w.FindElements(By.CssSelector(".jobList .jobResultItem A"));
			CollectionAssert.IsNotEmpty(links);
			IWebElement el = links.FirstOrDefault();
			Assert.IsNotNull(el);
			if(el.Displayed && el.Enabled)
			{
				CollectionAssert.IsNotEmpty(el.Text);
				el.Click();
			}
		}

		[OneTimeTearDown]
		public void Dispose()
		{
			_w.Quit();
			_w.Dispose();
			if (!_existingTor && _tor != null)
				_tor.Dispose();
		}
	}
}
