using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace FenixAutomat.EmailReceiver
{
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

		public D0DeleteEmailReceive(string exchangeURL, string domainName, string userName, string password)
		{
			this.exchangeURL = exchangeURL;
			this.domainName = domainName;
			this.userName = userName;
			this.password = password;			
		}
				
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
		/// <returns></returns>
		public int GetUnreadMailsCountInFolder(WellKnownFolderName folder)
		{
			int unreadMailsCount;
			switch (folder)
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
		/// <param name="unreadCount"></param>
		/// <returns></returns>
		public FindItemsResults<Item> GetUndreadItemsInFolder(WellKnownFolderName folder, int unreadCount)
		{							
			SearchFilter sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));
			ItemView view = new ItemView(unreadCount);
			return this.exchangeService.FindItems(folder, sf, view);
		}

	}
}
