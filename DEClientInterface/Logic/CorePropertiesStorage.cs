// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Logic.CorePropertiesStorage
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEClientInterface.InterfaceLogic;
using ExcavatorSharp.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DEClientInterface.Logic
{
	public class CorePropertiesStorage
	{
		public void LoadDECoreProperties()
		{
			try
			{
				if (!File.Exists(DEClientInterface.InterfaceLogic.IOCommon.GetDataExcavatorCommonPropertiesFilePath()))
				{
					return;
				}
				string value = File.ReadAllText(DEClientInterface.InterfaceLogic.IOCommon.GetDataExcavatorCommonPropertiesFilePath());
				Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
				if (dictionary.ContainsKey("CEFUserAgentCommon"))
				{
					DEConfig.CEFUserAgentCommon = dictionary["CEFUserAgentCommon"].ToString();
				}
				if (dictionary.ContainsKey("ProxyServerTestingResourceLink"))
				{
					DEConfig.ProxyServerTestingResourceLink = dictionary["ProxyServerTestingResourceLink"].ToString();
				}
				if (dictionary.ContainsKey("ProxyServerTestingExpectedSubstringInSourceCode"))
				{
					DEConfig.ProxyServerTestingExpectedSubstringInSourceCode = dictionary["ProxyServerTestingExpectedSubstringInSourceCode"].ToString();
				}
				if (dictionary.ContainsKey("TrustedHostsGlobal") && dictionary["TrustedHostsGlobal"] != null)
				{
					DEConfig.TrustedHostsGlobal = (from item in (dictionary["TrustedHostsGlobal"] as JArray).AsEnumerable()
						select item.ToString()).ToArray();
				}
				if (dictionary.ContainsKey("HttpConnectionsMaxCount"))
				{
					DEConfig.HttpConnectionsMaxCount = Convert.ToInt32(dictionary["HttpConnectionsMaxCount"]);
				}
				if (dictionary.ContainsKey("HttpWebRequest_Expect100Continue"))
				{
					DEConfig.HttpWebRequest_Expect100Continue = Convert.ToBoolean(dictionary["HttpWebRequest_Expect100Continue"].ToString());
				}
				if (dictionary.ContainsKey("HttpWebRequest_CheckCertificateRevocationList"))
				{
					DEConfig.HttpWebRequest_CheckCertificateRevocationList = Convert.ToBoolean(dictionary["HttpWebRequest_CheckCertificateRevocationList"].ToString());
				}
				if (dictionary.ContainsKey("MaximumCrawlDelayValueInSeconds"))
				{
					DEConfig.MaximumCrawlDelayValueInSeconds = Convert.ToInt32(dictionary["MaximumCrawlDelayValueInSeconds"].ToString());
				}
				if (dictionary.ContainsKey("AllowToSendCrashReports"))
				{
					DEConfig.AllowToSendCrashReports = Convert.ToBoolean(dictionary["AllowToSendCrashReports"].ToString());
				}
			}
			catch (Exception thrownException)
			{
				Logger.LogError("Core properties storage exception", thrownException);
			}
		}

		public void SaveCoreProperties()
		{
			Dictionary<string, object> dEPropertiesFromCore = GetDEPropertiesFromCore();
			try
			{
				string contents = JsonConvert.SerializeObject(dEPropertiesFromCore, Formatting.Indented);
				File.WriteAllText(DEClientInterface.InterfaceLogic.IOCommon.GetDataExcavatorCommonPropertiesFilePath(), contents);
			}
			catch (Exception thrownException)
			{
				Logger.LogError("Core properties storage exception", thrownException);
			}
		}

		public static Dictionary<string, object> GetDEPropertiesFromCore()
		{
			return new Dictionary<string, object>
			{
				{ "CEFUserAgentCommon", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" },
				{
					"ProxyServerTestingResourceLink",
					DEConfig.ProxyServerTestingResourceLink
				},
				{
					"ProxyServerTestingExpectedSubstringInSourceCode",
					DEConfig.ProxyServerTestingExpectedSubstringInSourceCode
				},
				{
					"TrustedHostsGlobal",
					DEConfig.TrustedHostsGlobal
				},
				{
					"HttpConnectionsMaxCount",
					DEConfig.HttpConnectionsMaxCount
				},
				{
					"HttpWebRequest_Expect100Continue",
					DEConfig.HttpWebRequest_Expect100Continue
				},
				{
					"HttpWebRequest_CheckCertificateRevocationList",
					DEConfig.HttpWebRequest_CheckCertificateRevocationList
				},
				{
					"MaximumCrawlDelayValueInSeconds",
					DEConfig.MaximumCrawlDelayValueInSeconds
				},
				{
					"AllowToSendCrashReports",
					DEConfig.AllowToSendCrashReports
				}
			};
		}
	}
}