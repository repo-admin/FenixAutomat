using System;
using System.Text;
using System.Data.Entity.Core.Objects;
using UPC.Extensions.Convert;
using UPC.Extensions.Enum;
using FenixHelper.Common;
using FenixHelper;
// ReSharper disable All

namespace FenixAutomat.Loggers
{
	/// <summary>
	/// Logování do databáze
	/// </summary>
	public class DbLogger
	{
		#region Public Methods

		/// <summary>
		/// Zápis XML zprávy do tabulky logu
		/// </summary>
		/// <param name="message"></param>
		/// <param name="xmlMessage"></param>
		/// <param name="source"></param>
		/// <param name="zicyzUserId"></param>
		public static void WriteXmlMessageToLog(string message, string xmlMessage, string source, int zicyzUserId)
		{
			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));

					db.prAppLogWriteNew("XML", message, String.Empty, xmlMessage, zicyzUserId, source, retVal, retMsg);
				}
			}
			catch
			{
				;
			}
		}

	    /// <summary>
	    /// Zápis do tabulky logu
	    /// <para>(případná chyba se ignoruje)</para>
	    /// </summary>
	    /// <param name="message"></param>
	    /// <param name="source"></param>
	    /// <param name="zicyzUserId"></param>
	    /// <param name="logType"></param>
	    public static void WriteToLog(string logType, string message, string source, int zicyzUserId)
		{
			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));

					db.prAppLogWriteNew(logType, message, String.Empty, String.Empty, zicyzUserId, source, retVal, retMsg);
				}
			}
			catch
			{
				;
			}
		}

		/// <summary>
		/// Zápis výsledku operace do tabulky logu
		/// <para>(případná chyba se ignoruje)</para>
		/// </summary>
		/// <param name="result"></param>
		/// <param name="source"></param>
		/// <param name="zicyzUserId"></param>
		public static void WriteToLog(ResultAppService result, string source, int zicyzUserId)
		{
			try
			{
				string logType = result.ResultNumber == BC.OK ? "INFO" : "ERROR";
				string message = String.Format("MethodName [{0}] result.ResultNumber [{1}] result.ResultMessage [{2}]", source, result.ResultNumber, result.ResultMessage);

				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));

					db.prAppLogWriteNew(logType, message, String.Empty, String.Empty, zicyzUserId, source, retVal, retMsg);
				}
			}
			catch
			{
				;
			}
		}

		#endregion
	}
}
