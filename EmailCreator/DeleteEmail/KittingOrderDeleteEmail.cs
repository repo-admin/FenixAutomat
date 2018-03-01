using System;
using System.Linq;
using FenixAutomat.Loggers;
using FenixHelper;
using FenixHelper.Common;

namespace FenixAutomat.EmailCreator.DeleteEmail
{
	/// <summary>
	/// D0 Delete Message [Order] pro KittingOrder
	/// (vytvoří rušící email z html souboru na disku, odešle rušící email, uloží rušící email do databáze - datová zádrž, aby byl poslán právě 1x)
	/// </summary>
	public class KittingOrderDeleteEmail : BaseOrderDeleteEmail
	{
		public KittingOrderDeleteEmail(int id, int messageId)
		{
			base.ID = id;
			base.MessageId = messageId;
			base.Subject = this.createEmailSubject();
			base.HtmlTemplateName = "K0forD0send.html";
			base.ChildClassName = this.className();
			base.Result = new ResultAppService(BC.NOT_OK, string.Empty);
		}

		public ResultAppService CreateAndSendEmail()
		{
			base.HeaderContent = this.createTableHeaderContent();
			base.DetailContent = this.createTableDetailContent();

			if (base.HeaderContent.IsNotNullOrEmpty() && base.DetailContent.IsNotNullOrEmpty())
			{
				base.CreateAndSendDeleteEmail();
				base.SaveEmailToDatabase();
			}

			return base.Result;
		}

		/// <summary>
		/// Vytvoření řádků tabulky pro hlavičku
		/// </summary>
		/// <returns></returns>
		private string createTableHeaderContent()
		{
			string content = String.Empty;

			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;

					var kittingOrdersSent = from b in db.vwCMKSent
									        orderby b.ID ascending
									        where b.IsActive == true && b.ID == this.ID && b.MessageId == this.MessageId
									        select b;

					foreach (var kittingOrderSent in kittingOrdersSent)
					{
						content += String.Format(
									"<tr class=\"gridrows\">" +
									"<td>{0}</td>" +
									"<td>{1}</td>" +
									"<td>{2}</td>" +
									"<td nowrap>{3}</td>" +
									"</tr>", kittingOrderSent.ID, kittingOrderSent.MessageId,
									kittingOrderSent.MessageDateOfShipment, kittingOrderSent.CompanyName);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.ProcessError(base.Result, ex, AppLog.GetMethodName(), BC.ServiceUserId);
			}

			return content;
		}

		/// <summary>
		/// Vytvoření řádků tabulky pro detail
		/// </summary>
		/// <returns></returns>
		private string createTableDetailContent()
		{
			string content = String.Empty;

			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;

					var kittingSentItems = from b in db.CommunicationMessagesKittingsSentItems
						                   orderby b.ID ascending
						                   where b.IsActive == true && b.CMSOId == this.ID
						                   select b;

					foreach (var kittingSentItem in kittingSentItems)
					{
						content += String.Format(
									"<tr class=\"gridrows\">" +
									"<td align=\"right\">{0}</td>" +
									"<td>{1}</td>" +
									"<td nowrap>{2}</td>" +
									"<td align=\"right\">{3}</td>" +
									"<td>{4}</td>" +
									"<td>{5}</td>" +									
									"</tr>", kittingSentItem.ID, kittingSentItem.KitId, kittingSentItem.KitDescription,
									kittingSentItem.KitQuantity.ToString("# ### ###.###"), kittingSentItem.KitUnitOfMeasure,
									kittingSentItem.KitQualityCode);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.ProcessError(base.Result, ex, AppLog.GetMethodName(), BC.ServiceUserId);
			}

			return content;
		}

		/// <summary>
		/// Vrací jmého třídy
		/// </summary>
		/// <returns></returns>
		private string className()
		{
			string[] par = AppLog.GetMethodName().Split('.');
			string name = par.Length >= 2 ? par[1] : "UNKNOWN CLASS NAME";

			return name;
		}

		private string createEmailSubject()
		{
			return string.Format("UPC_CZ Fenix DeleteMessage MessageDescription={0} ID={1} MessageID={2}", "KittingOrder", this.ID, this.MessageId);
		}
	}
}
