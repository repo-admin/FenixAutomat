using System;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using Fenix;
using Fenix.Common;
using FenixAutomat.EmailSender;
using FenixAutomat.Loggers;

namespace FenixAutomat.EmailCreator.DeleteEmail
{
	/// <summary>
	/// Bázová třída pro D0 Delete Message [Order]
	/// </summary>
	public class BaseOrderDeleteEmail
	{
		#region Properties

		/// <summary>
		/// výsledek
		/// </summary>
		public ResultAppService Result { get; set; }

		/// <summary>
		/// ID rušené zprávy
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Message ID rušené zprávy
		/// </summary>
		public int MessageId { get; set; }

		/// <summary>
		/// Hlavička co se nahradí v šabloně emailu vygenerovaným obsahem		
		/// </summary>
		private const string TableHeaderContent = "#TABLE_HEADER_CONTENT#";

		/// <summary>
		/// Vygenerovaný obsah hlavičky
		/// </summary>
		protected string HeaderContent { get; set; }

		/// <summary>
		/// Detail co se nahradí v šabloně emailu vygenerovaným obsahem		
		/// </summary>	
		private const string TableDetailContent = "#TABLE_DETAIL_CONTENT#";

		/// <summary>
		/// Vygenerovaný obsah detailu
		/// </summary>
		protected string DetailContent { get; set; }

		/// <summary>
		/// Tělo emailu
		/// </summary>
		protected string EmailBody { get; set; }

		/// <summary>
		/// Hash těla emailu a emailové adresy příjemce
		/// </summary>
		private string _emailBodyHash;

		/// <summary>
		/// Předmět emailu
		/// </summary>
		protected string Subject { get; set; }

		/// <summary>
		/// Název vkládaného obrázku
		/// </summary>
		private string _embededPictureName;

		/// <summary>
		/// Název šablony, ze které se vytvoří výsledný email
		/// </summary>
		protected string HtmlTemplateName { get; set; }

		/// <summary>
		/// Jméno třídy potomka
		/// </summary>
		protected string ChildClassName { get; set; } 

		/// <summary>
		/// Příznak, že rušící email byl úspěšne odeslán
		/// </summary>
		private bool _emailSuccessfullySent;
		
		#endregion

		/// <summary>
		/// ctor
		/// </summary>
		public BaseOrderDeleteEmail()
		{
			this.EmailBody = string.Empty;
			this._embededPictureName = "signature.jpg";
			this._emailSuccessfullySent = false;
		}
		
		/// <summary>
		/// Vlastní vytvoření a odeslání rušícího emailu
		///		- pro vytvoření se použije HTML template uložené na disku
		///		- do těla mailu se vkládá obrázek uložený na disku
		///		- email se odesílá právě 1x
		/// </summary>
		protected void CreateAndSendDeleteEmail()
		{
			string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string template = Path.Combine(assemblyPath, this.HtmlTemplateName);
			string pictureName = Path.Combine(assemblyPath, this._embededPictureName);

			try
			{
				using (StreamReader streamReader = new StreamReader(template))
				{
					this.EmailBody = streamReader.ReadToEnd();
				}

				this.EmailBody = this.EmailBody.Replace(TableHeaderContent, this.HeaderContent);
				this.EmailBody = this.EmailBody.Replace(TableDetailContent, this.DetailContent);

				this._emailBodyHash = BC.CreateSHA256Hash(this.EmailBody + BC.D0MailTo);

				if (Email.EmailCountByBodyHash(this._emailBodyHash) == 0)
				{
					AlternateView avHtml = AlternateView.CreateAlternateViewFromString(this.EmailBody, null, MediaTypeNames.Text.Html);
					LinkedResource pic1 = new LinkedResource(pictureName, MediaTypeNames.Image.Jpeg);
					pic1.ContentId = "Signature";
					avHtml.LinkedResources.Add(pic1);

					// Add the alternate views instead of using MailMessage.Body
					MailMessage mailMessage = new MailMessage();
					mailMessage.AlternateViews.Add(avHtml);

					// Address and send the message
					mailMessage.From = new MailAddress(BC.D0MailFrom);
					//mailMessage.To.Add(new MailAddress(BC.D0MailTo));
					this.AddAddressesTo(mailMessage);
					mailMessage.Subject = this.Subject;

					SmtpClient client = new SmtpClient(BC.MailServer);
					client.Send(mailMessage);

					this._emailSuccessfullySent = true;
				}
			}
			catch (Exception ex)
			{
				Logger.ProcessError(this.Result, ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
		}

		/// <summary>
		/// Přidá adresy příjemců
		/// </summary>
		/// <param name="mailMessage"></param>
		private void AddAddressesTo(MailMessage mailMessage)
		{
			char[] delims = { ';', ',' };
									
			if (String.IsNullOrWhiteSpace(BC.D0MailTo) == false)
			{
				string[] addrs = BC.D0MailTo.Split(delims);
				for (int i = 0; i < addrs.Length; i++)
				{
					mailMessage.To.Add(new MailAddress(addrs[i]));					
				}
			}
		}

		/// <summary>
		/// Uložení emailu do databáze
		/// </summary>
		protected void SaveEmailToDatabase()
		{
			if (this._emailSuccessfullySent)
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
						db.prEmailSentWrite(BC.APP_NAMESPACE, BC.D0MailFrom, BC.D0MailTo,
											this.Subject, this._emailBodyHash, this.EmailBody, this._embededPictureName,
											BC.ServiceUserId, this.ChildClassName, emailIsInternal, retVal, retMsg);
					}
					this.Result.ResultNumber = BC.OK;
				}
				catch (Exception ex)
				{
					Logger.ProcessError(this.Result, ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
				}
			}
		}
	}
}
