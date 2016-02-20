using System;
using System.Linq;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Abot.Selenium
{
	public static class SeleniumExtensions
	{
		public static IWebElement FirstVisible(this IWebDriver driver, By by)
		{
			if (driver == null)
				throw new ArgumentNullException("driver");

			return FirstVisible(driver.FindElements(by));
		}

		public static IWebElement FirstVisible(this IEnumerable<IWebElement> elements)
		{
			if (elements == null || !elements.Any())
				return null;

			return elements.FirstOrDefault(o => o.Displayed && o.Enabled);
		}

		static readonly Regex UNIT = new Regex(@"(-?\d+(?:[.,]\d+|))(px|pt|%|em)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static Measurement ToMeasurement(this string measure, Measurement defaultValue)
		{
			try
			{
				return ToMeasurement(measure);
			}
			catch (NotSupportedException)
			{
				if (defaultValue == null)
					throw;

				return defaultValue;
			}
		}

		public static Measurement ToMeasurement(this string measure)
		{
			if (!string.IsNullOrWhiteSpace(measure))
			{
				Match m = UNIT.Match(measure);
				if (m.Success && m.Groups.Count > 2)
				{
					var r = new Measurement();
					if (double.TryParse(m.Groups[1].Value, out r.Measure))
					{
						if (!Enum.TryParse(m.Groups[2].Value, true, out r.Unit) && m.Groups[2].Value == "%")
							r.Unit = MeasurementUnit.Percent;

						return r;
					}
				}
			}
			throw new NotSupportedException(measure);
		}

		public static SelectElement AsSelectBox(this IWebElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			if (element is SelectElement)
				return element as SelectElement;
			else
				return new SelectElement(element);
		}

		public static bool IsAccessible(this IWebElement element)
		{
			if (element == null)
				return false;
			try
			{
				return element.Displayed && element.Enabled;
			}
			catch (NotFoundException)
			{
				return false;
			}
		}

		public static bool WaitForCondition(this IWebDriver driver, int waitMS, Func<IWebDriver, bool> condition)
		{
			return WaitForCondition(driver, TimeSpan.FromMilliseconds(waitMS), condition);
		}

		public static bool WaitForCondition(this IWebDriver driver, TimeSpan wait, Func<IWebDriver, bool> condition)
		{
			if (wait < TimeSpan.Zero || wait > MAXWAIT)
				wait = MAXWAIT;

			TimeSpan waitTotal = TimeSpan.Zero;
			do
			{
				if (condition(driver))
					return true;

				TimeSpan actualWait = TimeSpan.FromMilliseconds(Math.Min(MINWAIT.TotalMilliseconds, wait.TotalMilliseconds));
				Thread.Sleep(actualWait);
				waitTotal += actualWait;
			} while (waitTotal < wait);
			return false;
		}

		static readonly TimeSpan MINWAIT = TimeSpan.FromMilliseconds(100);
		static readonly TimeSpan MAXWAIT = TimeSpan.FromHours(1);

		public static bool WaitForElementPresent(this IWebDriver driver, By by, int waitMS)
		{
			return WaitForElementPresent(driver, by, TimeSpan.FromMilliseconds(waitMS));
		}

		public static bool WaitForElementPresent(this IWebDriver driver, By by, TimeSpan wait)
		{
			if (wait < TimeSpan.Zero || wait > MAXWAIT)
				wait = MAXWAIT;

			TimeSpan waitTotal = TimeSpan.Zero;
			do
			{
				if (IsElementPresent(driver, by))
					return true;

				TimeSpan actualWait = TimeSpan.FromMilliseconds(Math.Min(MINWAIT.TotalMilliseconds, wait.TotalMilliseconds));
				Thread.Sleep(actualWait);
				waitTotal += actualWait;
			} while (waitTotal < wait);
			return false;
		}

		public static bool IsElementPresent(this IWebDriver driver, By by)
		{
			if (driver == null)
				throw new ArgumentNullException("driver");
			try
			{
				driver.FindElement(by);
				return true;
			}
			catch (NoSuchElementException)
			{
				return false;
			}
		}

		public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutMS)
		{
			return FindElement(driver, by, TimeSpan.FromMilliseconds(timeoutMS));
		}

		public static IWebElement FindElement(this IWebDriver driver, By by, TimeSpan timeout)
		{
			if (driver == null)
				throw new ArgumentNullException("driver");
			if (timeout > TimeSpan.Zero)
			{
				var wait = new WebDriverWait(driver, timeout);
				return wait.Until(drv => drv.FindElement(by));
			}
			return driver.FindElement(by);
		}

		public static bool EnsureUrl(this IWebDriver driver, string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return false;

			return EnsureUrl(driver, new Uri(url));
		}

		public static bool EnsureUrl(this IWebDriver driver, Uri url)
		{
			bool ok;
			if (ok = (url != null && string.Compare(driver.Url, url.ToString(), true) != 0))
				driver.Navigate().GoToUrl(url);

			return ok;
		}

		static readonly Regex PRICE_RE = new Regex(@"^\W?\s*(\d+(?:[\.,]\d{2}|))", RegexOptions.Compiled);

		public static decimal ParsePrice(this string value, decimal defaultValue = 0m)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				Match m = PRICE_RE.Match(value);
				if (m.Success && m.Groups.Count > 1)
				{
					decimal price;
					if (decimal.TryParse(m.Groups[1].Value, out price))
						return price;
				}
			}
			return defaultValue;
		}
	}

	public class Measurement
	{
		public Measurement() { }
		public Measurement(double measure) : this(measure, MeasurementUnit.Unknown) { }
		public Measurement(double measure, MeasurementUnit unit)
		{
			Measure = measure;
			Unit = unit;
		}
		public MeasurementUnit Unit;
		public double Measure;
	}

	public enum MeasurementUnit
	{
		Unknown,
		Px,
		Pt,
		Percent,
		EM,
	}
}
