using System;
using System.Data.Entity;
using System.Linq;
using FenixAutomat.EmailReceiver;
using FenixAutomat.EmailSender;
using FenixAutomat.Loggers;
using FenixHelper;
using FenixHelper.Common;
using Microsoft.Exchange.WebServices.Data;
using UPC.Extensions.Convert;

namespace FenixAutomat.DeleteMessageEmail
{
	/// <summary>
	/// Přečtení a zpracování doručeného emailu (odesílatel je ND/XPO)
	/// - poštovní schránka: fenix@upc.cz
	/// - požadovaný tvar  : UPC_CZ Fenix DeleteMessage MessageDescription={0} ID={1} MessageID={2}
	/// </summary>
	internal class D0DeleteMessageEmailReceive
	{
		/// <summary>
		/// zicyz ID aktuálního uživatele
		/// </summary>
		private int zicyzUserId;

	    private const int ReceptionOrder = 0;

		private const int KittingOrder = 1;

		private const int ShipmentOrder = 2;

		private const int RefurbishedOrder = 3;

	    public string[] MessageTypes = new string[] { "ReceptionOrder", "KittingOrder", "ShipmentOrder", "RefurbishedOrder" };

	    private const int MessageStatusReceived = 12;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="zicyzUserId"></param>
		public D0DeleteMessageEmailReceive(int zicyzUserId)
		{
			this.zicyzUserId = zicyzUserId;		
		}

	    public int Id { get; set; }

	    public int MessageId { get; set; }

	    public string MessageType { get; set; }

	    public string EmailSubject { get; set; }

	    internal int ProcessedEmailCount { get; set; }

	    /// <summary>
		/// Přečtení a zpracování doručeného emailu z ND(XPO)
		/// </summary>
		/// <returns></returns>
		public ResultAppService ReceiveAndProcessEmails()
		{
			ResultAppService result = new ResultAppService();

			try
			{
				D0DeleteEmailReceive d0DeleteEmailReceive = new D0DeleteEmailReceive(BC.ExchangeURL, BC.Domain, BC.DomainUserLogin, BC.DomainUserPassword);
				d0DeleteEmailReceive.CreateExchangeService();

				int unreadEmailCount = d0DeleteEmailReceive.GetUnreadMailsCountInFolder(WellKnownFolderName.Inbox);

				result.AddResultMessage(String.Format("Počet všech DeleteMessage Emailů ke zpracování {0}", unreadEmailCount));

				if (unreadEmailCount > 0)
				{
					FindItemsResults<Item> items = d0DeleteEmailReceive.GetUndreadItemsInFolder(WellKnownFolderName.Inbox, unreadEmailCount);
					foreach (EmailMessage message in items.Items)
					{
						if ((this.ParseEmailSubject(message.Subject)) && (message.IsRead == false))
						{
							if (this.EmailBySubjectAlreadyProcessed(message.Subject, message.From.Name) == false)
							{
								if (this.Process())
								{
									message.IsRead = true;
									message.Update(ConflictResolutionMode.AlwaysOverwrite);
									message.Delete(DeleteMode.MoveToDeletedItems);
									this.SendInfoEmail(AppLog.GetMethodName());
									this.ProcessedEmailCount++;
								}
							}
							else
							{
								message.IsRead = true;
								message.Update(ConflictResolutionMode.AlwaysOverwrite);
								message.Delete(DeleteMode.MoveToDeletedItems);
							}
						}
					}
				}
				result.AddResultMessage(String.Format("Počet DeleteMessage Emailů skutečně zpracovaných {0}", this.ProcessedEmailCount));
			}
			catch (Exception e)
			{
				Logger.ProcessError(result, e, AppLog.GetMethodName(), BC.ServiceUserId);
			}

			return result;
		}

