using System;
using System.Data.Entity.Core.Objects;
using Fenix;
using FenixAutomat.Loggers;
using FenixAutomat.Message.Sender;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastaví příznak odeslání Itemu
	/// </summary>
	public class CdlItemsSetSent : ISetSent
	{
		/// <summary>
		/// ID itemu
		/// </summary>
		public int ItemID { get; set; }

		/// <summary>
		/// Vrácená hodnota z ND
		/// </summary>
		public ReturnedValueFromND ReturnedValueFromND { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		private CdlItemsSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="itemID"></param>
		public CdlItemsSetSent(int itemID, ReturnedValueFromND returnedValueFromND)
		{
			this.ItemID = itemID;
			this.ReturnedValueFromND = returnedValueFromND;
		}

		/// <summary>
		/// vlastní nastavení
		/// </summary>
		/// <returns></returns>
		public string SetSent()
		{
			string result = String.Empty;

			if (!this.ReturnedValueFromND.ReturnedValueIsOK)
			{
				return result;
			}

			using (var db = new FenixAutomat.FenixEntities())
			{
				try
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));

					db.prCdlItemsSetSent(this.ItemID, retVal, retMsg);

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
