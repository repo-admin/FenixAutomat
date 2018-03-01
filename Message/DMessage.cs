using System;
using System.Collections.Generic;
using System.Text;
using FenixAutomat.Loggers;
using FenixAutomat.Message.Sender;
using FenixAutomat.Message.SetSent;
using FenixHelper;
using FenixHelper.Common;
using FenixHelper.XMLMessage;

namespace FenixAutomat.Message
{
	/// <summary>
	/// Vytvoří a odešle XML message s požadavkem na zrušení XML message na straně ND 
	/// </summary>
	public class DMessage : BaseMessage
	{
		/// <summary>
		/// ctor
		/// </summary>
		public DMessage(int zicyzUserId)
			: base(zicyzUserId) 
		{ }

		/// <summary>
		/// vlastní zpracování
		/// </summary>
		/// <returns></returns>
		public ResultAppService CreateAndSendMessage()
		{
			processDeleteOrder();			
			return base.Result;
		}

		/// <summary>
		/// zpracování požadavků na vymazání XML message na straně ND
		/// </summary>
		private void processDeleteOrder()
		{
			string returnedError = String.Empty;
			string methodName = AppLog.GetMethodName();

			List<D0Delete> deleteMessageOrderList = ListsCreator.CreateD0DeleteMessageOrder(ref returnedError);

			if (returnedError.IsNullOrEmpty())
			{
				this.Result.AddResultMessage(String.Format("Počet DeleteMessageOrder {0}", deleteMessageOrderList.Count));

				try
				{
					string xmlSourceString;
					ReturnedValueFromND ndResult;
					foreach (var deleteMessageOrder in deleteMessageOrderList)
					{
						xmlSourceString = XmlCreator.CreateXmlString(deleteMessageOrder, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
						Logger.PrepareAndWriteXmlMessageToLogs(String.Format("DeleteMessageOrder to ND  ID = [{0}]", deleteMessageOrder.Header.ID), xmlSourceString, methodName, this.ZicyzUserID);

						ndResult = base.SendXmlMessageToND(xmlSourceString, "DeleteMessageOrder");

						DeleteMessageOrderSetSent deleteMessageOrderSetSent = new DeleteMessageOrderSetSent(deleteMessageOrder.Header.ID, ndResult.Result);
						deleteMessageOrderSetSent.SetSent();

						base.ActionsAfterSendingXmlMessageToND(String.Format("DeleteMessageOrder ID = [{0}] odesláno do ND.", deleteMessageOrder.Header.ID), ndResult, methodName);
					}
				}
				catch (Exception ex)
				{
					Logger.ProcessError(ex, methodName, BC.ServiceUserId);
				}
			}
			else
			{
				base.ProcessListsCreatorError(returnedError, methodName);
			}
		}
	}
}
