using System;
using System.Collections.Generic;
using System.Text;
using Fenix;
using Fenix.Common;
using Fenix.Xml;
using Fenix.XmlMessages;
using FenixAutomat.EmailCreator;
using FenixAutomat.Loggers;
using FenixAutomat.Message.Sender;
using FenixAutomat.Message.SetSent;
using FenixAutomat.Message.SetSent.SetSentSpecial;

namespace FenixAutomat.Message
{
	/// <summary>
	/// Vytvoří a odešle XML message pro
	/// - objednávky recepce                          (R0 message) 
	/// - objednávky kittingu                         (K0 message) 
	/// - potrzení kittingu                           (K2 message)
	/// - objednávky expedice/závozu                  (S0 message)
	/// - objednávky naskladnění repasovaného zboží	  (RF0 message)
	/// - požadavky na zrušení XML message            (D0 message)    {pozor - podléhá nastaveni v konfigu !!!}
	/// - CRM objednávky                              (C0 message)
	/// </summary>
	public class RKSMessage : BaseMessage
	{
		private string[] messageTypes = new string[] { "ReceptionOrder", "KittingOrder", "KittingApproval", "ShipmentOrder", "RefurbishedOrder", "DeleteMessage", "CrmOrder" };

		private enum MessageType
		{
			/// <summary>
			/// objednávka recepce (R0 message) 
			/// </summary>
			ReceptionOrder = 0,

			/// <summary>
			/// objednávka kittingu (K0 message)
			/// </summary>
			KittingOrder = 1,

			/// <summary>
			/// potrzení kittingu (K2 message)
			/// </summary>
			KittingApproval = 2,

			/// <summary>
			/// objednávka expedice/závozu (S0 message)
			/// </summary>
			ShipmentOrder = 3,

			/// <summary>
			/// objednávka naskladnění repasovaného zboží (RF0 message)
			/// </summary>
			RefurbishedOrder = 4,

			/// <summary>
			/// požadavek na zrušení XML message (D0 message)
			/// </summary>
			DeleteMessage = 5,

			/// <summary>
			/// CRM objednavka (C0 message)
			/// </summary>
			CrmOrder = 6
		}

		/// <summary>
		/// ctor
		/// </summary>
		public RKSMessage(int zicyzUserId)
			: base(zicyzUserId)
		{ }

		/// <summary>
		/// Vlastní zpracování
		/// </summary>
		/// <returns></returns>
		public ResultAppService CreateAndSendMessage()
		{
			processReceptionOrder();
			processKittingOrder();
			processKittingApproval();
			processShipmentOrder();
			processRefurbishedOrder();

			if (BC.DeleteMessageViaXML)
			{
				processDeleteMessage();
			}

			processCrmOrder();
						
			return this.Result;
		}

		/// <summary>
		/// Zpracovani aktivních objednávek recepce (Reception Order R0 message)
		/// </summary>
		private void processReceptionOrder()
		{
			string returnedError = String.Empty;
			List<R0Reception> receptionOrderList = ListsCreator.CreateR0Reception(ref returnedError);
			
			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());				
				return;
			}
			