	    /// <summary>
	    /// Podle parsovaneho subjektu emailu rozhodne, zda je email již zpracovaný
	    /// (pokud není, je email uložen do databáze)
	    /// </summary>
	    /// <param name="emailSubject"></param>
	    /// <param name="emailFrom"></param>
	    /// <returns></returns>
	    private bool EmailBySubjectAlreadyProcessed(string emailSubject, string emailFrom)
		{
			bool result = false;

			string parsedSubject = string.Format("UPC_CZ Fenix DeleteMessage MessageDescription={0} ID={1} MessageID={2}", this.MessageType, this.Id, this.MessageId);
			string parsedSubjectHash = BC.CreateSHA256Hash(parsedSubject);

			using (var db = new FenixEntities())
			{
				try
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;

					var receivedEmail = (from d in db.EmailReceived
										 where d.IsActive == true && d.EmailParsedSubjectHash == parsedSubjectHash
										 select d).FirstOrDefault();
					if (receivedEmail == null)
					{
						EmailReceived emr = new EmailReceived();
						emr.Type = BC.APP_NAMESPACE;
						emr.EmailSubject = emailSubject;
						emr.EmailParsedSubject = parsedSubject;
						emr.EmailParsedSubjectHash = parsedSubjectHash;
						emr.EmailMessage = string.Empty;
						emr.EmailFrom = emailFrom;
						emr.Source = AppLog.GetMethodName();
						emr.IsActive = true;
						emr.IsExternal = true;
						emr.ModifyUserId = this.zicyzUserId;
						emr.ModifyDate = DateTime.Now;
						db.EmailReceived.Add(emr);
						db.SaveChanges();
					}
					else
					{
						result = true;
					}
				}
				catch 
				{
					//foreach (var validationResults in db.GetValidationErrors())
					//{
					//	foreach (var error in validationResults.ValidationErrors)
					//	{
					//		Console.WriteLine(
					//						  "Entity Property: {0}, Error {1}",
					//						  error.PropertyName,
					//						  error.ErrorMessage);
					//	}
					//}
					throw;
				}
			}

			return result;
		}

