using System;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using FenixAutomat.EmailSender;
using FenixAutomat.Message.Sender;
using FenixHelper;
using FenixHelper.XMLMessage;
using UPC.Extensions.Convert;

namespace FenixAutomat.EmailCreator
{
	/// <summary>
	/// Vytvoří a odešle email s oznámením o závozu CPE
	/// </summary>
	public class ShipmentOrderNotification
	{
		#region Properties

		/// <summary>
		/// Toto se nahradí v šabloně emailu vygenerovaným obsahem
		/// (generuje se z aktuální S0 Shipment Order)
		/// </summary>
		private const string TABLE_CONTENT = "#TABLE_CONTENT#";

		/// <summary>
		/// Aktuální S0, která slouží jako zdroj pro vygenerování obsahu
		/// </summary>
		private S0Shipment actualS0Shipment;

		/// <summary>
		/// Příznak, zda je možné odeslat email
		/// </summary>
		private bool canSendEmail;

		/// <summary>
		/// Tělo emailu
		/// </summary>
		private string emailBody;

		/// <summary>
		/// Hash těla emailu a emailové adresy příjemce
		/// </summary>
		private string emailBodyHash;

		/// <summary>
		/// Předmět emailu
		/// </summary>
		private string subject;

		/// <summary>
		/// Název vkládaného obrázku
		/// </summary>
		private string embededPictureName;

		/// <summary>
		/// Název šablony, ze které se vytvoří výsledný email
		/// </summary>
		private string templateName;

		#endregion		

		/// <summary>
		/// ctor
		/// </summary>
		private ShipmentOrderNotification()
		{
		}

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="s0Shipment"></param>
		public ShipmentOrderNotification(S0Shipment s0Shipment, ReturnedValueFromND ndResult)
		{
			this.actualS0Shipment = s0Shipment;			
			this.canSendEmail = this.emailCanBeSend(s0Shipment, ndResult);
			this.emailBody = String.Empty;
			this.subject = "Expedice CPE";
			this.embededPictureName = "signature.jpg";
			this.templateName = "S0notification.html";
		}

