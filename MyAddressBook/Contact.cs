//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyAddressBook
{
    using System;
    using System.Collections.Generic;
    
    public partial class Contact
    {
        public int ContactID { get; set; }
        public string ContactPersonFname { get; set; }
        public string ContactPersonLname { get; set; }
        public string ContactNo1 { get; set; }
        public string ContactNo2 { get; set; }
        public string EmailID { get; set; }
        public int CountryID { get; set; }
        public int StateID { get; set; }
        public string Address { get; set; }
        public string ImagePath { get; set; }
    
        public virtual Country Country { get; set; }
        public virtual State State { get; set; }
    }
}