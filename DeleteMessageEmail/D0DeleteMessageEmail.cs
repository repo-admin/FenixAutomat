using System;
using System.Linq;
using FenixAutomat.EmailCreator.DeleteEmail;
using FenixAutomat.EmailSender;
using FenixAutomat.Loggers;
using FenixAutomat.Message.SetSent;
using FenixHelper;
using FenixHelper.Common;

namespace FenixAutomat.DeleteMessageEmail
{
	/// <summary>
	/// D0 - vytvoří a odešle email message pro zrušení
	/// - objednávky recepce                          (R0 message) 
	/// - objednávky kittingu                         (K0 message) 	
	/// - objednávky expedice/závozu                  (S0 message)
	/// - objednávky naskladnění repasovaného zboží	  (RF0 message)
	/// </summary>
	public class D0DeleteMessageEmail
	{
		/// <summary>
		/// zicyz ID aktuálního uživatele
		/// </summary>
		private int zicyzUserId;
				
		/// <summary>
		/// Záznam čeká na odeslání 
		/// </summary>
		private const int MessageStatusWaitForSend = 1;

		/// <summary>
		/// Záznam odeslán emailem (email odeslán do ND/XPO)
		/// </summary>
		private const int MessageStatusEmailSent = 11;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="zicyzUserId"></param>
		public D0DeleteMessageEmail(int zicyzUserId)
		{
			this.zicyzUserId = zicyzUserId;
		}

		/// <summary>
		/// Vytvoření a odeslání emailu pro zrušení message na strane ND/XPO
		/// </summary>
		/// <returns></returns>
		public ResultAppService CreateAndSendEmails()
		{			
			ResultAppService result = new ResultAppService();

			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;

					var deleteMessages = (from b in db.DeleteMessageSent
									      orderby b.ID ascending
										  where b.IsActive == true && b.MessageStatusId == MessageStatusWaitForSend 
									      select b).Take(BC.NumRowsToSend);
					
					result.AddResultMessage(String.Format("Počet DeleteMessage {0}", deleteMessages != null ? deleteMessages.Count() : 0));

					foreach (var deleteMessage in deleteMessages)
					{
						this.ProcessDeleteMessage(result, deleteMessage);
					}
				}
			}
			catch (Exception e)
			{				
				Logger.ProcessError(result, e, AppLog.GetMethodName(), BC.ServiceUserId);
			}

			return result;
		}

		/// <summary>
		/// Vlastní vytvoření a odeslání emailu pro zrušení message na strane ND/XPO
		/// (odeslaný email se ukládá do databáze - datová zádrž, aby email byl odeslán právě 1x)
		/// </summary>
		/// <param name="result"></param>
		/// <param name="deleteMessage"></param>
		private void ProcessDeleteMessage(ResultAppService result, DeleteMessageSent deleteMessage)
		{
			ResultAppService res = new ResultAppService();

			switch (deleteMessage.DeleteMessageTypeId)
			{
				case 1:
					ReceptionOrderDeleteEmail receptionDeleteEmail = new ReceptionOrderDeleteEmail(deleteMessage.DeleteId, deleteMessage.DeleteMessageId);
					res = receptionDeleteEmail.CreateAndSendEmail();
					break;
				case 3:
					KittingOrderDeleteEmail kittingOrderDeleteEmail = new KittingOrderDeleteEmail(deleteMessage.DeleteId, deleteMessage.DeleteMessageId);
					res = kittingOrderDeleteEmail.CreateAndSendEmail();
					break;
				case 6:
					ShipmentOrderDeleteEmail shipmentOrderDeleteEmail = new ShipmentOrderDeleteEmail(deleteMessage.DeleteId, deleteMessage.DeleteMessageId);
					res = shipmentOrderDeleteEmail.CreateAndSendEmail();
					break;
				case 12:
					RefurbishedOrderDeleteEmail refurbishedOrderDeleteEmail = new RefurbishedOrderDeleteEmail(deleteMessage.DeleteId, deleteMessage.DeleteMessageId);
					res = refurbishedOrderDeleteEmail.CreateAndSendEmail();
					break;
				default:
					break;
			}

			if (res.ResultMessage.IsNotNullOrEmpty())
			{
				if ((result.ResultNumber != res.ResultNumber) && (result.ResultNumber != BC.OK))
					result.AddResultMessage(res.ResultMessage);
				else
					result.AddResultMessage(res.ResultNumber, res.ResultMessage);
			}

			if (res.ResultNumber == 0)
			{
				DeleteMessageEmailSetSent deleteMessageEmailSetSent = new DeleteMessageEmailSetSent(deleteMessage.ID, MessageStatusEmailSent, this.zicyzUserId);
				deleteMessageEmailSetSent.SetSent();
				this.SendInfoEmail(deleteMessage.ID, AppLog.GetMethodName());
			}
		}

	    /// <summary>
	    /// Odešle email s informací o odeslání DeleteMessageEmail (D0Message)
	    /// <para>(tento email se odešle právě 1x)</para>
	    /// </summary>
	    /// <param name="id">Identifikátor záznamu mazací zprávy v databázi</param>
	    /// <param name="methodName"></param>
	    private void SendInfoEmail(int id, string methodName)
		{			
			string message = string.Format("D0  DeleteMessageEmail ID = [{0}] odesláno do ND.", id);

			Email infoEmail = new Email()
			{
				Type = BC.APP_NAMESPACE,
				Subject = String.Format("{0} - Databáze: {1}", "Fenix Automat", BC.GetDatabaseName()),
				Body = message,
				MailTo = BC.MailTo,
				SendOnlyOnce = true,
				Source = methodName
			};

			infoEmail.SendMail();
		}
	}
}
