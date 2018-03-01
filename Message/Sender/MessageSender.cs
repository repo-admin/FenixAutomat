using System;
using System.Text;
using System.Xml;
using FenixAutomat.Loggers;
using FenixAutomat.NDLInterfaces;
using FenixHelper;
using FenixAutomat.Message.Sender.Simulator;
using FenixAutomat.Message.Sender.WebService;

namespace FenixAutomat.Message.Sender
{
	/// <summary>
	/// Factory pro odeslání XML message do ND
	/// </summary>
	public abstract class MessageSender
	{
		/// <summary>
		/// Tovární metoda pro vytvoření požadované třídy
		/// </summary>
		/// <returns></returns>
		public static MessageSender CreateMessageSender()
		{
			if (BC.ProductionMode == 1)
				return new WebServiceMessageSender();
			
			return new SimulatorMessageSender();
		}

		/// <summary>
		/// Odeslání zprávy do ND		
		/// </summary>
		/// <param name="xmlMessage">XML message</param>
		/// <param name="messageType">typ message</param>
		/// <returns></returns>
		public abstract ReturnedValueFromND SendMessageToND(string xmlMessage, string messageType);

		/// <summary>
		/// Připraví XML zprávu před odesláním
		/// </summary>
		/// <param name="xmlSourceString"></param>
		/// <returns></returns>
		protected string PrepareXmlMessageBeforeSending(string xmlSourceString)
		{
			return xmlSourceString.Replace("\t", "  ");			
		}

		/// <summary>
		/// Vyhodnocení návratové hodnoty, kterou vrátilo ND po odeslání XML zprávy
		/// </summary>
		/// <param name="returnedValue">vrácená hodnota z ND</param>
		/// <returns></returns>
		protected int EvaluateReturnedValue(string returnedValue)
		{
			int res = BC.WRITE_TO_ND_NOT_OK;						
			XmlDocument xmlDocument = new XmlDocument();

			try
			{
				string xmlString = XmlCreator.CreateXMLRootNode(returnedValue);
				xmlDocument.LoadXml(xmlString);

				XmlNodeList messageType = xmlDocument.SelectNodes("//NewDataSet/Errors/Error");

				if (messageType == null || messageType.Count == 0)
				{
					res = BC.WRITE_TO_ND_OK;
				}
			}
			catch (XmlException)
			{
				// na vstupu NENI XML .. je to Exception Message
				;
			}
			catch (Exception ex)
			{
				Logger.ProcessError(ex, AppLog.GetMethodName(), BC.ServiceUserId);
			}

			return res;
		}
	}
}
