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
    
    public partial class cdlKits
    {
        public cdlKits()
        {
            this.cdlKitsItems = new HashSet<cdlKitsItems>();
        }
    
        public int ID { get; set; }
        public string Code { get; set; }
        public string DescriptionCz { get; set; }
        public string DescriptionEng { get; set; }
        public int MeasuresId { get; set; }
        public string MeasuresCode { get; set; }
        public Nullable<int> KitQualitiesId { get; set; }
        public string KitQualitiesCode { get; set; }
        public bool IsSent { get; set; }
        public Nullable<System.DateTime> SentDate { get; set; }
        public Nullable<int> GroupsId { get; set; }
        public Nullable<int> Packaging { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime ModifyDate { get; set; }
        public int ModifyUserId { get; set; }
        public int Multiplayer { get; set; }
    
        public virtual ICollection<cdlKitsItems> cdlKitsItems { get; set; }
    }
}
