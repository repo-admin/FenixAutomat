﻿using System;
using Fenix;
using Fenix.Common;
using FenixAutomat.DeleteMessageEmail;
using FenixAutomat.Loggers;
using FenixAutomat.Message;

namespace FenixAutomat
{
    /// <summary>
    /// Hlavní aplikační část, tz. application entry stub
    /// </summary>
	class Program
	{
        /// <summary>
        /// Entry point aplikace
        /// </summary>
        /// <param name="args"></param>
		static void Main(string[] args)
		{
			if (ApplicationCanRun())
			{
				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, "Automat spuštěn...", ApplicationLog.GetMethodName(), BC.ServiceUserId);

				SendCdlMessagesToNd(BC.ServiceUserId);
				SendRksMessagesToNd(BC.ServiceUserId);

				if (BC.DeleteMessageViaXML == false)
				{
					SendDEmailMessagesToNd(BC.ServiceUserId);
					ReceiveDEmailMessagesFromNd(BC.ServiceUserId);
				}

				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, "Automat ukončen...", ApplicationLog.GetMethodName(), BC.ServiceUserId);				
			}
			else
			{
				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, "Automat má konfigurací zakázanou prováděcí část...", ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}

			Logger.WriteIntoLoggers("", String.Empty, String.Empty, BC.ServiceUserId);
		}


		/// <summary>
		/// Zjištění, zda je povolena prováděcí část programu (odesílání XML zpráv do ND)
		/// </summary>
		/// <returns></returns>
		private static bool ApplicationCanRun()
		{
			return BC.ApplicationCanRun == 1;
		}

		/// <summary>
		/// Vytvoření a odeslání XML zpráv pro items a kity
		/// </summary>
		/// <returns></returns>
		private static ResultAppService SendCdlMessagesToNd(int zicyzUserId)
		{
			ResultAppService result = new ResultAppService(BC.NOT_OK, BC.UNKNOWN);

			Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Aktualizace číselníků zahájena...", ApplicationLog.GetMethodName(), BC.ServiceUserId);

			try
			{
				CdlMessage cdlMessage = new CdlMessage(zicyzUserId);
				result = cdlMessage.CreateAndSendMessage();

				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, result.ResultMessage, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}

			Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Aktualizace číselníků ukončena...", ApplicationLog.GetMethodName(), BC.ServiceUserId);

			return result;
		}

		/// <summary>
		/// Vytvoření a odeslání XML zpráv pro recepci, kitting, shipment, repasi, delete (podléhá nastavení v konfigu !!!),
		/// CRM
		/// </summary>
		/// <returns></returns>
		private static ResultAppService SendRksMessagesToNd(int zicyzUserId)
		{
			ResultAppService result = new ResultAppService(BC.NOT_OK, BC.UNKNOWN);

			if (BC.DeleteMessageViaXML)
				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Odeslání zpráv(recepce, kitting, shipment, refurbished, delete, CRM) zahájeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);
			else
				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Odeslání zpráv(recepce, kitting, shipment, refurbished, CRM) zahájeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);

			try
			{
				RKSMessage rksMessage = new RKSMessage(zicyzUserId);
				result = rksMessage.CreateAndSendMessage();

				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, result.ResultMessage, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}

			if (BC.DeleteMessageViaXML)
				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Odeslání zpráv(recepce, kitting, shipment, refurbished, delete, CRM) ukončeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);
			else
				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Odeslání zpráv(recepce, kitting, shipment, refurbished, CRM) ukončeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);

			return result;
		}

		/// <summary>
		/// Vytvoření a odeslání emailů s požadavky na smazání messages na straně ND (D0 - DeleteMessage[Order])
		/// </summary>
		/// <returns></returns>
		private static void SendDEmailMessagesToNd(int zicyzUserId)
		{
			ResultAppService result = new ResultAppService(BC.NOT_OK, BC.UNKNOWN);

			Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Odeslání emailů(delete email message order) zahájeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);

			try
			{
				D0DeleteMessageEmail d0DeleteMessageEmail = new D0DeleteMessageEmail(zicyzUserId);
				result = d0DeleteMessageEmail.CreateAndSendEmails();
				
				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, result.ResultMessage, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}

			Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Odeslání emailů(delete email message order) ukončeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);
		}

		/// <summary>
		/// Zpracování přijatých emailů s Fenix požadavky na smazání messages na straně ND (odesílatelem je ND/XPO)
		/// </summary>
		/// <param name="zicyzUserId"></param>
		private static void ReceiveDEmailMessagesFromNd(int zicyzUserId)
		{
			ResultAppService result = new ResultAppService(BC.NOT_OK, BC.UNKNOWN);

			Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Zpracování přijatých emailů(delete email message order) zahájeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);

			try
			{
				D0DeleteMessageEmailReceive d0DeleteMessageEmailReceive = new D0DeleteMessageEmailReceive(zicyzUserId); 				
				result = d0DeleteMessageEmailReceive.ReceiveAndProcessEmails();

				Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, result.ResultMessage, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, ApplicationLog.GetMethodName(), BC.ServiceUserId);
			}

			Logger.WriteIntoLoggers(ApplicationLog.LogCategoryInfo, true, "Zpracování přijatých emailů(delete email message order) ukončeno...", ApplicationLog.GetMethodName(), BC.ServiceUserId);
		}
	}
}