		/// <summary>
		/// Nastavení statusu, inaktivity  
		/// pro kitting a shipment se nastavuje vrácené množství na skladové kartě a vytváří se interní doklad
		/// </summary>
		/// <returns></returns>
		private bool Process()
		{
			bool result = false;

			using (var db = new FenixEntities())
			{
				using (var tr = db.Database.BeginTransaction())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						this.EmailSetReceived(db);	
					
						if (this.MessageType.ToUpper() == MessageTypes[KittingOrder].ToUpper())
						{							
							this.KittingReturnQuantity(db);
						}
						if (this.MessageType.ToUpper() == MessageTypes[ShipmentOrder].ToUpper())
						{							
							this.ShipmentReturnQuantity(db);
						}

						this.OrderSetInactive(db);
						
						tr.Commit();
						result = true;
					}
					catch (Exception ex)
					{
						tr.Rollback();
						Logger.ProcessError(ex, AppLog.GetMethodName(), BC.ServiceUserId);
					}
				}
			}

			return result;
		}

        /// <summary>
        /// Shipment - vrácené množství
        /// item : Free = Free + itemQuantity 
        /// kit  : ReleasedForExpedition = ReleasedForExpedition + itemQuantity
        /// Reserved = Reserved - itemQuantity
        /// vytváří interní doklad
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
        private void ShipmentReturnQuantity(FenixEntities db)
		{
			var shipmentOrderItems = from so in db.CommunicationMessagesShipmentOrdersSentItems
									where so.IsActive == true && so.CMSOId == this.Id
									select so;

			foreach (var shipmentOrderItem in shipmentOrderItems)
			{
				var cardStockItems = from c in db.CardStockItems
									 where c.IsActive == true && c.ItemOrKitID == shipmentOrderItem.ItemOrKitID &&
										   c.ItemOrKitQuality == shipmentOrderItem.ItemOrKitQualityId &&
									       c.ItemOrKitUnitOfMeasureId == shipmentOrderItem.ItemOrKitUnitOfMeasureId
									 select c;
				
				foreach (var cardStockItem in cardStockItems)
				{					
					InternalDocumentsHandler internalDocumentsHandler = new InternalDocumentsHandler(this.zicyzUserId, InternalDocumentsSource.FenixAutomatDeleteMessageEmail);

					CardStockItems cardStockStatusBefore = new CardStockItems();
					cardStockStatusBefore.ID = cardStockItem.ID;
					cardStockStatusBefore.ItemVerKit = cardStockItem.ItemVerKit;
					cardStockStatusBefore.ItemOrKitID = cardStockItem.ItemOrKitID;
					cardStockStatusBefore.ItemOrKitUnitOfMeasureId = cardStockItem.ItemOrKitUnitOfMeasureId;
					cardStockStatusBefore.ItemOrKitQuantity = cardStockItem.ItemOrKitQuantity;
					cardStockStatusBefore.ItemOrKitQuality = cardStockItem.ItemOrKitQuality;
					cardStockStatusBefore.ItemOrKitFree = cardStockItem.ItemOrKitFree;
					cardStockStatusBefore.ItemOrKitUnConsilliation = cardStockItem.ItemOrKitUnConsilliation;
					cardStockStatusBefore.ItemOrKitReserved = cardStockItem.ItemOrKitReserved;
					cardStockStatusBefore.ItemOrKitReleasedForExpedition = cardStockItem.ItemOrKitReleasedForExpedition;
					cardStockStatusBefore.ItemOrKitExpedited = cardStockItem.ItemOrKitExpedited;
					cardStockStatusBefore.StockId = cardStockItem.StockId;
					cardStockStatusBefore.IsActive = cardStockItem.IsActive;
					cardStockStatusBefore.ModifyDate = cardStockItem.ModifyDate;
					cardStockStatusBefore.ModifyUserId = cardStockItem.ModifyUserId;

					if (shipmentOrderItem.ItemVerKit == 0)
					{	
						decimal? itemOrKitFree = cardStockItem.ItemOrKitFree;
						if (itemOrKitFree.HasValue == false)
							itemOrKitFree = 0M;
						decimal? itemOrKitReserved = cardStockItem.ItemOrKitReserved;
						if (itemOrKitReserved.HasValue == false)
							itemOrKitReserved = 0M;
												
						cardStockItem.ItemOrKitFree = (decimal)itemOrKitFree + shipmentOrderItem.ItemOrKitQuantity;
						cardStockItem.ItemOrKitReserved = (decimal)itemOrKitReserved - shipmentOrderItem.ItemOrKitQuantity;
						cardStockItem.ModifyDate = DateTime.Now;
						cardStockItem.ModifyUserId = this.zicyzUserId;						
					}
					else
					{
						decimal? itemOrKitReleasedForExpedition = cardStockItem.ItemOrKitReleasedForExpedition;
						if (itemOrKitReleasedForExpedition.HasValue == false)
							itemOrKitReleasedForExpedition = 0M;
						decimal? itemOrKitReserved = cardStockItem.ItemOrKitReserved;
						if (itemOrKitReserved.HasValue == false)
							itemOrKitReserved = 0M;

						cardStockItem.ItemOrKitReleasedForExpedition = (decimal)itemOrKitReleasedForExpedition + shipmentOrderItem.ItemOrKitQuantity;
						cardStockItem.ItemOrKitReserved = (decimal)itemOrKitReserved - shipmentOrderItem.ItemOrKitQuantity;
						cardStockItem.ModifyDate = DateTime.Now;
						cardStockItem.ModifyUserId = this.zicyzUserId;
					}
					db.SaveChanges();

					CardStockItems cardStockStatusAfter = internalDocumentsHandler.GetCardStock(db, cardStockItem.ItemOrKitID);
					internalDocumentsHandler.CreateInternalDocuments(db, cardStockStatusBefore, cardStockStatusAfter);
					db.SaveChanges();
				}
			}
		}

        /// <summary>
        /// Kitting - vrácené množství
        /// Free     = Free     + (kiQuantity * itemQuantity)
        /// Reserved = Reserved - (kiQuantity * itemQuantity)
        /// vytváří interní doklad
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
        private void KittingReturnQuantity(FenixEntities db)
		{
			var kitSentItem = (from d in db.CommunicationMessagesKittingsSentItems
							   where d.CMSOId == this.Id
							   select d).FirstOrDefault();
			if (kitSentItem == null) return;

			var v = from p in db.cdlKitsItems
					where p.IsActive == true && p.cdlKitsId == kitSentItem.KitId
					select p;

			foreach (var item in v)
			{
				decimal mySuma = kitSentItem.KitQuantity * item.ItemOrKitQuantity;

				var cardStock = (from cs in db.CardStockItems
								where cs.StockId == kitSentItem.CardStockItemsId && cs.ItemOrKitID == item.ItemOrKitId && 
									  cs.ItemVerKit == item.ItemVerKit && cs.ItemOrKitQuality == kitSentItem.KitQualityId
								select cs).FirstOrDefault();

				if (cardStock != null)
				{					
					InternalDocumentsHandler internalDocumentsHandler = new InternalDocumentsHandler(this.zicyzUserId, InternalDocumentsSource.FenixAutomatDeleteMessageEmail);
															
					//CardStockItems cardStockStatusBefore = internalDocument.GetCardStock(db, cardStock.ItemOrKitID);
					
					CardStockItems cardStockStatusBefore = new CardStockItems();        
					cardStockStatusBefore.ID = cardStock.ID;
					cardStockStatusBefore.ItemVerKit = cardStock.ItemVerKit;
					cardStockStatusBefore.ItemOrKitID = cardStock.ItemOrKitID;
					cardStockStatusBefore.ItemOrKitUnitOfMeasureId = cardStock.ItemOrKitUnitOfMeasureId;
					cardStockStatusBefore.ItemOrKitQuantity = cardStock.ItemOrKitQuantity;
					cardStockStatusBefore.ItemOrKitQuality = cardStock.ItemOrKitQuality;
					cardStockStatusBefore.ItemOrKitFree = cardStock.ItemOrKitFree;
					cardStockStatusBefore.ItemOrKitUnConsilliation = cardStock.ItemOrKitUnConsilliation;
					cardStockStatusBefore.ItemOrKitReserved = cardStock.ItemOrKitReserved;
					cardStockStatusBefore.ItemOrKitReleasedForExpedition = cardStock.ItemOrKitReleasedForExpedition;
					cardStockStatusBefore.ItemOrKitExpedited = cardStock.ItemOrKitExpedited;
					cardStockStatusBefore.StockId = cardStock.StockId;
					cardStockStatusBefore.IsActive = cardStock.IsActive;
					cardStockStatusBefore.ModifyDate = cardStock.ModifyDate;
					cardStockStatusBefore.ModifyUserId = cardStock.ModifyUserId;

					decimal? itemOrKitFree = cardStock.ItemOrKitFree;
					if (itemOrKitFree.HasValue == false)
						itemOrKitFree = 0M;
					decimal? itemOrKitReserved = cardStock.ItemOrKitReserved;
					if (itemOrKitReserved.HasValue == false)
						itemOrKitReserved = 0M;

					cardStock.ItemOrKitFree = itemOrKitFree.Value + mySuma;
					cardStock.ItemOrKitReserved = itemOrKitReserved.Value - mySuma;
					cardStock.ModifyDate = DateTime.Now;
					cardStock.ModifyUserId = this.zicyzUserId;
					db.SaveChanges();

					CardStockItems cardStockStatusAfter = internalDocumentsHandler.GetCardStock(db, cardStock.ItemOrKitID);
					internalDocumentsHandler.CreateInternalDocuments(db, cardStockStatusBefore, cardStockStatusAfter);
					db.SaveChanges();
				}
			}			
		}

        /// <summary>
        /// Nastaveni MessageStatusId na 12 = "příchozí email úspěšně zpracován, záznam úspěšně aktualizován ve Fenixu" 
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
        private void EmailSetReceived(FenixEntities db)
		{
			var deleteMessageSent = (from dmSent in db.DeleteMessageSent
									 where dmSent.DeleteId == this.Id && dmSent.DeleteMessageId == this.MessageId
									 select dmSent).FirstOrDefault();

			if (deleteMessageSent == null) return;

			deleteMessageSent.MessageStatusId = MessageStatusReceived;
			deleteMessageSent.ReceivedDate = DateTime.Now;
			deleteMessageSent.ReceivedUserId = this.zicyzUserId;
			db.SaveChanges();
		}

        /// <summary>
        /// Nastavení inaktivity (IsActive = false)
        /// (hlavička i položky/itemy)
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
        private void OrderSetInactive(FenixEntities db)
		{
			if (this.MessageType.ToUpper() == MessageTypes[ReceptionOrder].ToUpper())
			{
				this.SetReceptionOrderInactive(db);
			}

			if (this.MessageType.ToUpper() == MessageTypes[KittingOrder].ToUpper())
			{
				this.SetKittingOrderInactive(db);
			}

			if (this.MessageType.ToUpper() == MessageTypes[ShipmentOrder].ToUpper())
			{
				this.SetShipmentOrderInactive(db);
			}

			if (this.MessageType.ToUpper() == MessageTypes[RefurbishedOrder].ToUpper())
			{
				this.SetRefurbishedOrderInactive(db);
			}			
		}

        /// <summary>
        /// Nastavuje vlastnost <seealso cref="CommunicationMessagesReceptionSent.IsActive"/> třídy
        /// <seealso cref="CommunicationMessagesReceptionSent"/> na false
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
		private void SetReceptionOrderInactive(FenixEntities db)
		{
			var inactiveOrder = (from recSent in db.CommunicationMessagesReceptionSent
								 where recSent.ID == this.Id && recSent.MessageId == this.MessageId
								 select recSent).FirstOrDefault();
			if (inactiveOrder == null) return;

			inactiveOrder.IsActive = false;
			db.SaveChanges();

			var inactiveItems = from recSentItems in db.CommunicationMessagesReceptionSentItems
								where recSentItems.CMSOId == this.Id
								select recSentItems;
			foreach (var inactiveItem in inactiveItems)
			{
				inactiveItem.IsActive = false;
				db.SaveChanges();
			}
		}

        /// <summary>
        /// Nastavuje vlastnost <seealso cref="CommunicationMessagesKittingsSent.IsActive"/> třídy
        /// <seealso cref="CommunicationMessagesKittingsSent"/> na false
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
		private void SetKittingOrderInactive(FenixEntities db)
		{
			var inactiveOrder = (from kitSent in db.CommunicationMessagesKittingsSent
								 where kitSent.ID == this.Id && kitSent.MessageId == this.MessageId
								 select kitSent).FirstOrDefault();
			if (inactiveOrder == null) return;

			inactiveOrder.IsActive = false;
			db.SaveChanges();

			var inactiveItems = from kitSentItems in db.CommunicationMessagesKittingsSentItems
								where kitSentItems.CMSOId == this.Id
								select kitSentItems;
			foreach (var inactiveItem in inactiveItems)
			{
				inactiveItem.IsActive = false;
				db.SaveChanges();
			}
		}

        /// <summary>
        /// Nastavuje vlastnost <seealso cref="CommunicationMessagesShipmentOrdersSent.IsActive"/> třídy
        /// <seealso cref="CommunicationMessagesShipmentOrdersSent"/> s aktuálním <seealso cref="CommunicationMessagesShipmentOrdersSent.ID"/>
        /// na false
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
        private void SetShipmentOrderInactive(FenixEntities db)
		{
			var inactiveOrder = (from shipSent in db.CommunicationMessagesShipmentOrdersSent
								 where shipSent.ID == this.Id && shipSent.MessageId == this.MessageId
								 select shipSent).FirstOrDefault();
			if (inactiveOrder == null) return;

			inactiveOrder.IsActive = false;
			db.SaveChanges();

			var inactiveItems = from shipSentItems in db.CommunicationMessagesShipmentOrdersSentItems
								where shipSentItems.CMSOId == this.Id
								select shipSentItems;
			foreach (var inactiveItem in inactiveItems)
			{
				inactiveItem.IsActive = false;
				db.SaveChanges();
			}
		}

        /// <summary>
        /// Nastavuje vlastnost <seealso cref="CommunicationMessagesRefurbishedOrder.IsActive"/> třídy
        /// <seealso cref="CommunicationMessagesRefurbishedOrder"/> na false
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
		private void SetRefurbishedOrderInactive(FenixEntities db)
		{
			var inactiveOrder = (from refOrder in db.CommunicationMessagesRefurbishedOrder
								 where refOrder.ID == this.Id && refOrder.MessageId == this.MessageId
								 select refOrder).FirstOrDefault();
			if (inactiveOrder == null) return;

			inactiveOrder.IsActive = false;
			db.SaveChanges();

			var inactiveItems = from refOrderItems in db.CommunicationMessagesRefurbishedOrderItems
								where refOrderItems.CMSOId == this.Id
								select refOrderItems;
			foreach (var inactiveItem in inactiveItems)
			{
				inactiveItem.IsActive = false;
				db.SaveChanges();
			}
		}

		/// <summary>
		/// Parsing předmětu emailu
		/// požadován tvar: UPC_CZ Fenix DeleteMessage MessageDescription={0} ID={1} MessageID={2}
		/// </summary>
		/// <param name="emailSubject">Předmět emailu</param>
		/// <returns></returns>
		private bool ParseEmailSubject(string emailSubject)
		{
			bool parseSubject = false;

			this.EmailSubject = emailSubject;
			
			if (this.EmailSubject.IndexOf("UPC_CZ Fenix DeleteMessage MessageDescription=") >= 0)
			{
				string[] base2 = this.EmailSubject.Split(' ');				
				foreach (string item in base2)
				{					
					if (item.StartsWith("MessageDescription="))
					{
						this.MessageType = item.Substring(item.IndexOf("=") + 1);
					}
					if (item.StartsWith("ID="))
					{
						this.Id = ConvertExtensions.ToInt32(item.Substring(item.IndexOf("=") + 1), 0);
					}
					if (item.StartsWith("MessageID="))
					{
						this.MessageId = ConvertExtensions.ToInt32(item.Substring(item.IndexOf("=") + 1), 0);
					}
				}

				if ((Array.IndexOf(this.MessageTypes, this.MessageType) >= 0) && this.Id > 0 && this.MessageId > 0)
				{
					parseSubject = true;					
				}
			}
			
			return parseSubject;
		}

		/// <summary>
		/// Odešle email s informací o přijetí DeleteMessageEmail (D0Message)
		/// <para>(tento email se odešle právě 1x)</para>
		/// </summary>
		/// <param name="methodName">Název metody</param>
		private void SendInfoEmail(string methodName)
		{
			string message = string.Format("D0  DeleteMessageEmail  MessageDescription = [{0}]  ID = [{1}]  MessageID = [{2}]  přijato z ND/XPO.", 
				                           this.MessageType, this.Id, this.MessageId);

			Email infoEmail = new Email()
			{
				Type = BC.APP_NAMESPACE,
				Subject = String.Format("{0} - Databáze: {1}", "Fenix Automat", BC.GetDatabaseName()),
				Body = message,
				MailTo = BC.MailTo,
				SendOnlyOnce = true,
				Source = methodName
			};

			infoEmail.SendMail();
		}
	}
}
