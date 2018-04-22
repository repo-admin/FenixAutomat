using System;
using FenixAutomat.Loggers;
using FenixAutomat.Message.Sender;
using FenixHelper.Common;
using FenixAutomat.EmailSender;
// ReSharper disable All

namespace FenixAutomat.Message
{
	/// <summary>
	/// Bázová třída všechny typy XML zpráv odesílaných do ND
	/// </summary>
	public class BaseMessage
	{
		/// <summary>
		/// výsledek
		/// </summary>
		public ResultAppService Result { get; set; }

		/// <summary>
		/// zicyz ID aktuálního uživatele
		/// </summary>
		public int ZicyzUserID { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="zicyzUserId"></param>
		public BaseMessage(int zicyzUserId)
		{
			this.Result = new ResultAppService();
			this.ZicyzUserID = zicyzUserId;
		}

        #region Protected Methods

        /// <summary>
        /// Zpracování chyby, která nastala ve třídě <seealso cref="ListsCreator"/> 
        /// </summary>
        /// <param name="returnError"></param>
        /// <param name="methodName"></param>
        protected void ProcessListsCreatorError(string returnError, string methodName)
		{
			this.Result.AddResultMessage(BC.NOT_OK, returnError);
			Logger.ProcessError(returnError, methodName, BC.ServiceUserId);
		}

	    /// <summary>
	    /// Akce po odeslání XML zprávy do ND
	    /// - pošle email s informací o odeslání zprávy do ND
	    /// - pokud ND vrátilo chybu, je odeslán další mail s detailem této chyby a chyba je zapsána do databáze		
	    /// </summary>
	    /// <param name="message"></param>
	    /// <param name="ndResult"></param>
	    /// <param name="methodName"></param>
	    protected void ActionsAfterSendingXmlMessageToND(string message, ReturnedValueFromND ndResult, string methodName)
		{
			this.sendInfoEmail(message, ndResult, methodName);

			if (!ndResult.ReturnedValueIsOK)
			{
				this.sendErrorEmail(message, ndResult, methodName);
				DbLogger.WriteToLog("ERROR", ndResult.Message, methodName, BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Odeslání XML zprávy do ND
		/// </summary>
		/// <param name="xmlSourceString"></param>
		/// <param name="messageType"></param>
		/// <returns></returns>
		protected ReturnedValueFromND SendXmlMessageToND(string xmlSourceString, string messageType)
		{			
			MessageSender messageSender = MessageSender.CreateMessageSender();
			
			ReturnedValueFromND ndResult = messageSender.SendMessageToND(xmlSourceString, messageType);
			this.Result.AddResultMessage(String.Format("ND result {0}", ndResult.Message));

			return ndResult;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Odešle chybový email
		/// </summary>
		/// <param name="message"></param>
		/// <param name="ndResult"></param>
		/// <param name="methodName"></param>
		private void sendErrorEmail(string message, ReturnedValueFromND ndResult, string methodName)
		{
			Email errorEmail = new Email()
			{
				Type = BC.APP_NAMESPACE,				
				Subject = String.Format("{0} - Databáze: {1}", "Fenix Automat ERROR", BC.GetDatabaseName()),
				Body = String.Format("{0}{1}{1}ND vrátilo chybu:{1}{2}", message, Environment.NewLine, ndResult.Message),
				MailTo = BC.MailErrorTo,
				Source = methodName
			};

			errorEmail.SendMail();
		}

		/// <summary>
		/// Odešle informační email
		/// <para>pokud na straně ND nastala chyba:</para>
		/// <para>- do těla emailu se přidá informace o chybě (pouze informativní text)</para>
		/// <para>(tento email se odešle právě 1x)</para>
		/// </summary>
		/// <param name="message"></param>
		/// <param name="ndResult"></param>
		/// <param name="methodName"></param>
		private void sendInfoEmail(string message, ReturnedValueFromND ndResult, string methodName)
		{
			string emailMessage = prepareEmailBody(message, ndResult);

			Email infoEmail = new Email()
			{
				Type = BC.APP_NAMESPACE,
				Subject = String.Format("{0} - Databáze: {1}", "Fenix Automat", BC.GetDatabaseName()),
				Body = emailMessage,
				MailTo = BC.MailTo,
				SendOnlyOnce = ndResult.ReturnedValueIsOK ? false : true,
				Source = methodName
			};

			infoEmail.SendMail();
		}

		/// <summary>
		/// Příprava těla mailu před odesláním - do těla emailu se přidá informace o chybě na straně ND (pokud chyba nastala)
		/// </summary>
		/// <param name="message"></param>
		/// <param name="ndResult"></param>
		/// <returns></returns>
		private static string prepareEmailBody(string message, ReturnedValueFromND ndResult)
		{
			string emailMessage = message;
			emailMessage += (!ndResult.ReturnedValueIsOK) ? String.Format("{0}Zpracování zprávy na straně ND vrátilo chybu.", Environment.NewLine) : String.Empty;
			
			return emailMessage;
		}

		#endregion
	}
}
