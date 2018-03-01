using System;

namespace FenixAutomat.Message.Sender.Simulator
{
	/// <summary>
	/// Simulované odeslání XML zprávy do ND
	/// </summary>
	public class SimulatorMessageSender : MessageSender
	{
		/// <summary>
		/// Simulace chyby
		/// </summary>
		private enum SimulateError
		{
			/// <summary>
			/// Je požadována simulace chyby (ND "přijalo" XML zprávu a vrátilo chybu)
			/// </summary>
			Yes = 0,

			/// <summary>
			/// NENÍ požadována simulace chyby (ND "přijalo" XML zprávu a vrátilo OK)
			/// </summary>
			No = 1
		}

		/// <summary>
		/// Simulované odeslání zprávy do ND
		/// </summary>
		/// <param name="xmlMessage">XML message</param>
		/// <param name="messageType">typ message</param>
		/// <returns></returns>
		public override ReturnedValueFromND SendMessageToND(string xmlMessage, string messageType)
		{
			ReturnedValueFromND returnedValueFromND = new ReturnedValueFromND();
						
			returnedValueFromND.Message = this.simulateSendToND(SimulateError.No);
			returnedValueFromND.Result = base.EvaluateReturnedValue(returnedValueFromND.Message);

			return returnedValueFromND;
		}

		/// <summary>
		/// Simulace odeslání XML message do ND		
		/// </summary>
		/// <param name="simulateError">požadavek na (ne)generovani chyby</param>
		/// <returns>XML bez chyby/s chybou</returns>
		private string simulateSendToND(SimulateError simulateError)
		{
			string result = String.Empty;

			if (simulateError == SimulateError.Yes)
			{
				//chyba
				result = @"<?xml version=""1.0"" encoding=""utf-8""?>
						<NewDataSet xmlns=""http://www.w3.org/2001/XMLSchema"">
						  <ID>1</ID>
						  <MessageID>38</MessageID>
						  <MessageType>6</MessageType>
						  <MessageTypeDescription>ShipmentOrder</MessageTypeDescription>
						  <Errors>
							<Error>
							  <ItemID>1500000003</ItemID>
							  <ErrorID>01</ErrorID>
							  <ErrorDescription>Quality is not valid</ErrorDescription>
							  <ErrorDescriptionComplt>Quality is not valid : 1 ZZZZ</ErrorDescriptionComplt>
							</Error>
						  </Errors>
						</NewDataSet>";
			}
			else if (simulateError == SimulateError.No)
			{
				//OK
				result = @"<?xml version=""1.0"" encoding=""utf-8""?>
						<NewDataSet xmlns=""http://www.w3.org/2001/XMLSchema"">
						  <ID>1</ID>
						  <MessageID>38</MessageID>
						  <MessageType>6</MessageType>
						  <MessageTypeDescription>ShipmentOrder</MessageTypeDescription>
						</NewDataSet>";
			}

			return result;
		}
	}
}
