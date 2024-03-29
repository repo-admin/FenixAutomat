﻿using System;
using System.Data.Entity.Core.Objects;
using Fenix;
using FenixAutomat.Loggers;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message.SetSent
{
	/// <summary>
	/// Nastavení datumu odeslání a typu zprávy pro objednávku kittingu (Kittings Order)
	/// </summary>
	public class KittingsOrderSetSent : ISetSent
	{
		/// <summary>
		/// ID objednávky kitingu
		/// </summary>
		public int KittingsOrderID { get; set; }

		/// <summary>
		/// ID statusu zprávy
		/// </summary>
		public int MessageStatusID { get; set; } 

		/// <summary>
		/// ctor
		/// </summary>
		private KittingsOrderSetSent()
		{ }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="receptionOrderID"></param>
		/// <param name="messageStatusID"></param>
		public KittingsOrderSetSent(int receptionOrderID, int messageStatusID)
		{
			this.KittingsOrderID = receptionOrderID;
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

					db.prKittingsOrderSetSent(this.KittingsOrderID, this.MessageStatusID, retVal, retMsg);

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
