using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    public class Customer
    {
        public int CusId { get; set; }
        public string CardNo { get; set; }
        public string FullName { get; set; }
        public decimal CreditBal { get; set; }
    }

    [Table("dbo.Customer")]
    public class CustomerViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Customer ID")]
        public int CusId { get; set; }

        [Required]
        [Display(Name = "Card No")]
        public string CardNo { get; set; }

        //[UIHint("FileUpload")]
        [Display(Name = "Upload Image")]
        public string ImagePath { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        //[Required]
        [Display(Name = "Identity Card (IC)")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numeric allowed.")]
        public string IcNo { get; set; }

        [Required]
        [Display(Name = "Contact No")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numeric allowed.")]
        public string ContactNo { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Add By")]
        public string AddBy { get; set; }

        [Display(Name = "Update By")]
        public string ModBy { get; set; }

        [Display(Name = "Add Date")]
        [DataType(DataType.Date)]
        public DateTime? AddDate { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime? ModDate { get; set; }


        [Display(Name = "Credit")]
        public decimal CreditBal { get; set; }

        [Display(Name = "TopUp Due Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? TPDueAmt { get; set; }

        [Display(Name = "Service Due Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? SVDueAmt { get; set; }
    }

    public class CustomerDetailsViewModel
    {
        [Display(Name = "Customer ID")]
        public int CusId { get; set; }

        [Required]
        [Display(Name = "Card No")]
        public string CardNo { get; set; }

        //[UIHint("FileUpload")]
        [Display(Name = "Upload Image")]
        public string ImagePath { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Identity Card (IC)")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numeric allowed.")]
        public string IcNo { get; set; }

        [Required]
        [Display(Name = "Contact No")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numeric allowed.")]
        public string ContactNo { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Add By")]
        public string AddBy { get; set; }

        [Display(Name = "Update By")]
        public string ModBy { get; set; }

        [Display(Name = "Add Date")]
        [DataType(DataType.Date)]
        public DateTime? AddDate { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime? ModDate { get; set; }


        [Display(Name = "Credit")]
        public decimal CreditBal { get; set; }

        [Display(Name = "Top Up Due Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? TPDueAmt { get; set; }

        [Display(Name = "Service Due Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? SVDueAmt { get; set; }
    }

    public class NewCustomer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Customer ID")]
        public int CusId { get; set; }

        [Required]
        [Display(Name = "Card No")]
        public string CardNo { get; set; }

        //[UIHint("FileUpload")]
        [Display(Name = "Upload Image")]
        public string ImagePath { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Identity Card (IC)")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numeric allowed.")]
        public string IcNo { get; set; }

        [Required]
        [Display(Name = "Contact No")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Only numeric allowed.")]
        public string ContactNo { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Add By")]
        public string AddBy { get; set; }

        [Display(Name = "Update By")]
        public string ModBy { get; set; }

        [Display(Name = "Add Date")]
        [DataType(DataType.Date)]
        public DateTime? AddDate { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime? ModDate { get; set; }

        [Display(Name = "Credit")]
        public decimal CreditBal { get; set; }
    }

    public class ExpiringListModel
    {
        [Display(Name = "Customer ID")]
        public int CusId { get; set; }

        [Display(Name = "Card No")]
        public string CardNo { get; set; }

        //[UIHint("FileUpload")]
        [Display(Name = "Upload Image")]
        public string ImagePath { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Invoice")]
        public string SalesId { get; set; }

        [Display(Name = "Service")]
        public string Service { get; set; }

        [Display(Name = "Course Balance")]
        public int CourseBal { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }
    }
}
