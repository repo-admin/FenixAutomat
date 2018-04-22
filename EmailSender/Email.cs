using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Net.Mail;
using UPC.Extensions.Convert;

namespace FenixAutomat.EmailSender
{
    /// <summary>
    /// Objektová reprezentace emailové zprávy
    /// </summary>
	public class Email
	{
		#region Properties

		/// <summary>
		/// Hash těla mailu
		/// </summary>
		private string emailBodyHash;

		/// <summary>
		/// Typ = z jakého programu je mail posílán
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Předmět
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// Tělo
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Adresa/y příjemce. Povinná je alespoň jeda adresa
		/// </summary>
		public string MailTo { get; set; }

		/// <summary>
		/// Adresa/y příjemce kopie
		/// </summary>
		public string MailCC { get; set; }

		/// <summary>
		/// Adresa/y příjemce slepé kopie
		/// </summary>
		public string MailBcc { get; set; }

		/// <summary>
		/// Příznak, zda je tělo mailu ve formátu HTML
		/// </summary>
		public bool IsBodyHtml { get; set; }

		/// <summary>
		/// Jméno procedury ze které se mail posílá
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// Požadavek na odeslání právě jednou
		/// </summary>
		public bool SendOnlyOnce { get; set; }

		/// <summary>
		/// Obrázek vložený do emailu
		/// </summary>
		public string EmbededPicture { get; set; }

		/// <summary>
		/// interní email(UPC)/externí email(mimo UPC)
		/// (default je interni)
		/// </summary>
		public bool IsInternal { get; set; } 
		
		#endregion

		#region Constructors

		public Email()
		{
			this.Type = String.Empty;
			this.Subject = String.Empty;
			this.Body = String.Empty;
			this.MailTo = String.Empty;
			this.MailCC = String.Empty;
			this.MailBcc = String.Empty;
			this.IsBodyHtml = false;
			this.Source = String.Empty;
			this.SendOnlyOnce = false;
			this.emailBodyHash = String.Empty;
			this.IsInternal = true;
		}

		#endregion

		#region Public Methods

		/// <summary>Odeslání mailu (email se neukládá do databáze)</summary>
		/// <param name="mailSubject">Předmět mailu.</param>
		/// <param name="mailBody">Tělo mailu.</param>
		/// <param name="isBodyHtml">Příznak, jseslti je tělo formátu html.</param>
		/// <param name="mailTo">Adresy příjemců oddělené středníkem. Povinná je alespoň jeda adresa.</param>
		/// <param name="mailCC">Adresy příjemců kopie oddělené středníkem.</param>
		/// <param name="mailBcc">Adresy příjemců slepé kopie oddělené středníkem.</param>
		public static bool SendMail(string mailSubject, string mailBody, bool isBodyHtml, string mailTo, string mailCC, string mailBcc)
		{
			return Email.SendMail(mailSubject, mailBody, isBodyHtml, mailTo, mailCC, mailBcc, null);
		}

		/// <summary>Odeslání mailu (email se neukládá do databáze)</summary>
		/// <param name="mailSubject">Předmět mailu.</param>
		/// <param name="mailBody">Tělo mailu.</param>
		/// <param name="isBodyHtml">Příznak, jseslti je tělo formátu html.</param>
		/// <param name="mailTo">Adresy příjemců oddělené středníkem. Povinná je alespoň jeda adresa.</param>
		/// <param name="mailCC">Adresy příjemců kopie oddělené středníkem.</param>
		/// <param name="mailBcc">Adresy příjemců slepé kopie oddělené středníkem.</param>
		/// <param name="attachments">Kolekce příloh.</param>
		public static bool SendMail(string mailSubject, string mailBody, bool isBodyHtml, string mailTo, string mailCC, string mailBcc, List<Attachment> attachments)
		{
			bool retVal = false;
			try
			{
				char[] delims = { ';', ',' };

				// Adresy příjemců
				MailAddressCollection addrsTo = new MailAddressCollection();
				if (String.IsNullOrWhiteSpace(mailTo) == false)
				{
					string[] addrs = mailTo.Split(delims);
					for (int i = 0; i < addrs.Length; i++)
					{
						addrsTo.Add(new MailAddress(addrs[i]));
					}
				}

				// Adresy příjemců kopie
				MailAddressCollection addrsCC = new MailAddressCollection();
				if (String.IsNullOrWhiteSpace(mailCC) == false)
				{
					string[] addrs = mailCC.Split(delims);
					for (int i = 0; i < addrs.Length; i++)
					{
						addrsCC.Add(new MailAddress(addrs[i]));
					}
				}

				// Adresy příjemců slepé kopie
				MailAddressCollection addrsBcc = new MailAddressCollection();
				if (String.IsNullOrWhiteSpace(mailBcc) == false)
				{
					string[] addrs = mailBcc.Split(delims);
					for (int i = 0; i < addrs.Length; i++)
					{
						addrsBcc.Add(new MailAddress(addrs[i]));
					}
				}

				retVal = Email.SendMail(mailSubject, mailBody, isBodyHtml, addrsTo, addrsCC, addrsBcc, attachments);
			}
			catch { }

			return retVal;
		}