		/// <summary>
		/// Vytvoření a odeslání emailu
		/// </summary>
		public void CreateAndSendEmail()
		{
			try
			{
				if (this.canSendEmail)
				{
					this.createAndSendEmail();
					this.saveEmailToDatabase();
				}
				else
				{
					if (actualS0Shipment.Header.ContactEmail.IsNullOrEmpty())
					{
						this.sendErrorEmail(String.Format("S0 ID = [{0}]\nnemá vyplněný email příjemce.", this.actualS0Shipment.Header.ID), AppLog.GetMethodName());
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Rozhodnutí, zda je možné odeslat informační email
		///		1. S0 se vztahuje k CPE
		///		2. S0 byla úspěšně zapsána do ND
		///		3. je vyplněný kontaktní email
		/// </summary>
		/// <param name="s0Shipment"></param>
		/// <param name="ndResult"></param>
		/// <returns></returns>
		private bool emailCanBeSend(S0Shipment s0Shipment, ReturnedValueFromND ndResult)
		{
			bool canBeSend = false;
			
			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;

					var vydejka = (
									from p in db.CommunicationMessagesShipmentOrdersSent
									where p.ID == s0Shipment.Header.ID
									select p.IdWf
								  ).SingleOrDefault();
					 
					canBeSend = (vydejka == null) && (s0Shipment.Header.ContactEmail.IsNotNullOrEmpty() && ndResult.Result == BC.WRITE_TO_ND_OK);
				}
			}
			catch (Exception)
			{				
			}

			return canBeSend;
		}

		/// <summary>
		/// Vlastní vytvoření o odeslání S0 oznamovacího emailu
		///		- pro vytvoření se použije HTML template uložené na disku
		///		- do těla mailu se vkládá obrázek uložený na disku
		///		- email se odesílá právě 1x
		/// </summary>
		private void createAndSendEmail()
		{
			string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string template = Path.Combine(assemblyPath, this.templateName);
			string pictureName = Path.Combine(assemblyPath, this.embededPictureName);

			try
			{
				using (StreamReader streamReader = new StreamReader(template))
				{
					this.emailBody = streamReader.ReadToEnd();
				}

				this.emailBody = this.emailBody.Replace(TABLE_CONTENT, createTableContent());
				this.emailBodyHash = BC.CreateSHA256Hash(this.emailBody + this.actualS0Shipment.Header.ContactEmail);

				if (Email.EmailCountByBodyHash(this.emailBodyHash) == 0)
				{
					AlternateView avHtml = AlternateView.CreateAlternateViewFromString(this.emailBody, null, MediaTypeNames.Text.Html);
					LinkedResource pic1 = new LinkedResource(pictureName, MediaTypeNames.Image.Jpeg);
					pic1.ContentId = "Signature";
					avHtml.LinkedResources.Add(pic1);

					// Add the alternate views instead of using MailMessage.Body
					MailMessage mailMessage = new MailMessage();
					mailMessage.AlternateViews.Add(avHtml);

					// Address and send the message
					mailMessage.From = new MailAddress(BC.MailFrom);
					mailMessage.To.Add(new MailAddress(this.actualS0Shipment.Header.ContactEmail));
					mailMessage.Subject = this.subject;

					SmtpClient client = new SmtpClient(BC.MailServer);
					client.Send(mailMessage);
				}
			}
			catch (Exception ex)
			{
				this.sendErrorEmail(ex, AppLog.GetMethodName());
				throw;
			}
		}

		/// <summary>
		/// Uložení emailu do databáze
		/// </summary>
		private void saveEmailToDatabase()
		{
			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));

					bool emailIsInternal = false;
					db.prEmailSentWrite(BC.APP_NAMESPACE, BC.MailFrom, this.actualS0Shipment.Header.ContactEmail,
						                this.subject, this.emailBodyHash, this.emailBody, this.embededPictureName, 
										BC.ServiceUserId, className(), emailIsInternal, retVal, retMsg);
				}
			}
			catch(Exception ex)
			{
				this.sendErrorEmail(ex, AppLog.GetMethodName());
				throw;
			}
		}

		/// <summary>
		/// Vytvoření řádků tabulky
		/// </summary>
		/// <returns></returns>
		private string createTableContent()
		{
			string content = String.Empty;
			string lokace = this.actualS0Shipment.Header.CustomerName ?? String.Empty;

			foreach (S0Items item in this.actualS0Shipment.Header.items)
			{
				content += String.Format(
							"<tr class=\"gridrows\">" +
							"<td>{0}</td>" +
							"<td nowrap>{1}</td>" +
							"<td align=\"right\">{2}</td>" +
							"<td>{3}</td>" +
							"<td>{4}</td>" +
							"<td nowrap>{5}</td>" +
							"</tr>", item.ItemOrKitID, item.ItemOrKitDescription, 
							ConvertExtensions.ToInt32(item.ItemOrKitQuantity, 0), 
							item.ItemOrKitUnitOfMeasure, item.ItemOrKitQuality, lokace);
			}
					
			return content;
		}

		private void sendErrorEmail(Exception exception, string methodName)
		{
			this.sendErrorEmail(BC.CreateErrorMessage(methodName, exception), methodName);
		}

		private void sendErrorEmail(string errMessage, string methodName)
		{
			Email errorEmail = new Email()
			{
				Type = BC.APP_NAMESPACE,
				Subject = String.Format("{0} - {1}", "Fenix Automat ERROR", className()),
				Body = errMessage,
				MailTo = BC.MailErrorTo,
				SendOnlyOnce = true,
				Source = methodName
			};

			errorEmail.SendMail();
		}

		/// <summary>
		/// Vrací jmého třídy
		/// </summary>
		/// <returns></returns>
		private static string className()
		{
			string[] par = AppLog.GetMethodName().Split('.');
			string name = par.Length >= 2 ? par[1] : "UNKNOWN CLASS NAME";

			return name;
		}
	}
}
