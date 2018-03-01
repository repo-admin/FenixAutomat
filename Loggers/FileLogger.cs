using System;
using System.Text;

namespace FenixAutomat.Loggers
{
	/// <summary>
	/// Logování (do souboru na disku)
	/// </summary>
	public class FileLogger
	{
		#region Public Methods

		/// <summary>zápis do LOG souboru.</summary>
		/// <param name="logFile">úplné jméno LOG souboru</param>
		/// <param name="msg">text zprávy</param>
		public static void WriteToLog(string logFile, string msg)
		{
			System.IO.StreamWriter wr = null;
			try
			{
				wr = new System.IO.StreamWriter(logFile, true, Encoding.GetEncoding(1250));
				wr.WriteLine(msg);
				wr.Flush();
				wr.Close();
			}
			catch
			{
				if (wr != null)
					wr.Close();
			}
		}

		/// <summary>
		/// příprava zprávy (je rozšířena o datum a čas)
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string PrepareMsg(string message)
		{ 
			return String.IsNullOrEmpty(message) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") 
				                                 : String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);			
		}

		/// <summary>
		/// smazání LOG souboru pokud přesáhne definovanou velikost
		/// </summary>
		/// <param name="logFile"></param>
		public static void DeleteLogFile(string logFile)
		{
			// odstranění LOGu, je-li větší než ~ 500 MB
			try
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(logFile);
				if (fi.Length > (500 * 1024 * 1024))
					fi.Delete();
			}
			catch { }
		}
	
		#endregion
	}
}
