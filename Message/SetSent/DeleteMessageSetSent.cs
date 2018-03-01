using System;
using System.Linq;
using FenixAutomat.Loggers;
using FenixHelper;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastavení datumu odeslání, uživatele co odeslal a statusu zprávy pro delete message (D0 - DeleteMessage)
	/// </summary>
	public class DeleteMessageSetSent : ISetSent
	{
		/// <summary>
		/// 
		/// </summary>
		public int MessageID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int UserID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int MessageStatusID { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		private DeleteMessageSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="messageID"></param>
		/// <param name="messageStatusID"></param>
		public DeleteMessageSetSent(int messageID, int messageStatusID, int userID)
		{
			this.MessageID = messageID;
			this.MessageStatusID = messageStatusID;
			this.UserID = userID;
		}

		/// <summary>
		/// vlastní nastavení
		/// </summary>
		/// <returns></returns>
		public string SetSent()
		{
			string result = String.Empty;

			using (var db = new FenixEntities())
			{
				try
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;

					var deleteMessageOrder = (from b in db.CommunicationMessagesDeleteMessage											  
											  where b.IsActive == true && b.MessageId == this.MessageID
											  select b).FirstOrDefault();

					if (deleteMessageOrder != null)
					{
						deleteMessageOrder.MessageStatusId = this.MessageStatusID;
						deleteMessageOrder.SentDate = DateTime.Now;
						deleteMessageOrder.SentUserId = this.UserID;
						db.SaveChanges();
					}

					result = String.Format("{0} returnValue [{1}] returnMessage [{2}]", AppLog.GetMethodName(), 0, String.Empty);
				}
				catch (Exception ex)
				{
					Logger.ProcessError(ex, AppLog.GetMethodName(), BC.ServiceUserId);
				}
			}

			return result;
		}
	}

}
