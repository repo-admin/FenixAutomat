using System;
using System.Data.Entity.Core.Objects;
using FenixAutomat.Loggers;
using FenixAutomat.Message.Sender;
using FenixHelper;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastaví příznak odeslání Kitu
	/// </summary>
	public class CdlKitsSetSent : ISetSent
	{
		/// <summary>
		/// ID kitu
		/// </summary>
		public int KitID { get; set; }

		/// <summary>
		/// Vrácená hodnota z ND
		/// </summary>
		public ReturnedValueFromND ReturnedValueFromND { get; set; }

		/// <summary>
		/// ctor
		/// </summary>
		private CdlKitsSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="itemID"></param>
		public CdlKitsSetSent(int itemID, ReturnedValueFromND returnedValueFromND)
		{
			this.KitID = itemID;
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

			using (var db = new FenixEntities())
			{
				try
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));

					db.prCdlKitsSetSent(this.KitID, retVal, retMsg);

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
