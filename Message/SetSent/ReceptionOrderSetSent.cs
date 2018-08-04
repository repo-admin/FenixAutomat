using System;
using System.Data.Entity.Core.Objects;
using Fenix;
using FenixAutomat.Loggers;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastavení datumu odeslání a statusu zprávy pro objednávku recepce (Reception Order)
	/// </summary>
	public class ReceptionOrderSetSent : ISetSent
	{
		/// <summary>
		/// ID objednávky recepce
		/// </summary>
		public int ReceptionOrderID { get; set; }

		/// <summary>
		/// ID statusu zprávy
		/// </summary>
		public int MessageStatusID { get; set; } 

		/// <summary>
		/// ctor
		/// </summary>
		private ReceptionOrderSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="receptionOrderID"></param>
		/// <param name="messageStatusID"></param>
		public ReceptionOrderSetSent(int receptionOrderID, int messageStatusID)
		{
			this.ReceptionOrderID = receptionOrderID;
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

					db.prReceptionOrderSetSent(this.ReceptionOrderID, this.MessageStatusID, retVal, retMsg);

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
