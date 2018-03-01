using System;

namespace FenixAutomat.Message.Sender
{
	/// <summary>
	/// Návratová hodnota odeslání XML zprávy do ND
	/// </summary>
	public class ReturnedValueFromND
	{
		private bool isOK;

		/// <summary>
		/// Originální návratová hodnota (XML zpráva)
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Vyhodnocená návratová hodnota
		/// </summary>
		public int Result { get; set; }	
	
		/// <summary>
		/// Vyhodnocení návratové hodnoty
		/// </summary>
		public bool ReturnedValueIsOK 
		{
			get
			{ 
				return this.Result == BC.WRITE_TO_ND_OK; 
			}
			private set
			{
				this.isOK = value;
			}
		}

		/// <summary>
		/// ctor
		/// </summary>
		public ReturnedValueFromND()
		{
			this.Message = String.Empty;
			this.Result = BC.WRITE_TO_ND_NOT_OK;			
			this.ReturnedValueIsOK = false;			
		}
	}
}
