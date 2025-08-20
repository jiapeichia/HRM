using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    [Table("dbo.CompanyProfile")]
    public class CompanyProfile
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Company Name")]
        public string Name { get; set; }

        [Display(Name = "1st Line Address")]
        public string Address1 { get; set; }

        [Display(Name = "2nd Line Address")]
        public string Address2 { get; set; }

        [Display(Name = "3rd Line Address")]
        public string Address3 { get; set; }

        [Display(Name = "Register No")]
        public string RegNo { get; set; }

        [Display(Name = "Contact No")]
        public string Tel { get; set; }

        [Display(Name = "Currency")]
        public string Currency { get; set; }

        [Display(Name = "Company Logo")]
        public string Logo { get; set; }
    }
}
