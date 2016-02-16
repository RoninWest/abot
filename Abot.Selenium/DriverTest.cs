using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
		readonly IWebDriver _driver;
		readonly TorProcess _tp;
		
		public DriverTest()
		{
			_tp = new TorProcess();
			Assert.IsNotNull(_tp.Start());
			Assert.IsTrue(_tp.InitWait());
			_driver = new TorBrowserDriver(_tp);
			//_driver = new FirefoxDriver();
		}

		[Explicit]
		[TestCase("http://condenast.avature.net/careers", ".jobList .jobResultItem A")]
		[TestCase("http://testblank", "#iisstart", IgnoreReason = "local test only")]
		public void ClickLink(string url, string selector)
		{
			Assert.That(!string.IsNullOrWhiteSpace(url));
			Assert.That(!string.IsNullOrWhiteSpace(selector));

			_driver.Navigate().GoToUrl(url);
			IEnumerable<IWebElement> links = _driver.FindElements(By.CssSelector(selector));
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
				if (_driver != null)
				{
					try {
						_driver.Quit();
						_driver.Dispose();
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