		/// <summary>Odeslání mailu (email se neukládá do databáze)</summary>
		/// <param name="mailSubject">Předmět mailu.</param>
		/// <param name="mailBody">Tělo mailu.</param>
		/// <param name="isBodyHtml">Příznak, zda je tělo ve formátu html.</param>
		/// <param name="mailsTo">Adresy příjemců. Povinná je alespoň jeda adresa.</param>
		/// <param name="mailsCC">Adresy příjemců kopie.</param>
		/// <param name="mailsBcc">Adresy příjemců slepé kopie.</param>
		/// <param name="attachments">Kolekce příloh.</param>
		public static bool SendMail(string mailSubject, string mailBody, bool isBodyHtml, MailAddressCollection mailsTo, MailAddressCollection mailsCC, MailAddressCollection mailsBcc, List<Attachment> attachments)
		{
			if (mailsTo == null || mailsTo.Count <= 0) return false;
			char[] delims = { ';', ',' };

			bool retVal = false;
			try
			{

				string mailServer = BC.MailServer;
				if (String.IsNullOrWhiteSpace(mailServer) == false)
				{
					using (MailMessage mailMsg = new MailMessage(new MailAddress(BC.MailFrom), mailsTo[0]))
					{
						for (int i = 1; i < mailsTo.Count; i++)
						{
							mailMsg.To.Add(mailsTo[i]);
						}

						// Kopie mailu
						if (mailsCC != null)
						{
							for (int i = 0; i < mailsCC.Count; i++)
							{
								if (mailMsg.CC.IndexOf(mailsCC[i]) < 0)
									mailMsg.CC.Add(mailsCC[i]);
							}
						}

						// Slepé kopie mailu
						if (mailsBcc != null)
						{
							for (int i = 0; i < mailsBcc.Count; i++)
							{
								if (mailMsg.Bcc.IndexOf(mailsBcc[i]) < 0)
									mailMsg.Bcc.Add(mailsBcc[i]);
							}
						}

						mailMsg.IsBodyHtml = isBodyHtml;
						mailMsg.Subject = mailSubject;
						mailMsg.Body = mailBody;	//Replace(Environment.NewLine, "<br />")
						mailMsg.Priority = MailPriority.Normal;
						if (attachments != null && attachments.Count > 0)
						{
							foreach (var attm in attachments)
							{
								mailMsg.Attachments.Add(attm);
							}
						}

						SmtpClient smtp = new SmtpClient(mailServer);
						smtp.Send(mailMsg);
						retVal = true;
					}
				}
			}
			catch { }

			return retVal;
		}

		/// <summary>
		/// Odešle email a uloží ho do databáze
		/// <para>v případě požadavku 'odeslat pouze jednou' - se kontroluje (hashem těla mailu) existence emailu v databázi</para>
		/// </summary>
		/// <returns></returns>
		public bool SendMail()
		{
			bool result = false;

			if (String.IsNullOrEmpty(this.MailTo))
			{
				return result;
			}

			try
			{
				this.emailBodyHash = BC.CreateSHA256Hash(this.Body);

				if (this.SendOnlyOnce)
				{
					if (EmailCountByBodyHash(this.emailBodyHash) == 0)
					{
						result = Email.SendMail(this.Subject, this.Body, this.IsBodyHtml, this.MailTo, this.MailCC, this.MailBcc, null);
						this.writeIntoEmailSent();
					}
				}
				else
				{
					result = Email.SendMail(this.Subject, this.Body, this.IsBodyHtml, this.MailTo, this.MailCC, this.MailBcc, null);
					this.writeIntoEmailSent();
				}
			}
			catch (Exception)
			{ }

			return result;
		}

		/// <summary>
		/// Počet emailů v databázi (podle hashe těla emailu)		
		/// </summary>
		/// <returns></returns>
		public static int EmailCountByBodyHash(string bodyHash)
		{
			int resultValue = 0;

			using (var db = new FenixEntities())
			{
				try
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;

					ObjectParameter hashCount = new ObjectParameter("HashCount", typeof(int));
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));

					db.prEmailSentGetHashCount(bodyHash, hashCount, retVal, retMsg);

					if (retVal.Value.ToInt32(BC.NOT_OK) == BC.OK)
					{
						resultValue = hashCount.Value.ToInt32(0);
					}
				}
				catch
				{ }
			}

			return resultValue;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Zápis emailu do databáze
		/// </summary>
		private void writeIntoEmailSent()
		{
			try
			{
				using (var db = new FenixEntities())
				{
					db.Configuration.LazyLoadingEnabled = false;
					db.Configuration.ProxyCreationEnabled = false;
					ObjectParameter retVal = new ObjectParameter("ReturnValue", typeof(int));
					ObjectParameter retMsg = new ObjectParameter("ReturnMessage", typeof(string));
					
					db.prEmailSentWrite(this.Type, BC.MailFrom, this.MailTo, this.Subject, this.emailBodyHash, this.Body, this.EmbededPicture, BC.ServiceUserId, this.Source, this.IsInternal, retVal, retMsg);
				}
			}
			catch
			{ }
		}

		#endregion
	}
}
