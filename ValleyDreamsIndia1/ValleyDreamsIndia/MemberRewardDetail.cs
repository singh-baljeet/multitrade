//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ValleyDreamsIndia
{
    using System;
    using System.Collections.Generic;
    
    public partial class MemberRewardDetail
    {
        public int Id { get; set; }
        public Nullable<int> LeftTeamCount { get; set; }
        public Nullable<int> RightTeamCount { get; set; }
        public Nullable<int> Pairs { get; set; }
        public string Status { get; set; }
        public string PaidStatus { get; set; }
        public Nullable<System.DateTime> PaidDate { get; set; }
        public string PaidRemarks { get; set; }
        public Nullable<int> UserDetailsId { get; set; }
        public Nullable<int> Deleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
    
        public virtual UsersDetail UsersDetail { get; set; }
    }
}