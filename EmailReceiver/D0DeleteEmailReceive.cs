using System;
using Microsoft.Exchange.WebServices.Data;

namespace FenixAutomat.EmailReceiver
{
    /// <summary>
    /// 
    /// </summary>
	public class D0DeleteEmailReceive
	{
		private readonly string exchangeURL;
		private readonly string domainName;
		private readonly string userName;
		private readonly string password;
		private ExchangeService exchangeService;

		public ExchangeService ExchangeService
		{
			get 
			{
				if (this.exchangeService == null)
				{
					throw new Exception("ExchangeService not initialized.");
				}
				return this.exchangeService; 
			}
			private set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				this.exchangeService = value;
			}
		}

		private D0DeleteEmailReceive()
		{
		}

        /// <summary>
        /// Vytváří instanci <seealso cref="D0DeleteEmailReceive"/>
        /// </summary>
        /// <param name="exchangeUrl"></param>
        /// <param name="domainName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
		public D0DeleteEmailReceive(string exchangeUrl, string domainName, string userName, string password)
		{
			this.exchangeURL = exchangeUrl;
			this.domainName = domainName;
			this.userName = userName;
			this.password = password;			
		}

        /// <summary>
        /// Vytváří instanci třídy <seealso cref="Microsoft.Exchange.WebServices.Data.ExchangeService"/>
        /// </summary>
        public void CreateExchangeService()
		{
			this.ExchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP2)		//Exchange2010_SP2  ExchangeVersion.Exchange2007_SP1
			{
				Credentials = new WebCredentials(this.userName, this.password, this.domainName),
				Url = new Uri(exchangeURL)
			};
		}

        /// <summary>
        /// Vrací počet nepřečtených emailů v adresáři
        /// </summary>
        /// <param name="folderType">typ adresáře</param>
        /// <returns>Vrací počet nepřečtených emailů v adresáři</returns>
        public int GetUnreadMailsCountInFolder(WellKnownFolderName folderType)
		{
			int unreadMailsCount;
			switch (folderType)
			{
				case WellKnownFolderName.Inbox:
					unreadMailsCount = Folder.Bind(this.exchangeService, WellKnownFolderName.Inbox).UnreadCount;
					break;
				case WellKnownFolderName.SentItems:
					unreadMailsCount = Folder.Bind(this.exchangeService, WellKnownFolderName.SentItems).UnreadCount;
					break;
				case WellKnownFolderName.DeletedItems:
					unreadMailsCount = Folder.Bind(this.exchangeService, WellKnownFolderName.DeletedItems).UnreadCount;
					break;
				default:
					throw new Exception("Unknown folder type.");
			}
			
			return unreadMailsCount;
		}

        /// <summary>
        /// Nepřečtené itemy
        /// </summary>
        /// <param name="folderType">typ adresáře</param>
        /// <param name="pageSize">Počet položek na stránce</param>
        /// <returns></returns>
        public FindItemsResults<Item> GetUndreadItemsInFolder(WellKnownFolderName folderType, int pageSize)
		{							
			SearchFilter sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));
			ItemView view = new ItemView(pageSize);
			return this.exchangeService.FindItems(folderType, sf, view);
		}

	}
}
