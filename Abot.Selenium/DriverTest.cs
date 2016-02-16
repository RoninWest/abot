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
	[TestFixture]
	public class DriverTest : IDisposable
	{
		readonly IWebDriver _w;
		readonly TorProcess _tp;
		
		public DriverTest()
		{
			_tp = new TorProcess();
			Assert.IsNotNull(_tp.Start());
			Assert.IsTrue(_tp.InitWait());
			_w = new TorBrowserDriver(_tp);
		}

		[Explicit]
		[TestCase("http://condenast.avature.net/careers", ".jobList .jobResultItem A")]
		public void ClickLink(string url, string selector)
		{
			Assert.That(!string.IsNullOrWhiteSpace(url));
			Assert.That(!string.IsNullOrWhiteSpace(selector));

			_w.Navigate().GoToUrl(url);
			IEnumerable<IWebElement> links = _w.FindElements(By.CssSelector(selector));
			CollectionAssert.IsNotEmpty(links);
			IWebElement el = links.FirstOrDefault();
			Assert.IsNotNull(el);
			if(el.Displayed && el.Enabled)
			{
				CollectionAssert.IsNotEmpty(el.Text);
				el.Click();
			}
		}

		~DriverTest() { Dispose(); }
		int _disposing = 0;
		[TestFixtureTearDown]
		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposing, 1, 0) == 0)
			{
				if (_w != null)
				{
					try {
						_w.Quit();
						_w.Dispose();
					}
					catch(Exception ex)
					{
						Console.Error.WriteLine(ex.Message);
					}
				}
				if (_tp != null)
					_tp.Dispose();
			}
		}
	}
}
