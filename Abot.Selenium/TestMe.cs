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
		readonly TorProcess _tp;
		
		public TestMe()
		{
			_tp = new TorProcess();
			Assert.IsNotNull(_tp.Start());
			Assert.IsTrue(_tp.InitWait());
			_w = new TorBrowserDriver(_tp);
			_w.Navigate().GoToUrl("http://condenast.avature.net/careers");
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

		~TestMe() { Dispose(); }
		int _disposing = 0;
		[OneTimeTearDown]
		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposing, 1, 0) == 0)
			{
				if (_w != null)
				{
					_w.Quit();
					_w.Dispose();
				}
				if (_tp != null)
					_tp.Dispose();
			}
		}
	}
}
