//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FenixAutomat
{
    using System;
    using System.Collections.Generic;
    
    public partial class InternalDocuments
    {
        public int ID { get; set; }
        public bool ItemVerKit { get; set; }
        public int ItemOrKitID { get; set; }
        public int ItemOrKitUnitOfMeasureId { get; set; }
        public Nullable<int> ItemOrKitQualityId { get; set; }
        public Nullable<decimal> ItemOrKitQuantityBefore { get; set; }
        public Nullable<decimal> ItemOrKitQuantityAfter { get; set; }
        public decimal ItemOrKitFreeBefore { get; set; }
        public decimal ItemOrKitFreeAfter { get; set; }
        public decimal ItemOrKitUnConsilliationBefore { get; set; }
        public decimal ItemOrKitUnConsilliationAfter { get; set; }
        public decimal ItemOrKitReservedBefore { get; set; }
        public decimal ItemOrKitReservedAfter { get; set; }
        public decimal ItemOrKitReleasedForExpeditionBefore { get; set; }
        public decimal ItemOrKitReleasedForExpeditionAfter { get; set; }
        public Nullable<decimal> ItemOrKitExpeditedBefore { get; set; }
        public Nullable<decimal> ItemOrKitExpeditedAfter { get; set; }
        public int StockId { get; set; }
        public int InternalDocumentsSourceId { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime ModifyDate { get; set; }
        public int ModifyUserId { get; set; }
    }
}
