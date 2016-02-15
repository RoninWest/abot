using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Abot.Selenium
{
	[TestFixture("Selenium")]
	public class TestMe : IDisposable
	{
		readonly IWebDriver _w;

		public TestMe()
		{
			_w = new FirefoxDriver();
			_w.Navigate().GoToUrl("https://losangeles.craigslist.org/search/jjj");
		}

		[Test]
		public void ClickLink()
		{
			IEnumerable<IWebElement> rows = _w.FindElements(By.CssSelector(".content .row"));
			CollectionAssert.IsNotEmpty(rows);
		}

		[OneTimeTearDown]
		public void Dispose()
		{
			_w.Quit();
		}
	}
}
