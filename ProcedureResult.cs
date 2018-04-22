using System.Runtime.Serialization;

namespace FenixAutomat
{
    /// <summary>
    /// Výsledek akce procedury
    /// </summary>
    [DataContract(Namespace = BC.APP_NAMESPACE)]
	public class ProcedureResult
	{
        /// <summary>
        /// Chyba
        /// </summary>
        [DataMember]
		public string Error { set; get; }

        /// <summary>
        /// Identita
        /// </summary>
	    [DataMember]
		public string Identity { set; get; }

        /// <summary>
        /// Duplicita
        /// </summary>
		[DataMember]
		public string Duplicity { set; get; }

        /// <summary>
        /// Popis stavu
        /// </summary>
	    [DataMember]
		public string StatusDesc { set; get; }
	}

	/// <summary>Výsledek akce procedury.</summary>
	[DataContract(Namespace = BC.APP_NAMESPACE)]
	public class ProcResult
	{
		/// <summary>Návratová hodnota</summary>
		[DataMember(IsRequired = true)]
		public int ReturnValue { set; get; }

		/// <summary>Návratová zpráva</summary>
		[DataMember]
		public string ReturnMessage { set; get; }
	}
}
