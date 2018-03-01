using System;
using System.Data.Entity.Core.Objects;
using FenixAutomat.Loggers;
using FenixHelper;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastavení datumu odeslání a statusu zprávy pro požadavek zrušení XML message na straně ND (Delete Message Order)
	/// </summary>
	public class DeleteMessageOrderSetSent : ISetSent
	{
		/// <summary>
		/// ID požadavku na zrušení XML message
		/// </summary>
		public int DeleteMessageOrderID { get; set; }

		/// <summary>
		/// ID statusu zprávy
		/// (2 .. záznam byl odeslán a nebylo potvrzeno přijetí;  3 .. potvrzeno přijetí na straně ND a aktualizováno ve Fenixu)
		/// </summary>
		public int MessageStatusID { get; set; } 

		/// <summary>
		/// ctor
		/// </summary>
		private DeleteMessageOrderSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="receptionOrderID"></param>
		/// <param name="messageStatusID"></param>
		public DeleteMessageOrderSetSent(int receptionOrderID, int messageStatusID)
		{
			this.DeleteMessageOrderID = receptionOrderID;
			this.MessageStatusID = messageStatusID;
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
					
					db.prDeleteMessageOrderSetSent(this.DeleteMessageOrderID, this.MessageStatusID, retVal, retMsg);

					result = String.Format("{0} returnValue [{1}] returnMessage [{2}]", AppLog.GetMethodName(), retVal.Value.ToInt32(BC.NOT_OK), retMsg.Value.ToString(String.Empty));
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
