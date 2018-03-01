using System;
using System.Data.Entity.Core.Objects;
using FenixAutomat.Loggers;
using FenixHelper;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastavení datumu odeslání a typu zprávy pro objednávku závozu/expedici (Shipment Order)
	/// </summary>
	public class ShipmentOrderSetSent : ISetSent
	{
		/// <summary>
		/// ID objednávky závozu/expedice
		/// </summary>
		public int ShipmentOrderID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int MessageStatusID { get; set; } 

		/// <summary>
		/// ctor
		/// </summary>
		private ShipmentOrderSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="shipmentOrderID"></param>
		/// <param name="messageStatusID"></param>
		public ShipmentOrderSetSent(int shipmentOrderID, int messageStatusID)
		{
			this.ShipmentOrderID = shipmentOrderID;
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

					db.prShipmentOrderSetSent(this.ShipmentOrderID, this.MessageStatusID, retVal, retMsg);
					
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
