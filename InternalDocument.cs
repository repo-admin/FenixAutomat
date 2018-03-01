using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FenixAutomat.Loggers;
using FenixHelper;

namespace FenixAutomat
{
	public class InternalDocument
	{
		/// <summary>
		/// ZicyzId uživatele, který provedl změnu na skladové kartě
		/// </summary>
		private int zicyzId;
				
		/// <summary>
		/// Místo vzniku interního dokladu
		/// </summary>
		private InternalDocumentsSource source;

		#region Not used
		//private CardStockItems cardStockStatusBefore;
		
		///// <summary>
		///// Stav na skladové kartě před změnou/změnami
		///// </summary>
		//public CardStockItems CardStockStatusBefore 
		//{
		//	get 
		//	{ 
		//		return this.cardStockStatusBefore; 
		//	} 
		//	set 
		//	{
		//		this.cardStockStatusBefore = value;
		//	} 
		//}		

		///// <summary>
		///// Stav na skladové kartě po změně/změnách
		///// </summary>
		//public CardStockItems CardStockStatusAfter { get; set; } 
		#endregion

		public InternalDocument(int zicyzId, InternalDocumentsSource source)
		{
			this.zicyzId = zicyzId;
			this.source = source;
		}

		public CardStockItems GetCardStock(FenixEntities db, int id)
		{
			CardStockItems cardStockItem = new CardStockItems();

			try
			{
				cardStockItem = (from c in db.CardStockItems
									where c.IsActive == true && c.ItemOrKitID == id 
									select c).FirstOrDefault();
			}
			catch (Exception ex)
			{					
				Logger.ProcessError(ex, AppLog.GetMethodName(), BC.ServiceUserId);
			}

			return cardStockItem;
		}

		public void CreateInternalDocument(FenixEntities db, CardStockItems cardStockStatusBefore, CardStockItems cardStockStatusAfter)
		{
			if ((cardStockStatusBefore == null) || (cardStockStatusAfter == null) || (this.isSame(cardStockStatusBefore, cardStockStatusAfter))) return;
			
			try
			{
				InternalDocuments internalDocument = new InternalDocuments();
				internalDocument.ItemVerKit = cardStockStatusBefore.ItemVerKit;
				internalDocument.ItemOrKitID = cardStockStatusBefore.ItemOrKitID;
				internalDocument.ItemOrKitUnitOfMeasureId = cardStockStatusBefore.ItemOrKitUnitOfMeasureId;
				internalDocument.ItemOrKitQualityId = cardStockStatusBefore.ItemOrKitQuality;
				//1
				internalDocument.ItemOrKitFreeBefore = cardStockStatusBefore.ItemOrKitFree;
				internalDocument.ItemOrKitFreeAfter = cardStockStatusAfter.ItemOrKitFree;
				//2
				internalDocument.ItemOrKitUnConsilliationBefore = cardStockStatusBefore.ItemOrKitUnConsilliation;
				internalDocument.ItemOrKitUnConsilliationAfter = cardStockStatusAfter.ItemOrKitUnConsilliation;
				//3
				internalDocument.ItemOrKitReservedBefore = cardStockStatusBefore.ItemOrKitReserved;
				internalDocument.ItemOrKitReservedAfter = cardStockStatusAfter.ItemOrKitReserved;
				//4
				internalDocument.ItemOrKitReleasedForExpeditionBefore = cardStockStatusBefore.ItemOrKitReleasedForExpedition;
				internalDocument.ItemOrKitReleasedForExpeditionAfter = cardStockStatusAfter.ItemOrKitReleasedForExpedition;
				//5
				internalDocument.ItemOrKitExpeditedBefore = cardStockStatusBefore.ItemOrKitExpedited;
				internalDocument.ItemOrKitExpeditedAfter = cardStockStatusAfter.ItemOrKitExpedited;

				internalDocument.StockId = 2;	//ND/XPO
				internalDocument.InternalDocumentsSourceId = (int)this.source;
				internalDocument.IsActive = true;
				internalDocument.ModifyDate = DateTime.Now;
				internalDocument.ModifyUserId = this.zicyzId;

				db.InternalDocuments.Add(internalDocument);				
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, AppLog.GetMethodName(), BC.ServiceUserId);
			}			
		} 

		/// <summary>
		/// Rozhodnutí, zda sledovaná množství na skladové kartě před změnou a po změně jsou stejná
		/// </summary>
		/// <returns></returns>
		private bool isSame(CardStockItems cardStockStatusBefore, CardStockItems cardStockStatusAfter)
		{
			return (
						(cardStockStatusBefore.ItemOrKitFree == cardStockStatusAfter.ItemOrKitFree) &&
						(cardStockStatusBefore.ItemOrKitUnConsilliation == cardStockStatusAfter.ItemOrKitUnConsilliation) &&
						(cardStockStatusBefore.ItemOrKitReserved == cardStockStatusAfter.ItemOrKitReserved) &&
						(cardStockStatusBefore.ItemOrKitReleasedForExpedition == cardStockStatusAfter.ItemOrKitReleasedForExpedition) &&
						(cardStockStatusBefore.ItemOrKitExpedited == cardStockStatusAfter.ItemOrKitExpedited)
					);

		}
	}
}