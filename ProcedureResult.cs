using System.Runtime.Serialization;

namespace FenixAutomat
{
	/// <summary>Výsledek akce procedury.</summary>
	[DataContract(Namespace = BC.APP_NAMESPACE)]
	public class ProcedureResult
	{
		/// <summary></summary>
		[DataMember]
		public string Error { set; get; }
		[DataMember]

		/// <summary></summary>
		public string Identity { set; get; }
		[DataMember]

		/// <summary></summary>
		public string Duplicity { set; get; }
		[DataMember]

		/// <summary></summary>
		public string StatusDesc { set; get; }
	}

	/// <summary>Výsledek akce procedury.</summary>
	[DataContract(Namespace = BC.APP_NAMESPACE)]
	public class ProcResult
	{
		/// <summary></summary>
		[DataMember(IsRequired = true)]
		public int ReturnValue { set; get; }

		/// <summary></summary>
		[DataMember]
		public string ReturnMessage { set; get; }
	}
}
