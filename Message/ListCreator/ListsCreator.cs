using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Fenix;
using Fenix.XmlMessages;
using UPC.Extensions.Convert;

namespace FenixAutomat.Message
{
	/// <summary>
	/// Vytváří seznamy 
	/// do ND neodeslaných aktivních položek (items)
	/// do ND neodeslaných aktivních kitů
	/// do ND neodeslaných aktivních objednávek recepce(R0), objednávek kittingu(K0), potvrzení kittingu(K2), objednávek závozu(S0), objednávek repase(RF0)
	/// do ND meodeslaných aktivních požadavků na zrušení XML message (D0 message)
	/// do ND neodeslaných aktivních CRM objednávek (C0 message)
	/// </summary>
	public class ListsCreator
	{
		private const int MESSAGE_ID_ADDING = 1000000;
		
		/// <summary>
		/// vytvoří seznam aktivních items(položek) (Item), které nebyly odeslány do ND
		/// </summary>
		/// <returns></returns>
		public static List<CDLItems> CreateCDLItems(ref string returnMessage)
		{
			List<CDLItems> result = new List<CDLItems>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var blogs = from b in db.cdlItems
									orderby b.ID ascending
									where b.IsSent == false && b.IsActive == true
									select b;

						foreach (var item in blogs)
						{
							CDLItems cdlItems = new CDLItems();

							cdlItems.ItemIntegration.ID = CreateCdlID();
							cdlItems.ItemIntegration.MessageID = cdlItems.ItemIntegration.ID + MESSAGE_ID_ADDING;
							cdlItems.ItemIntegration.MessageType = "Item";
							cdlItems.ItemIntegration.MessageDescription = "ItemIntegration";
							cdlItems.ItemIntegration.MessageStatus = 1;

							cdlItems.ItemIntegration.items = new List<CdlItemsItem>();

							CdlItemsItem cdlItemsItem = new CdlItemsItem();

							cdlItemsItem.ItemID = item.ID;
							cdlItemsItem.ItemDescription = item.DescriptionCz;
							cdlItemsItem.SupplierID = "UPC";
							cdlItemsItem.SupplierName = "UPC";

							cdlItemsItem.ComponentManagement = 0;
							cdlItemsItem.QtyBox = 1;
							cdlItemsItem.QtyPallet = 1;

							cdlItemsItem.ItemTypeID = item.ItemTypesId;
							cdlItemsItem.ItemTypeCode = item.ItemType.Trim();
							cdlItemsItem.ItemTypeDesc1 = item.ItemTypeDesc1.Trim();
							cdlItemsItem.ItemTypeDesc2 = item.ItemTypeDesc2.Trim();

							cdlItemsItem.HeliosGroupGoods = item.GroupGoods;
							cdlItemsItem.HeliosCode = item.Code;

							cdlItems.ItemIntegration.items.Add(cdlItemsItem);

							result.Add(cdlItems);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
					}
				}
			}
			catch (Exception ex)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
			}

			return result;
		}

		/// <summary>
		/// vytvoří seznam aktivních kitů (speciální případ Item), které nebyly odeslány do ND
		/// </summary>
		/// <returns></returns>
		public static List<CDLItemsForKit> CreateCDLKits(ref string returnMessage)
		{
			List<CDLItemsForKit> result = new List<CDLItemsForKit>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						//var kits = from b in db.cdlKits
						//		   orderby b.ID ascending
						//		   where (b.IsSent == false || b.IsSent == null) && b.IsActive == true
						//		   select b;

                        // 22.06.2016 (DJ) -> removed null check on not-nullable boolean variable
                        var kits = from b in db.cdlKits
                                   orderby b.ID ascending
                                   where b.IsSent == false && b.IsActive == true
                                   select b;

                        foreach (var item in kits)
						{
							CDLItemsForKit cdlItems1 = new CDLItemsForKit();

							cdlItems1.ItemIntegration.ID = CreateCdlID();
							cdlItems1.ItemIntegration.MessageID = cdlItems1.ItemIntegration.ID + MESSAGE_ID_ADDING;
							cdlItems1.ItemIntegration.MessageType = "Item";
							cdlItems1.ItemIntegration.MessageDescription = "ItemIntegration";
							cdlItems1.ItemIntegration.MessageStatus = 1;

							cdlItems1.ItemIntegration.items = new List<CdlItemsForKitItem>();

							CdlItemsForKitItem cdlIKitsItem = new CdlItemsForKitItem();

							cdlIKitsItem.ItemID = item.ID;
							cdlIKitsItem.ItemDescription = item.DescriptionCz;
							cdlIKitsItem.SupplierID = "UPC";
							cdlIKitsItem.SupplierName = "UPC";

							cdlIKitsItem.ComponentManagement = 1;

							//Components
							cdlIKitsItem.components = new List<R1ComponentItem>();

							R1ComponentItem r1ComponentItem = new R1ComponentItem();

							var kitItems = from b in db.cdlKitsItems
										   orderby b.ID ascending
										   where b.cdlKitsId == item.ID
										   select b;

							foreach (var item2 in kitItems)
							{
								cdlIKitsItem.components.Add(new R1ComponentItem() { ComponentItemID = item2.ItemOrKitId, ComponentQty = item2.ItemOrKitQuantity });
							}

							cdlIKitsItem.QtyBox = 1;
							cdlIKitsItem.QtyPallet = 1;

							cdlItems1.ItemIntegration.items.Add(cdlIKitsItem);

							result.Add(cdlItems1);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
					}
				}
			}
			catch (Exception ex)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
			}

			return result;
		}

		/// <summary>
		/// vytvoří seznam aktivních objednávek recepce (Reception Order R0), které nebyly odeslány do ND
		/// </summary>
		/// <param name="returnMessage"></param>
		/// <returns></returns>
		public static List<R0Reception> CreateR0Reception(ref string returnMessage)
		{
			List<R0Reception> result = new List<R0Reception>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var receptionOrder = (from b in db.CommunicationMessagesReceptionSent
											  orderby b.ID ascending
											  where b.IsActive == true && (b.MessageStatusId == 1 || b.MessageStatusId == 2)
											  select b).Take(BC.NumRowsToSend);

						foreach (var item in receptionOrder)
						{
							R0Reception r0Reception = new R0Reception();

							r0Reception.Header.ID = item.ID;
							r0Reception.Header.MessageID = item.MessageId;
							r0Reception.Header.MessageTypeID = item.MessageType;
							r0Reception.Header.MessageTypeDescription = item.MessageDescription;
							r0Reception.Header.MessageDateOfShipment = DateTime.Now;
							r0Reception.Header.HeliosOrderID = item.HeliosOrderId;
							r0Reception.Header.ItemSupplierID = item.ItemSupplierId;
							r0Reception.Header.ItemSupplierDescription = item.ItemSupplierDescription;
							r0Reception.Header.ItemDateOfDelivery = item.ItemDateOfDelivery;

							r0Reception.Header.items = new List<R0Items>();

							var receptionOrderItems = from b in db.CommunicationMessagesReceptionSentItems
													  orderby b.ID ascending
													  where b.CMSOId == item.ID
													  select b;

							foreach (var r0ItemSql in receptionOrderItems)
							{
								R0Items r0Item = new R0Items();

								r0Item.HeliosOrderRecordID = r0ItemSql.HeliosOrderRecordId ?? 0;
								r0Item.ItemID = r0ItemSql.ItemID;
								r0Item.ItemDescription = r0ItemSql.ItemDescription;
								r0Item.ItemQuantity = r0ItemSql.ItemQuantity;
								r0Item.ItemUnitOfMeasureID = r0ItemSql.MeasuresID;
								r0Item.ItemUnitOfMeasure = r0ItemSql.ItemUnitOfMeasure.ToUpper();
								r0Item.ItemQualityID = r0ItemSql.ItemQualityId;
								r0Item.ItemQuality = r0ItemSql.ItemQualityCode;

								r0Reception.Header.items.Add(r0Item);
							}

							result.Add(r0Reception);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);						
					}
				}
			}
			catch (Exception e)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), e);				
			}

			return result;
		}

		/// <summary>
		/// vytvoří seznam aktivních objednávek kittingu (sestavení) (KitOrder K0), které nebyly odeslány do ND
		/// </summary>
		/// <returns></returns>
		public static List<K0Kit> CreateK0KitOrder(ref string returnMessage)
		{
			List<K0Kit> result = new List<K0Kit>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var kitOrders = (from b in db.CommunicationMessagesKittingsSent
										 orderby b.ID ascending
										 where b.IsActive == true && b.StockId == 2 && (b.MessageStatusId == 1 || b.MessageStatusId == 2)
										 select b).Take(BC.NumRowsToSend);

						foreach (var kitOrder in kitOrders)
						{
							K0Kit k0Kit = new K0Kit();

							k0Kit.Header.ID = kitOrder.ID;
							k0Kit.Header.MessageID = kitOrder.MessageId;
							k0Kit.Header.MessageTypeID = kitOrder.MessageType;
							k0Kit.Header.MessageTypeDescription = kitOrder.MessageDescription;
							k0Kit.Header.MessageDateOfShipment = DateTime.Now;
							k0Kit.Header.HeliosOrderID = kitOrder.HeliosOrderId;
							k0Kit.Header.KitDateOfDelivery = kitOrder.KitDateOfDelivery;

							var receptionOrderItems = from b in db.CommunicationMessagesKittingsSentItems
													  orderby b.ID ascending
													  where b.CMSOId == kitOrder.ID
													  select b;

							foreach (var r0ItemSql in receptionOrderItems)
							{

								k0Kit.Header.KitID = r0ItemSql.KitId;
								k0Kit.Header.KitDescription = r0ItemSql.KitDescription;
								k0Kit.Header.KitQuantity = r0ItemSql.KitQuantity;
								k0Kit.Header.KitUnitOfMeasureID = r0ItemSql.MeasuresID;
								k0Kit.Header.KitUnitOfMeasure = r0ItemSql.KitUnitOfMeasure.ToUpper();
								k0Kit.Header.KitQualityID = r0ItemSql.KitQualityId;
								k0Kit.Header.KitQuality = r0ItemSql.KitQualityCode;

								break;	// NO FUJ ... nicméně - ND si to vynutilo
							}

							result.Add(k0Kit);
							break;		// NO FUJ ... nicméně - ND si to vynutilo
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);						
					}
				}
			}
			catch (Exception e)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), e);				
			}

			return result;
		}

		/// <summary>
		/// vytvoří seznam aktivních potvrzení kittingu (sestavení) (KitApproval K2), které nebyly odeslány do ND
		/// </summary>
		/// <param name="returnMessage"></param>
		/// <returns></returns>
		public static List<K2Kit> CreateK2KitApproval(ref string returnMessage)
		{
			List<K2Kit> result = new List<K2Kit>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var kitApprovals = (from b in db.CommunicationMessagesKittingsApprovalSent
											orderby b.ID ascending
											where b.IsActive == true && b.Released == true && (b.MessageStatusID == 1 || b.MessageStatusID == 2)
											select b).Take(BC.NumRowsToSend);

						foreach (var kitApproval in kitApprovals)
						{
							K2Kit k2Kit = new K2Kit();

							k2Kit.Header.ID = kitApproval.ID;
							k2Kit.Header.MessageID = kitApproval.MessageId;
							k2Kit.Header.MessageTypeID = kitApproval.MessageTypeID;
							k2Kit.Header.MessageTypeDescription = kitApproval.MessageDescription;
							k2Kit.Header.MessageDateOfShipment = DateTime.Now;
							k2Kit.Header.RequiredReleaseDate = kitApproval.RequiredReleaseDate;
							k2Kit.Header.HeliosOrderID = kitApproval.HeliosOrderID ?? 0;

							var approvalKitsSent = from b in db.CommunicationMessagesKittingsApprovalKitsSent
												   orderby b.ID ascending
												   where b.ApprovalID == kitApproval.ID
												   select b;

							foreach (var approvalKitSent in approvalKitsSent)
							{
								k2Kit.Header.KitID = approvalKitSent.KitID;
								k2Kit.Header.KitDescription = approvalKitSent.KitDescription;
								k2Kit.Header.KitQuantity = approvalKitSent.KitQuantity;
								k2Kit.Header.KitUnitOfMeasureID = approvalKitSent.KitUnitOfMeasureID;
								k2Kit.Header.KitUnitOfMeasure = approvalKitSent.KitUnitOfMeasure.ToUpper();
								k2Kit.Header.KitQualityID = approvalKitSent.KitQualityId;
								k2Kit.Header.KitQuality = approvalKitSent.KitQuality;

								// seriova cisla
								var SNs = from b in db.CommunicationMessagesKittingsApprovalKitsSerNumSent
										  orderby b.ID ascending
										  where b.ApprovalKitsID == approvalKitSent.ID
										  select b;

								foreach (var sn in SNs)
								{
									k2Kit.Header.ItemSNs.Add(new K2ItemSN() { SerialNumber1 = (sn.SN1 ?? String.Empty).Trim(), SerialNumber2 = (sn.SN2 ?? String.Empty).Trim() });
								}

								break;	// NO FUJ ... nicméně - ND si to vynutilo
							}

							result.Add(k2Kit);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
					}
				}
			}
			catch (Exception e)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), e);				
			}

			return result;
		}

		/// <summary>
		/// vytvoří seznam aktivních objednávek závozu/expedice (ShipmentOrder S0), které nebyly odeslány do ND
		/// </summary>
		/// <returns></returns>
		public static List<S0Shipment> CreateS0ShipmentOrder(ref string returnMessage)
		{
			List<S0Shipment> result = new List<S0Shipment>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var shipmentOrders = (from b in db.CommunicationMessagesShipmentOrdersSent
											  orderby b.ID ascending
											  where b.IsActive == true && b.StockId == 2 && (b.MessageStatusId == 1 || b.MessageStatusId == 2)
											  select b).Take(BC.NumRowsToSend);

						foreach (var shipmentOrder in shipmentOrders)
						{
							S0Shipment s0Shipment = new S0Shipment();

							s0Shipment.Header.ID = shipmentOrder.ID;
							s0Shipment.Header.MessageID = shipmentOrder.MessageId;
							s0Shipment.Header.MessageTypeID = shipmentOrder.MessageTypeId;
							s0Shipment.Header.MessageTypeDescription = shipmentOrder.MessageDescription;
							s0Shipment.Header.MessageDateOfShipment = DateTime.Now;
							s0Shipment.Header.RequiredDateOfShipment = shipmentOrder.RequiredDateOfShipment;
							s0Shipment.Header.HeliosOrderID = shipmentOrder.HeliosOrderId;

							s0Shipment.Header.CustomerID = shipmentOrder.CustomerID;
							s0Shipment.Header.CustomerName = shipmentOrder.CustomerName;
							s0Shipment.Header.CustomerAddress1 = shipmentOrder.CustomerAddress1;
							s0Shipment.Header.CustomerAddress2 = shipmentOrder.CustomerAddress2;
							s0Shipment.Header.CustomerAddress3 = shipmentOrder.CustomerAddress3;
							s0Shipment.Header.CustomerCity = shipmentOrder.CustomerCity;
							s0Shipment.Header.CustomerZipCode = shipmentOrder.CustomerZipCode;
							s0Shipment.Header.CustomerCountryISO = shipmentOrder.CustomerCountryISO;

							s0Shipment.Header.ContactID = shipmentOrder.ContactID;
							s0Shipment.Header.ContactTitle = shipmentOrder.ContactTitle;
							s0Shipment.Header.ContactFirstName = shipmentOrder.ContactFirstName;
							s0Shipment.Header.ContactLastName = shipmentOrder.ContactLastName;
							s0Shipment.Header.ContactPhoneNumber1 = shipmentOrder.ContactPhoneNumber1;
							s0Shipment.Header.ContactPhoneNumber2 = shipmentOrder.ContactPhoneNumber2;
							s0Shipment.Header.ContactFaxNumber = shipmentOrder.ContactFaxNumber;
							s0Shipment.Header.ContactEmail = shipmentOrder.ContactEmail;

							var shipmentOrderItems = from b in db.CommunicationMessagesShipmentOrdersSentItems
													 orderby b.ID ascending
													 where b.CMSOId == shipmentOrder.ID && b.IsActive == true
													 select b;

							foreach (var s0ItemSql in shipmentOrderItems)
							{
								S0Items s0Item = new S0Items();

								s0Item.SingleOrMaster = s0ItemSql.SingleOrMaster;
								s0Item.HeliosOrderRecordID = s0ItemSql.HeliosOrderRecordId;
								s0Item.ItemOrKitID = s0ItemSql.ItemOrKitID;
								s0Item.ItemOrKitDescription = s0ItemSql.ItemOrKitDescription;
								s0Item.ItemOrKitQuantity = s0ItemSql.ItemOrKitQuantity;
								s0Item.ItemOrKitUnitOfMeasureID = s0ItemSql.ItemOrKitUnitOfMeasureId;
								s0Item.ItemOrKitUnitOfMeasure = s0ItemSql.ItemOrKitUnitOfMeasure;
								s0Item.ItemOrKitQualityID = s0ItemSql.ItemOrKitQualityId;
								s0Item.ItemOrKitQuality = s0ItemSql.ItemOrKitQualityCode;
								s0Item.ItemVerKit = s0ItemSql.ItemVerKit;
								s0Item.IncotermID = s0ItemSql.IncotermsId;
								s0Item.IncotermDescription = s0ItemSql.Incoterms;

								s0Shipment.Header.items.Add(s0Item);
							}

							result.Add(s0Shipment);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
					}
				}
			}
			catch (Exception e)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), e);
			}

			return result;
		}
				
		/// <summary>
		/// vytvoří seznam aktivních objednávek naskladnění repasovaného zboží (RefurbishedOrder  RF0), které nebyly odeslány do ND
		/// </summary>
		/// <param name="returnMessage"></param>
		/// <returns></returns>
		public static List<RF0Refurbished> CreateRF0RefurbishedOrder(ref string returnMessage)
		{
			List<RF0Refurbished> result = new List<RF0Refurbished>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var refurbishedOrders = (from b in db.CommunicationMessagesRefurbishedOrder
												 orderby b.ID ascending
												 where b.IsActive == true && b.StockId == 2 && (b.MessageStatusId == 1 || b.MessageStatusId == 2)
												 select b).Take(BC.NumRowsToSend);

						foreach (var refurbishedOrder in refurbishedOrders)
						{
							RF0Refurbished rf0Refurbished = new RF0Refurbished();

							rf0Refurbished.Header.ID = refurbishedOrder.ID;
							rf0Refurbished.Header.MessageID = refurbishedOrder.MessageId;
							rf0Refurbished.Header.MessageTypeID = refurbishedOrder.MessageTypeId;
							rf0Refurbished.Header.MessageTypeDescription = refurbishedOrder.MessageDescription;
							rf0Refurbished.Header.MessageDateOfShipment = DateTime.Now;
							rf0Refurbished.Header.CustomerID = refurbishedOrder.CustomerID;
							rf0Refurbished.Header.CustomerDescription = refurbishedOrder.CustomerDescription;
							rf0Refurbished.Header.DateOfDelivery = refurbishedOrder.DateOfDelivery;

							var refurbishedOrderItems = from b in db.CommunicationMessagesRefurbishedOrderItems
														orderby b.ID ascending
														where b.CMSOId == refurbishedOrder.ID && b.IsActive == true
														select b;

							foreach (var refurbishedOrderItem in refurbishedOrderItems)
							{
								RF0Items rf0Item = new RF0Items();

								rf0Item.ItemVerKit = refurbishedOrderItem.ItemVerKit;
								rf0Item.ItemOrKitID = refurbishedOrderItem.ItemOrKitID;
								rf0Item.ItemOrKitDescription = refurbishedOrderItem.ItemOrKitDescription;
								rf0Item.ItemOrKitQuantity = refurbishedOrderItem.ItemOrKitQuantity;
								rf0Item.ItemOrKitUnitOfMeasureID = refurbishedOrderItem.ItemOrKitUnitOfMeasureId;
								rf0Item.ItemOrKitUnitOfMeasure = refurbishedOrderItem.ItemOrKitUnitOfMeasure;
								rf0Item.ItemOrKitQualityID = refurbishedOrderItem.ItemOrKitQualityId;
								rf0Item.ItemOrKitQuality = refurbishedOrderItem.ItemOrKitQualityCode;

								rf0Refurbished.Header.items.Add(rf0Item);
							}

							result.Add(rf0Refurbished);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);						
					}
				}
			}
			catch (Exception e)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), e);				
			}

			return result;
		}

		/// <summary>
		/// Vytvoří seznam aktivních požadavků na zrušení XML message (DeleteMessage  D0)		
		/// </summary>
		/// <param name="returnMessage"></param>
		/// <returns></returns>
		public static List<D0Delete> CreateD0DeleteMessage(ref string returnMessage)
		{
			List<D0Delete> result = new List<D0Delete>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var deleteMessageOrders = from b in db.CommunicationMessagesDeleteMessage
												  orderby b.ID ascending
												  where b.IsActive == true && (b.MessageStatusId == 1 || b.MessageStatusId == 2)
												  select b;

						foreach (var deleteMessageOrder in deleteMessageOrders)
						{
							D0Delete d0Delete = new D0Delete();

							d0Delete.Header.ID = deleteMessageOrder.ID;
							d0Delete.Header.MessageID = deleteMessageOrder.MessageId;
							d0Delete.Header.MessageTypeID = deleteMessageOrder.MessageTypeId;
							d0Delete.Header.MessageTypeDescription = deleteMessageOrder.MessageTypeDescription;
							d0Delete.Header.SentDate = DateTime.Now;
							d0Delete.Header.DeleteMessageID = deleteMessageOrder.DeleteMessageId;
							d0Delete.Header.DeleteMessageTypeID = deleteMessageOrder.DeleteMessageTypeId;
							d0Delete.Header.DeleteMessageTypeDescription = deleteMessageOrder.DeleteMessageTypeDescription;

							result.Add(d0Delete);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
					}
				}
			}
			catch (Exception e)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), e);
			}

			return result;
		}

		/// <summary>
		/// vytvoří seznam aktivních CRM objednávek (CrmOrder C0), které nebyly odeslány do ND
		/// </summary>
		/// <returns></returns>
		public static List<C0CrmOrder> CreateC0CrmOrder(ref string returnMessage)
		{
			List<C0CrmOrder> result = new List<C0CrmOrder>();

			try
			{
				using (var db = new FenixEntities())
				{
					try
					{
						db.Configuration.LazyLoadingEnabled = false;
						db.Configuration.ProxyCreationEnabled = false;

						var crmOrders = (from b in db.CommunicationMessagesCrmOrder
										 orderby b.ID ascending
										 where b.IsActive == true && (b.MessageStatusID == 1 || b.MessageStatusID == 2)
										 select b).Take(BC.NumRowsToSend);

						foreach (var crmOrder in crmOrders)
						{
							C0CrmOrder c0CrmOrder = new C0CrmOrder();

							c0CrmOrder.Header.ID = crmOrder.ID;
							c0CrmOrder.Header.MessageID = crmOrder.MessageID;
							c0CrmOrder.Header.MessageTypeID = crmOrder.MessageTypeID;
							c0CrmOrder.Header.MessageTypeDescription = crmOrder.MessageDescription;
							c0CrmOrder.Header.MessageDateOfShipment = DateTime.Now;
							c0CrmOrder.Header.WO_PR_NUMBER = crmOrder.WO_PR_NUMBER;
							c0CrmOrder.Header.WO_PR_CREATE_DATE = crmOrder.WO_PR_CREATE_DATE;
							c0CrmOrder.Header.WO_DELIVERY_DATE_FROM = crmOrder.WO_DELIVERY_DATE_FROM;
							c0CrmOrder.Header.WO_DELIVERY_DATE_TO = crmOrder.WO_DELIVERY_DATE_TO;
							c0CrmOrder.Header.WO_CID = crmOrder.WO_CID;
							c0CrmOrder.Header.WO_FIRST_LAST_NAME = crmOrder.WO_FIRST_LAST_NAME;
							c0CrmOrder.Header.WO_STREET_NAME = crmOrder.WO_STREET_NAME;
							c0CrmOrder.Header.WO_HOUSE_NUMBER = crmOrder.WO_HOUSE_NUMBER;
							c0CrmOrder.Header.WO_CITY = crmOrder.WO_CITY;
							c0CrmOrder.Header.WO_ZIP = crmOrder.WO_ZIP;
							c0CrmOrder.Header.WO_PHONE = crmOrder.WO_PHONE;
							c0CrmOrder.Header.WO_EMAIL_INFO1 = crmOrder.WO_EMAIL_INFO1;
							c0CrmOrder.Header.WO_PR_TYPE = crmOrder.WO_PR_TYPE;
							c0CrmOrder.Header.WO_FLOOR_FLAT = crmOrder.WO_FLOOR_FLAT;
							c0CrmOrder.Header.WO_UPC_TEL1 = crmOrder.WO_UPC_TEL1;
							c0CrmOrder.Header.WO_UPC_TEL2 = crmOrder.WO_UPC_TEL2;
							c0CrmOrder.Header.WO_EMAIL = crmOrder.WO_EMAIL;
							c0CrmOrder.Header.WO_PASSWORD = crmOrder.WO_PASSWORD;
							c0CrmOrder.Header.WO_CPE_BACK = crmOrder.WO_CPE_BACK;
							c0CrmOrder.Header.WO_NOTE = crmOrder.WO_NOTE;
							c0CrmOrder.Header.L_TYPE_OF_ORDER = "SINGLE";		//prozatim konstanta 'SINGLE', vyhledove crmOrder.TypeOfOrder;							
							c0CrmOrder.Header.L_SHIPMENT_EXPECTED = crmOrder.L_SHIPMENT_EXPECTED;

							var crmOrderItems = from b in db.CommunicationMessagesCrmOrderItems
											    orderby b.ID ascending
												where b.CommunicationMessageID == crmOrder.ID && b.IsActive == true
												select b;

							foreach (var crmOrderItem in crmOrderItems)
							{
								C0Items c0Item = new C0Items();

								c0Item.L_ITEM_VER_KIT = crmOrderItem.L_ITEM_VER_KIT;//.ItemVerKit;
								c0Item.L_ITEM_OR_KIT_ID = crmOrderItem.L_ITEM_OR_KIT_ID;//.ItemOrKitID;
								c0Item.L_ITEM_OR_KIT_DESCRIPTION = crmOrderItem.L_ITEM_OR_KIT_DESCRIPTION;//.ItemOrKitDescription;
								c0Item.L_ITEM_OR_KIT_QUANTITY = crmOrderItem.L_ITEM_OR_KIT_QUANTITY;//.ItemOrKitQuantity;
								c0Item.L_ITEM_OR_KIT_QUALITY_ID = crmOrderItem.L_ITEM_OR_KIT_QUALITY_ID;//.ItemOrKitQualityID;
								c0Item.L_ITEM_OR_KIT_QUALITY = crmOrderItem.L_ITEM_OR_KIT_QUALITY;//.ItemOrKitQuality;
								c0Item.L_ITEM_OR_KIT_MEASURE_ID = crmOrderItem.L_ITEM_OR_KIT_MEASURE_ID;//.ItemOrKitUnitOfMeasureID;
								c0Item.L_ITEM_OR_KIT_MEASURE = crmOrderItem.L_ITEM_OR_KIT_MEASURE;//.ItemOrKitUnitOfMeasure;

								c0CrmOrder.Header.items.Add(c0Item);
							}

							result.Add(c0CrmOrder);
						}
					}
					catch (Exception ex)
					{
						returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), ex);
					}
				}
			}
			catch (Exception e)
			{
				returnMessage = BC.CreateErrorMessage(ApplicationLog.GetMethodName(), e);
			}

			return result;
		}


		private static int CreateCdlID()
		{
			int result = 0;

			using (var db = new FenixEntities())
			{
				try
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("NewValue", typeof(int));

					db.prGetIntValueFromCounter("CDL", retVal);

					result = retVal.Value.ToInt32(BC.NOT_OK);
				}
				catch
				{
					throw;
				}
			}

			return result;
		}				
	}
}
