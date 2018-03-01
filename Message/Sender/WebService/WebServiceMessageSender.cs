using System;
using System.Text;
using FenixAutomat.NDLInterfaces;
using FenixHelper;

namespace FenixAutomat.Message.Sender.WebService
{
	/// <summary>
	/// Odeslání XML zprávy do ND
	/// </summary>
	public class WebServiceMessageSender : MessageSender
	{
		/// <summary>
		/// Odeslání zprávy do ND		
		/// </summary>
		/// <param name="xmlMessage">XML message</param>
		/// <param name="messageType">typ message</param>
		/// <returns></returns>
		public override ReturnedValueFromND SendMessageToND(string xmlMessage, string messageType)
		{
			ReturnedValueFromND returnedValueFromND = new ReturnedValueFromND();

			string preparedXmlMessage = base.PrepareXmlMessageBeforeSending(xmlMessage);						
			returnedValueFromND.Message = this.sendToND(preparedXmlMessage, messageType);
			returnedValueFromND.Result = base.EvaluateReturnedValue(returnedValueFromND.Message);

			return returnedValueFromND;
		}

		/// <summary>
		/// Vlastní odeslání zprávy do ND (volá se web service na straně ND)
		/// </summary>
		/// <param name="sourceXML"></param>
		/// <param name="messageType"></param>
		/// <returns></returns>
		private string sendToND(string sourceXML, string messageType)
		{
			byte[] ndAnswer;
			try
			{
				byte[] image = sourceXML.ToArray(Encoding.UTF8);

				PortTypeNDL_InterfacesClient client = new PortTypeNDL_InterfacesClient();
				client.InnerChannel.OperationTimeout = new TimeSpan(0, 20, 0);

				ndAnswer = client.UPWSI0(BC.Login, BC.Password, BC.PartnerCode, messageType, BC.Encoding, image);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

			return ndAnswer.ToString(Encoding.UTF8, Encoding.Unicode);
		}
	}
}
