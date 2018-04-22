using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FenixHelper;
using FenixAutomat.EmailSender;
using FenixHelper.Common;
// ReSharper disable All

namespace FenixAutomat.Loggers
{
	/// <summary>
	/// Agregovaný logger adaptér pro logování do databáze a do souboru
	/// </summary>
	public class Logger
	{
		/// <summary>
		/// zápis XML message do logů
		/// </summary>
		/// <param name="message"></param>
		/// <param name="xmlMessage"></param>
		/// <param name="source"></param>
		/// <param name="zicyzUserId"></param>
		public static void PrepareAndWriteXmlMessageToLogs(string message, string xmlMessage, string source, int zicyzUserId)
		{			
			FileLogger.WriteToLog(BC.LogFile, message);
			FileLogger.WriteToLog(BC.LogFile, xmlMessage);

			// připraví XML message (zruší deklarační část a jmenné prostory)
			string modifiedXmlString = XmlCreator.CreateXMLRootNode(xmlMessage);
			DbLogger.WriteXmlMessageToLog(message, modifiedXmlString, source, zicyzUserId);
		}

	    /// <summary>
	    /// zápis zprávy do logů
	    /// </summary>
	    /// <param name="messageType"></param>
	    /// <param name="addDateTime"></param>
	    /// <param name="message"></param>
	    /// <param name="methodName"></param>
	    /// <param name="zicyzID"></param>
	    public static void WriteIntoLoggers(string messageType, bool addDateTime, string message, string methodName, int zicyzID)
		{
			if (addDateTime)
			{
				FileLogger.WriteToLog(BC.LogFile, FileLogger.PrepareMsg(message));
			}
			else
			{
				FileLogger.WriteToLog(BC.LogFile, message);
			}

			if (!String.IsNullOrEmpty(messageType))
			{
				string preparedMessage = PrepareMessage(message);
				DbLogger.WriteToLog(messageType, preparedMessage, methodName, zicyzID);
			}
		}

		/// <summary>
		/// zápis zprávy do logů
		/// </summary>
		/// <param name="messageType"></param>
		/// <param name="message"></param>
		/// <param name="methodName"></param>
		/// <param name="zicyzID"></param>
		public static void WriteIntoLoggers(string messageType, string message, string methodName, int zicyzID)
		{
			FileLogger.WriteToLog(BC.LogFile, message);

			if (!String.IsNullOrEmpty(messageType))
			{
				string preparedMessage = PrepareMessage(message);
				DbLogger.WriteToLog(messageType, preparedMessage, methodName, zicyzID);
			}
		}

	    /// <summary>
	    /// Zpracování vyjímky
	    /// <para>- zápis vyjímky do logů</para>		
	    /// <para>- odeslání chybového emailu</para>
	    /// </summary>
	    /// <param name="result"></param>
	    /// <param name="exception"></param>
	    /// <param name="methodName"></param>
	    /// <param name="zicyzID"></param>		
	    public static void ProcessError(ResultAppService result, Exception exception, string methodName, int zicyzID)
		{
			string exceptionMessage = exception != null ? exception.Message : "UNKNOWN error!";

			result.ResultNumber = BC.NOT_OK;
			result.AddResultMessage(exceptionMessage);
			
			ProcessError(exceptionMessage, methodName, zicyzID);

		}

		/// <summary>
		/// Zpracování vyjímky
		/// <para>- zápis vyjímky do logů</para>		
		/// <para>- odeslání chybového emailu</para>
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="methodName"></param>
		/// <param name="zicyzID"></param>		
		public static void ProcessError(Exception exception, string methodName, int zicyzID)
		{			
			string exceptionMessage = exception != null ? exception.Message : "UNKNOWN error!";
			
			ProcessError(exceptionMessage, methodName, zicyzID);
		}

		/// <summary>
		/// Zpracování vyjímky
		/// <para>- zápis vyjímky do logů</para>		
		/// <para>- odeslání chybového emailu</para>
		/// </summary>
		/// <param name="errorMessage"></param>
		/// <param name="methodName"></param>
		/// <param name="zicyzID"></param>
		public static void ProcessError(string errorMessage, string methodName, int zicyzID)
		{
			string errorMsg = !String.IsNullOrEmpty(errorMessage) ? errorMessage : "UNKNOWN error!";

			FileLogger.WriteToLog(BC.LogFile, FileLogger.PrepareMsg(errorMsg));
			DbLogger.WriteToLog("ERROR", errorMsg, methodName, zicyzID);

			Email.SendMail("Fenix Automat Error", String.Format("{0}{1}{2}", methodName, Environment.NewLine, errorMsg), false, BC.MailErrorTo, "", "");
		}

		/// <summary>
		/// Zápis zprávy do logů
		/// </summary>
		/// <param name="messageType"></param>
		/// <param name="exception"></param>
		/// <param name="methodName"></param>
		/// <param name="zicyzID"></param>
		public static void WriteIntoLoggersORG(string messageType, Exception exception, string methodName, int zicyzID)
		{
			FileLogger.WriteToLog(BC.LogFile, FileLogger.PrepareMsg(exception.Message));

			if (!String.IsNullOrEmpty(messageType))
			{
				DbLogger.WriteToLog(messageType, exception.Message, methodName, zicyzID);
			}
		}

		/// <summary>
		/// upraví zprávu, která se zapisuje do databáze
		/// (tvar 'yyyy-MM-dd HH:mm:ss.fff Enviroment.NewLine text zprávy' je převeden na 'text zprávy')
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private static string PrepareMessage(string message)
		{
			string result = String.Empty;

			try
			{
				string[] elementsInMessage = message.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

				if (elementsInMessage.GetLength(0) == 1)
				{
					result = message;
				}
				else
				{
					for (int i = 0; i < elementsInMessage.GetLength(0); i++)
					{
						if (i % 2 != 0)
						{
							result += elementsInMessage[i] + "  ";
						}
					}
				}
			}
			catch (Exception)
			{
				result = message;
			}

			return result.Trim();
		}
	}
}
