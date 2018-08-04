using System;
using System.Data.Entity;
using System.Linq;
using Fenix;
using FenixAutomat.Loggers;

namespace FenixAutomat
{
    /// <summary>
    /// Třída sloužící pro práci s <seealso cref="InternalDocuments"/>
    /// </summary>
	public class InternalDocumentsHandler
	{
	    /// <summary>
	    /// ZicyzId uživatele, který provedl změnu na skladové kartě
	    /// </summary>
	    public int ZicyzId { get; }

	    /// <summary>
	    /// Místo vzniku interního dokladu
	    /// </summary>
	    public InternalDocumentsSource Source { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zicyzId">Id uživatele, který provedl změnu na skladové kartě</param>
        /// <param name="source">Instance třídy <seealso cref="InternalDocumentsSource"/></param>
		public InternalDocumentsHandler(int zicyzId, InternalDocumentsSource source)
		{
			this.ZicyzId = zicyzId;
			this.Source = source;
		}

        /// <summary>
        /// Vrací kolekci aktivních objektů <seealso cref="CardStockItems"/> pro specifikované kid ID,
        /// viz <param name="id"></param>
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
        /// <param name="id"></param>
        /// <returns></returns>
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
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}

			return cardStockItem;
		}

        /// <summary>
        /// Vytváří novou instanci třídy <seealso cref="InternalDocuments"/>
        /// </summary>
        /// <param name="db">Instance třídy <seealso cref="DbContext"/> databáze Fénix</param>
        /// <param name="cardStockStatusBefore"></param>
        /// <param name="cardStockStatusAfter"></param>
		public void CreateInternalDocuments(FenixEntities db, CardStockItems cardStockStatusBefore, CardStockItems cardStockStatusAfter)
		{
			if ((cardStockStatusBefore == null) || (cardStockStatusAfter == null) || (this.IsSame(cardStockStatusBefore, cardStockStatusAfter))) return;
			
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
				internalDocument.InternalDocumentsSourceId = (int)this.Source;
				internalDocument.IsActive = true;
				internalDocument.ModifyDate = DateTime.Now;
				internalDocument.ModifyUserId = this.ZicyzId;

				db.InternalDocuments.Add(internalDocument);				
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}			
		} 

		/// <summary>
		/// Rozhodnutí, zda sledovaná množství na skladové kartě před změnou a po změně jsou stejná
		/// </summary>
		/// <returns></returns>
		private bool IsSame(CardStockItems cardStockStatusBefore, CardStockItems cardStockStatusAfter)
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