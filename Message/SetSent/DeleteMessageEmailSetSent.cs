using System;
using System.Data.Entity.Core.Objects;
using Fenix;
using FenixAutomat.Loggers;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastavení datumu odeslání, uživatele co odeslal a statusu zprávy pro delete message email (D0 - DeleteMessage[Order] Sent)
	/// </summary>
	public class DeleteMessageEmailSetSent : ISetSent
	{
		/// <summary>
		/// 
		/// </summary>
		public int DeleteMessageID { get; set; }

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
		private DeleteMessageEmailSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="deleteMessageID"></param>
		/// <param name="messageStatusID"></param>
		public DeleteMessageEmailSetSent(int deleteMessageID, int messageStatusID, int userID)
		{
			this.DeleteMessageID = deleteMessageID;
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
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));
										
					db.prDeleteMessageEmailSetSent(this.DeleteMessageID, this.MessageStatusID, BC.ServiceUserId, retVal, retMsg);

					result = String.Format("{0} returnValue [{1}] returnMessage [{2}]", ApplicationLog.GetMethodName(), retVal.Value.ToInt32(BC.NOT_OK), retMsg.Value.ToString(String.Empty));
				}
				catch (Exception ex)
				{
					Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
				}
			}

			return result;
		}
	}
}
