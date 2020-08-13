using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyAddressBook
{
    public class ContactValidation
    {
        [Display(Name= "First Name")]
        [Required(ErrorMessage = "Please provide First Name", AllowEmptyStrings = false)]
        public string ContactPersonFname { get; set; }


        [Display(Name= "Last Name")] // It is not required
        public string ContactPersonLname { get; set; }

        [Display(Name= "Contact No1")]
        [Required(ErrorMessage = "Please provide First Name", AllowEmptyStrings = false)]
        public string ContactNo1 { get; set; }

        [Display(Name= "Contact No2")]
        public string ContactNo2 { get; set; }

        [Display(Name= "Email ID")]
        [RegularExpression(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", ErrorMessage = "Email not valid")]

        public string EmailID { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage = "Please Select Country")]
        public int CountryID { get; set; }

        [Display(Name = "State")]
        [Required(ErrorMessage = "Please Select State")]
        public int StateID { get; set; }
    }

    [MetadataType(typeof(ContactValidation))]   // Apply validation
    public partial class Contact
    {

    }
}