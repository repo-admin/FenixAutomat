using System;
using System.Data.Entity.Core.Objects;
using Fenix;
using FenixAutomat.Loggers;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastavení datumu odeslání a typu zprávy pro repasi - objednávka naskladnění repasovaného zboží (RefurbishedOrder  RF0)
	/// </summary>
	public class RefurbishedOrderSetSent : ISetSent
	{
		/// <summary>
		/// ID objednávky naskladnění repasovaného zboží
		/// </summary>
		public int RefurbishedOrderID { get; set; }

		/// <summary>
		/// ID statusu zprávy
		/// </summary>
		public int MessageStatusID { get; set; } 

		/// <summary>
		/// ctor
		/// </summary>
		private RefurbishedOrderSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="refurbishedOrderID"></param>
		/// <param name="messageStatusID"></param>
		public RefurbishedOrderSetSent(int refurbishedOrderID, int messageStatusID)
		{
			this.RefurbishedOrderID = refurbishedOrderID;
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

					db.prRefurbishedOrderSetSent(this.RefurbishedOrderID, this.MessageStatusID, retVal, retMsg);

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
