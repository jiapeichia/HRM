using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    public class GIROViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Customer ID")]
        public int CusId { get; set; }

        [Display(Name = "Customer Name")]
        public string CusName { get; set; }

        [Display(Name = "Payment Method")]
        public int PaymentMethod { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? TotalAmt { get; set; } 

        [Display(Name = "Paid Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? PaidAmt { get; set; }

        [Display(Name = "Discount Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? DiscAmt { get; set; }

        [Display(Name = "Balance Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? BalAmt { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Exchange")]
        public bool Exchange { get; set; }

        [Display(Name = "Add By")]
        public string AddBy { get; set; }

        [Display(Name = "Mod By")]
        public string ModBy { get; set; }

        [Display(Name = "Add Date")]
        [DataType(DataType.Date)]
        public DateTime? AddDate { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime ModDate { get; set; }

        public List<SalesItemViewModels> SalesDetails { get; set; }

        public bool IsTopUp { get; set; } = false;
    }
    
}