			this.Result.AddResultMessage(String.Format("Počet ReceptionOrder {0}", receptionOrderList.Count));
			
			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var receptionOrder in receptionOrderList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(receptionOrder, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("ReceptionOrder to ND  ID = [{0}]", receptionOrder.Header.ID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);
															
					ndResult = this.SendXmlMessageToND(xmlSourceString, messageTypes[(int)MessageType.ReceptionOrder]);
															
					ReceptionOrderSetSent receptionOrderSetSent = new ReceptionOrderSetSent(receptionOrder.Header.ID, ndResult.Result);					
					receptionOrderSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("R0 ReceptionOrder ID = [{0}] odesláno do ND.", receptionOrder.Header.ID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex)
			{				
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Zpracování aktivních objednávek kittingu (Kitting Order K0 message)
		/// </summary>
		private void processKittingOrder()
		{
			string returnedError = String.Empty;
			List<K0Kit> kitOrderList = ListsCreator.CreateK0KitOrder(ref returnedError);

			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());				
				return;
			}
						
			this.Result.AddResultMessage(String.Format("Počet KittingOrder {0}", kitOrderList.Count));
			
			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var kitOrder in kitOrderList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(kitOrder, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("KittingOrder to ND  ID = [{0}]", kitOrder.Header.ID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, messageTypes[(int)MessageType.KittingOrder]);
															
					KittingsOrderSetSent receptionOrderSetSent = new KittingsOrderSetSent(kitOrder.Header.ID, ndResult.Result);					
					receptionOrderSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("K0 KittingOrder ID = [{0}] odesláno do ND.", kitOrder.Header.ID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex)
			{				
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Zpracování aktivních potvrzení kittingu (Kitting Approval K2 message)
		/// </summary>
		private void processKittingApproval()
		{
			string returnedError = String.Empty;
			List<K2Kit> kitApprovalList = ListsCreator.CreateK2KitApproval(ref returnedError);

			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());				
				return;
			}
						
			this.Result.AddResultMessage(String.Format("Počet KittingApproval {0}", kitApprovalList.Count));
			
			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var kitApproval in kitApprovalList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(kitApproval, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("KittingApproval to ND  ID = [{0}]", kitApproval.Header.ID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, messageTypes[(int)MessageType.KittingApproval]);
					
					KittingsApprovalSetSent kittingsApprovalSetSent = new KittingsApprovalSetSent(kitApproval.Header.ID, ndResult.Result);					
					kittingsApprovalSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("K2 KittingApproval ID = [{0}] odesláno do ND.", kitApproval.Header.ID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex)
			{				
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Zpracování aktivních objednávek závozu/expedice (Shipment Order S0 message)
		/// </summary>
		private void processShipmentOrder()
		{
			string returnedError = String.Empty;
			List<S0Shipment> shipmentOrderList = ListsCreator.CreateS0ShipmentOrder(ref returnedError);

			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());				
				return;
			}
						
			this.Result.AddResultMessage(String.Format("Počet ShipmentOrder {0}", shipmentOrderList.Count));
			
			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var shipmentOrder in shipmentOrderList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(shipmentOrder, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("ShipmentOrder to ND  ID = [{0}]", shipmentOrder.Header.ID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, messageTypes[(int)MessageType.ShipmentOrder]);
					
					ShipmentOrderSetSent shipmentOrderSetSent = new ShipmentOrderSetSent(shipmentOrder.Header.ID, ndResult.Result);
					shipmentOrderSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("S0 ShipmentOrder ID = [{0}] odesláno do ND.", shipmentOrder.Header.ID), ndResult, ApplicationLog.GetMethodName());

					//vytvoření a odeslání upozorňovacího emailu
					ShipmentOrderNotification shipmentOrderNotification = new ShipmentOrderNotification(shipmentOrder, ndResult);
					shipmentOrderNotification.CreateAndSendEmail();
				}
			}
			catch (Exception ex)
			{				
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Zpracování aktivních objednávek naskladnění repasovaného zboží (Refurbished Order RF0 message)
		/// </summary>
		private void processRefurbishedOrder()
		{
			string returnedError = String.Empty;						
			List<RF0Refurbished> refurbishedOrderList = ListsCreator.CreateRF0RefurbishedOrder(ref returnedError);

			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());				
				return;
			}
						
			this.Result.AddResultMessage(String.Format("Počet RefurbishedOrder {0}", refurbishedOrderList.Count));
			
			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var refurbishedOrder in refurbishedOrderList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(refurbishedOrder, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("RefurbishedOrder to ND  ID = [{0}]", refurbishedOrder.Header.ID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, messageTypes[(int)MessageType.RefurbishedOrder]);
					
					RefurbishedOrderSetSent refurbishedOrderSetSent = new RefurbishedOrderSetSent(refurbishedOrder.Header.ID, ndResult.Result);
					refurbishedOrderSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("RF0 RefurbishedOrder ID = [{0}] odesláno do ND.", refurbishedOrder.Header.ID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex)
			{				
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Zpracování aktivních požadavků na zrušení XML message (DeleteMessage D0 message)
		/// </summary>
		private void processDeleteMessage()
		{
			string returnedError = String.Empty;
			List<D0Delete> deleteMessageList = ListsCreator.CreateD0DeleteMessage(ref returnedError);

			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());
				return;
			}

			this.Result.AddResultMessage(String.Format("Počet DeleteMessage {0}", deleteMessageList.Count));

			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var deleteMessage in deleteMessageList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(deleteMessage, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("DeleteMessage to ND  ID = [{0}]", deleteMessage.Header.ID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, messageTypes[(int)MessageType.DeleteMessage]);

					DeleteMessageSetSent deleteMessageSetSent = new DeleteMessageSetSent(deleteMessage.Header.MessageID, ndResult.Result, this.ZicyzUserID);
					deleteMessageSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("D0 DeleteMessage ID = [{0}] odesláno do ND.", deleteMessage.Header.ID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Zpracovani aktivních CRM objednávek (Crm Order C0 message)
		/// </summary>
		private void processCrmOrder()
		{
			string returnedError = String.Empty;
			List<C0CrmOrder> crmOrderList = ListsCreator.CreateC0CrmOrder(ref returnedError);
			
			if (returnedError != String.Empty)
			{
				this.ProcessListsCreatorError(returnedError, ApplicationLog.GetMethodName());
				return;
			}

			this.Result.AddResultMessage(String.Format("Počet CrmOrder {0}", crmOrderList.Count));

			try
			{
				string xmlSourceString;
				ReturnedValueFromND ndResult;
				foreach (var crmOrder in crmOrderList)
				{
					xmlSourceString = XmlCreator.CreateXmlString(crmOrder, BC.URL_W3_ORG_SCHEMA, Encoding.UTF8, CreatorSettings.Declaration);
					Logger.PrepareAndWriteXmlMessageToLogs(String.Format("CrmOrder to ND  ID = [{0}]", crmOrder.Header.ID), xmlSourceString, ApplicationLog.GetMethodName(), this.ZicyzUserID);

					ndResult = this.SendXmlMessageToND(xmlSourceString, messageTypes[(int)MessageType.CrmOrder]);

					CrmOrderSetSentAndModifyCardStockItem crmOrderSetSentAndModifyCardStockItem = new CrmOrderSetSentAndModifyCardStockItem(crmOrder.Header.ID, BC.WRITE_TO_ND_OK, ndResult.Result);
					crmOrderSetSentAndModifyCardStockItem.SetSentAndModifyCardStockItems();

					//crmOrderSetSentAndModifyCardStockItem.set
					//ReceptionOrderSetSent receptionOrderSetSent = new ReceptionOrderSetSent(crmOrder.Header.ID, ndResult.Result);
					//receptionOrderSetSent.SetSent();

					this.ActionsAfterSendingXmlMessageToND(String.Format("C0 CrmOrder ID = [{0}] odesláno do ND.", crmOrder.Header.ID), ndResult, ApplicationLog.GetMethodName());
				}
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}
	}
}
