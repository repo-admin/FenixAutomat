using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FenixAutomat.Message.SetSent;
using FenixHelper;
using FenixHelper.Common;
using FenixHelper.XMLMessage;
using FenixAutomat.Loggers;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent.SetSentSpecial
{
	/// <summary>
	/// nastaveni statusu aktualni zpravy
	/// zmeny na skladovych kartach
	/// (s ohledem na transakcni pozadavek nutno resit takto)
	/// </summary>
	public class CrmOrderSetSentAndModifyCardStockItem 
	{
		/// <summary>
		/// ID CRM objednavky
		/// </summary>
		public int CrmOrderID { get; set; }

		/// <summary>
		/// OK hodnota - do XPO se podarilo spravne zapsat
		/// </summary>
		public int OkWriteToXPO { get; set; }

		/// <summary>
		/// ID statusu zprávy
		/// </summary>
		public int MessageStatusID { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		private CrmOrderSetSentAndModifyCardStockItem()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="crmOrder">ID CRM objednavky</param>
		/// <param name="okWriteToXPO">hodnota reprezentujici OK zapis do XPO</param>
		/// <param name="messageStatusID">aktualni status po zapisu do XPO</param>
		public CrmOrderSetSentAndModifyCardStockItem(int crmOrderID, int okWriteToXPO, int messageStatusID)
		{
			this.CrmOrderID = crmOrderID;
			this.OkWriteToXPO = okWriteToXPO;
			this.MessageStatusID = messageStatusID;
		}

		/// <summary>
		/// 1. nastavení datume a priznaku odeslani
		/// 2. zmeny na skladovych kartach
		/// </summary>
		/// <returns></returns>
		public string SetSentAndModifyCardStockItems()
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

					db.prCrmOrderSetSentAndModifyCardStockItem(this.CrmOrderID, this.OkWriteToXPO, this.MessageStatusID, retVal, retMsg);
					//ALTER PROCEDURE [dbo].[prCrmOrderSetSentAndModifyCardStockItem]
					//(
					//	@CrmOrderID int,
					//	@StatusOK int,
					//	@MessageStatusID int,

					//	----@ItemVerKit bit,
					//	----@ItemOrKitID int,
					//	----@ItemOrKitQualityId int,
					//	----@ItemOrKitUnitOfMeasureId int,
					//	----@ItemOrKitQuantity numeric(18,3),
					//	----db.prReceptionOrderSetSent(this.CrmOrder.ID, this.okWriteToXPO, this.MessageStatusID, retVal, retMsg);
		
					//	@ReturnValue int = -1 OUTPUT,
					//	@ReturnMessage nvarchar(2048) = NULL OUTPUT
					//)

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
