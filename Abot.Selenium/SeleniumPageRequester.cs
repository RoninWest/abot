using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Abot.Core;
using Abot.Poco;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using com.LandonKey.SocksWebProxy;
using log4net;

namespace Abot.Selenium
{
	[Serializable]
	public class SeleniumPageRequester : IPageRequester
	{
		static readonly ILog _logger =  LogManager.GetLogger(typeof(SeleniumPageRequester));

		readonly protected CrawlConfiguration _config; //guaranteed
		readonly protected RemoteWebDriver _driver; //guaranteed
		readonly TorProcess _tor; //maybe null, depending on input

		#region CTOR & DTOR

		public SeleniumPageRequester(CrawlConfiguration config, Func<RemoteWebDriver> getDriver = null)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			_config = config;
			if (_config.HttpServicePointConnectionLimit > 0)
				ServicePointManager.DefaultConnectionLimit = _config.HttpServicePointConnectionLimit;
			if (!_config.IsSslCertificateValidationEnabled)
				ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

			if (getDriver != null)
				_driver = getDriver();
			else if (_config.UseTorProxy)
			{
				_tor = new TorProcess();
				_driver = new TorBrowserDriver(_tor);
			}
			else
				_driver = new FirefoxDriver(); //standard firefox driver

			_logger.DebugFormat("CTOR: Using {0}", _driver.GetType().FullName);
		}

		~SeleniumPageRequester() { Dispose(); }
		int _disposing = 0;
		public virtual void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposing, 1, 0) == 0)
			{
				try {
					if (_driver != null)
					{
						try
						{
							_driver.Quit();
							_driver.Dispose();
						}
						catch (Exception ex)
						{
							_logger.Warn("Dispose (inside)", ex);
						}
					}
					if (_tor != null)
						_tor.Dispose();
				}
				catch(Exception oex)
				{
					_logger.Warn("Dispose (outside)", oex);
				}
			}
		}

		#endregion

		public virtual CrawledPage MakeRequest(Uri uri)
		{
			return MakeRequest(uri, (x) => new CrawlDecision { Allow = true });
		}

		public virtual CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			var pg = new CrawledPage(uri);
			pg.RequestStarted = DateTime.Now;
			INavigation nav = _driver.Navigate();
			nav.GoToUrl(uri.ToString());

			var content = pg.Content = new PageContent
			{
				Text = _driver.PageSource,
			};
			if(content.Charset == null)
				content.Charset = GetCharsetFromBody(content.Text);
			if (content.Charset == null)
				content.Charset = Encoding.Default.WebName;
			if(content.Encoding == null)
				content.Encoding = GetEncoding(content.Charset);

			return pg;
		}

		/// <summary>
		/// <see cref="http://stackoverflow.com/questions/3458217/how-to-use-regular-expression-to-match-the-charset-string-in-html"/>
		/// </summary>
		static readonly Regex CHAR_SET_RE = new Regex(
			@"<meta(?!\s*(?:name|value)\s*=)(?:[^>]*?content\s*=[\s""']*)?([^>]*?)[\s""';]*charset\s*=[\s""']*([^\s""'/>]*)", 
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		protected string GetCharsetFromBody(string body)
		{
			string charset = null;
			if (!string.IsNullOrWhiteSpace(body))
			{
				try {
					Match match = CHAR_SET_RE.Match(body);
					if (match.Success && match.Groups.Count > 2 &&
						string.IsNullOrWhiteSpace(match.Groups[2].Value))
					{
						charset = match.Groups[2].Value;
					}
				}
				catch(Exception ex)
				{
					_logger.Warn("GetCharsetFromBody: " + _driver.Url, ex);
				}
			}
			return charset;
		}

		protected Encoding GetEncoding(string charset)
		{
			Encoding e = Encoding.UTF8;
			if (charset != null)
			{
				try
				{
					e = Encoding.GetEncoding(charset);
				}
				catch(Exception ex)
				{
					_logger.Warn("GetEncoding: " + charset + " for " + _driver.Url, ex);
				}
			}
			return e;
		}
	}
}
