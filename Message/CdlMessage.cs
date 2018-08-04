using System;
using System.Collections.Generic;
using System.Text;
using Fenix;
using Fenix.Common;
using Fenix.Xml;
using Fenix.XmlMessages;
using FenixAutomat.Loggers;
using FenixAutomat.Message.Sender;
using FenixAutomat.Message.SetSent;

namespace FenixAutomat.Message
{
	/// <summary>
	/// Vytvoří a odešle XML message pro Item a Item odpovídající kitu
	/// </summary>
	public class CdlMessage : BaseMessage
	{
		/// <summary>
		/// ctor
		/// </summary>
		public CdlMessage(int zicyzUserId)
			: base(zicyzUserId) 
		{ }

		/// <summary>
		/// vlastní zpracování
		/// </summary>
		/// <returns></returns>
		public ResultAppService CreateAndSendMessage()
		{
			processCdlItems();
			processCdlKits();
			
			return this.Result;
		}

		/// <summary>
		/// zpracovani Itemů (co nejsou kity)
		/// </summary>
		private void processCdlItems()
		{
			string returnedError = String.Empty;
			List<CDLItems> cdlItemsList = ListsCreator.CreateCDLItems(ref returnedError);

			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());
				return;
			}
						
			this.Result.AddResultMessage(String.Format("Počet CDLItems {0}", cdlItemsList.Count));
			
			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var cdlItem in cdlItemsList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(cdlItem, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("CdlItem to ND  ID = [{0}]", cdlItem.ItemIntegration.items[0].ItemID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, "Item");

					CdlItemsSetSent cdlItemsSetSent = new CdlItemsSetSent(cdlItem.ItemIntegration.items[0].ItemID, ndResult);
					cdlItemsSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("CdlItem ID = [{0}] odesláno do ND.", cdlItem.ItemIntegration.items[0].ItemID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex)
			{				
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// zpracování Kitů (jako speciální případ Itemu)
		/// </summary>
		private void processCdlKits()
		{
			string returnedError = String.Empty;
			List<CDLItemsForKit> cdlItemsForKit = ListsCreator.CreateCDLKits(ref returnedError);

			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());				
				return;
			}
						
			this.Result.AddResultMessage(String.Format("Počet CdlKits {0}", cdlItemsForKit.Count));
			
			try
			{				
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var cdlItemForKit in cdlItemsForKit)
				{
					xmlSourceString = XmlCreator.CreateXmlString(cdlItemForKit, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("CdlKit to ND  ID = [{0}]", cdlItemForKit.ItemIntegration.items[0].ItemID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, "Item");
					
					CdlKitsSetSent cdlKitsSetSent = new CdlKitsSetSent(cdlItemForKit.ItemIntegration.items[0].ItemID, ndResult);
					cdlKitsSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("CdlKit ID = [{0}] odesláno do ND.", cdlItemForKit.ItemIntegration.items[0].ItemID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex )
			{				
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}
	}
}
