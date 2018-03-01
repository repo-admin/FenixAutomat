using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace FenixAutomat
{
	/// <summary>
	/// Místo(zdroj), kde vznikl interní doklad
	/// </summary>
	public enum InternalDocumentsSource
	{
		/// <summary>
		/// Fenix - skladová karta
		/// </summary>
		FenixCardStock = 1,

		/// <summary>
		/// Fenix - uvolnění kitů z TRR
		/// </summary>
		FenixReleaseKit = 2,

		/// <summary>
		/// FenixAutomat - D0 delete message email
		/// </summary>
		FenixAutomatDeleteMessageEmail = 3,

		/// <summary>
		/// Ručně (ad hoc)
		/// </summary>
		Manually = 4
	} 

	internal class BC
	{
		/// <summary>
		/// <value>FenixAutomat</value>
		/// </summary>
		internal const string APP_NAMESPACE = "FenixAutomat";

		/// <summary>
		/// URL na w3.org XML Schema
		/// <value>http://www.w3.org/2001/XMLSchema</value>
		/// </summary>
		internal const string URL_W3_ORG_SCHEMA = "http://www.w3.org/2001/XMLSchema";

		/// <summary>
		/// OK
		/// <value>0</value>
		/// </summary>
		internal const int OK = 0;

		/// <summary>
		/// Not OK
		/// <value>-1</value>
		/// </summary>
		internal const int NOT_OK = -1;

		/// <summary>
		/// do ND se nepodařilo správně zapsat
		/// <value>2</value>
		/// </summary>
		internal const int WRITE_TO_ND_NOT_OK = 2;

		/// <summary>
		/// do ND se podařilo správně zapsat
		/// <value>3</value>
		/// </summary>
		internal const int WRITE_TO_ND_OK = 3;

		/// <summary>
		/// Unknown		
		/// </summary>
		internal const string UNKNOWN = "Unknown";
				
		private const string LOG_FILE = "FenixAutomat.log";

		#region Properties

		/// <summary>
		/// Jméno logovacího souboru včetně cesty
		/// </summary>
		internal static string LogFile
		{			
			get 
			{
				string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				return Path.Combine(assemblyPath, LOG_FILE); 
			}
		}

		/// <summary>
		/// Login (do ND; přiděluje ND)
		/// </summary>
		internal static string Login
		{
			get { try { return ConfigurationManager.AppSettings["Login"].Trim(); } catch { return String.Empty; } }
		}

		/// <summary>
		/// Heslo (do ND; přiděluje ND)
		/// </summary>
		internal static string Password
		{
			get { try { return ConfigurationManager.AppSettings["Password"].Trim(); } catch { return String.Empty; } }
		}

		/// <summary>
		/// Partnerský kód (do ND; přiděluje ND)
		/// </summary>
		internal static string PartnerCode
		{
			get { try { return ConfigurationManager.AppSettings["PartnerCode"].Trim(); } catch { return String.Empty; } }
		}

		/// <summary>
		/// Výchozí kódování XML
		/// </summary>
		internal static string Encoding
		{
			get { try { return ConfigurationManager.AppSettings["Encoding"].Trim(); } catch { return String.Empty; } }
		}

		/// <summary>
		/// Max. počet XML zpráv, které se najednou zpracují
		/// </summary>
		internal static int NumRowsToSend
		{
			get { try { return int.Parse(ConfigurationManager.AppSettings["NumRowsToSend"].Trim()); } catch { return 50; } }
		}

		/// <summary>
		/// ZICYZ Id pod kterým aplikace 'běží'
		/// </summary>
		internal static int ServiceUserId
		{
			get { try { return int.Parse(ConfigurationManager.AppSettings["ZicyzUserId"].Trim()); } catch { return 89999; } }
		}
		
		/// <summary>
		/// Příznak (ne)povolení výkonné části programu
		/// </summary>
		internal static int ApplicationCanRun
		{
			get { try { return int.Parse(ConfigurationManager.AppSettings["ApplicationCanRun"].Trim()); } catch { return 1; } }
		}
		
		/// <summary>
		/// Příznak (ne)povolení odeslání XML zpráv do ND
		/// </summary>
		internal static int SendXmlMessageToNDIsEnabled
		{
			get { try { return int.Parse(ConfigurationManager.AppSettings["SendXmlMessageToND"].Trim()); } catch { return 1; } }
		}

		internal static string MailServer
		{
			get { try { return ConfigurationManager.AppSettings["MailServer"].Trim(); } catch { return "relay.upc.cz"; } }
		}

		internal static string MailFrom
		{
			get { try { return ConfigurationManager.AppSettings["MailFrom"].Trim(); } catch { return "automat.fenix@upc.cz"; } }
		}

		internal static string MailTo
		{
			get { try { return ConfigurationManager.AppSettings["MailTo"].Trim(); } catch { return "michal.rezler@upc.cz"; } }
		}

		internal static string MailErrorTo
		{
			get { try { return ConfigurationManager.AppSettings["MailErrorTo"].Trim(); } catch { return "michal.rezler@upc.cz"; } }
		}

		internal static int ProductionMode
		{
			get { try { return int.Parse(ConfigurationManager.AppSettings["ProductionMode"].Trim()); } catch { return 1; } }
		}

		internal static string D0MailFrom
		{
			get { try { return ConfigurationManager.AppSettings["D0MailFrom"].Trim(); } catch { return "fenix@upc.cz"; } }
		}

		internal static string D0MailTo
		{
			get { try { return ConfigurationManager.AppSettings["D0MailTo"].Trim(); } catch { return "michal.rezler@upc.cz"; } }
		}

		internal static string Domain
		{
			get { try { return ConfigurationManager.AppSettings["Domain"].Trim(); } catch { return "upc"; } }
		}

		internal static string DomainUserLogin
		{
			get { try { return ConfigurationManager.AppSettings["DomainUserLogin"].Trim(); } catch { return "fenix"; } }
		}

		internal static string DomainUserPassword
		{
			get { try { return ConfigurationManager.AppSettings["DomainUserPassword"].Trim(); } catch { return "PtakFenix*1993"; } }
		}

		internal static string ExchangeURL
		{
			get { try { return ConfigurationManager.AppSettings["ExchangeURL"].Trim(); } catch { return "https://mail.upc.cz/EWS/Exchange.asmx"; } }
		}

		/// <summary>
		/// Delete Message pomocí XML
		/// </summary>
		internal static bool DeleteMessageViaXML
		{
			get { try { return ConfigurationManager.AppSettings["DeleteMessageViaXML"] == "1"; } catch { return false; } }
		}

		#endregion

		/// <summary>
		/// Vytvoří chybový ProcResult
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		internal static ProcResult CreateErrorResult(string methodName, Exception ex)
		{
			ProcResult result = new ProcResult();

			result.ReturnValue = BC.NOT_OK;
			result.ReturnMessage = String.Format("{0}{1}{2}", methodName, Environment.NewLine, ex.Message);

			if (ex.InnerException != null)
				result.ReturnMessage = result.ReturnMessage + Environment.NewLine + ex.InnerException.Message;

			return result;
		}

		/// <summary>
		/// Vytvoří chybové hlášení
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		internal static string CreateErrorMessage(string methodName, Exception exception)
		{
			string errorMessage = String.Format("{0}{1}{2}", methodName, Environment.NewLine, exception.Message);
			if (exception.InnerException != null)
				errorMessage += Environment.NewLine + exception.InnerException.Message;

			return errorMessage;
		}

		/// <summary>
		/// Ze vstupního řetězce vytvoří hexa řetězec představující SHA256 hash
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		internal static string CreateSHA256Hash(string input)
		{
			byte[] data;

			using (SHA256Managed sha256Hash = new SHA256Managed())
			{
				data = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
			}

			StringBuilder sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			return sBuilder.ToString();
		}

		/// <summary>
		/// Vrací jmého databáze
		/// </summary>
		/// <returns>jméno databáze/prázdný string</returns>
		internal static string GetDatabaseName()
		{
			string result = String.Empty;
						
			using (var db = new FenixEntities())
			{
				try
				{
					result = db.Database.Connection.Database.ToString();
				}
				catch
				{					
				}
			}

			return result;
		}
	}
}
