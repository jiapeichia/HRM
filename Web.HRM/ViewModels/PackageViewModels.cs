using Meo.Web.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.UI;

namespace Meo.Web.ViewModels
{
    [Table("dbo.Package")]
    public class PackageViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int ProductType { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Package Code")]
        public string Code { get; set; }

        //[RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Only numeric values and decimal points are allowed.")]
        //[Display(Name = "Total Cost")]
        //[Range(typeof(decimal), "0.00", "99999999.00")]
        //[DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Selling Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalCost { get; set; }

        //[RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Only numeric values and decimal points are allowed.")]
        //[Display(Name = "Selling Price")]
        //[Range(typeof(decimal), "0.00", "99999999.00")]
        //[DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]

        [Display(Name = "Selling Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

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
        public DateTime ModDate { get; set; }

        //[Display(Name = "Use credit to buy?")]
        //public bool CreditBuy { get; set; }

        [Display(Name = "Expiry Duration")]
        public int? ExpiryPeriod { get; set; }

        [Display(Name = "Allow GIRO buy?")]
        public bool GIROBuy { get; set; }

        [Display(Name = "Minimum 1st Time Payment Amount")]
        public int FirstPayAmt { get; set; }

        [Display(Name = "Special Pay")]
        public bool SpecialPay { get; set; }
    }

    [Table("dbo.PackageDetails")]
    public class PackageDetailsViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Package #ID")]
        public int PackageId { get; set; }

        [Display(Name = "ItemId")]
        public int ItemId { get; set; }

        [Display(Name = "ItemType")]
        public int ItemType { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Cost { get; set; }

        [Display(Name = "Total Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalCost { get; set; }

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
        public DateTime ModDate { get; set; }
    }

    [Table("dbo.PackageSold")]
    public class PackageSoldViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "CusId")]
        public int CusId { get; set; }

        [Display(Name = "Package Id")]
        public int PackageId { get; set; }

        [Display(Name = "Package Code")]
        public string PackageCode { get; set; }

        [Display(Name = "Package Type")]
        public int PackageType { get; set; }

        [Display(Name = "Total Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalCost { get; set; }

        [Display(Name = "Selling Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "PackageRemarks")]
        public string PackageRemarks { get; set; }

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
        //[DataType(DataType.Date)]
        public DateTime ModDate { get; set; }

        [Display(Name = "Expiry Duration")]
        public int? ExpiryPeriod { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }
    }

    [Table("dbo.PackageSoldDetails")]
    public class PackageSoldDetailsViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "PackageSoldId")]
        public int PackageSoldId { get; set; }

        [Display(Name = "ItemId")]
        public int ItemId { get; set; }

        [Display(Name = "ItemType")]
        public int ItemType { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Cost { get; set; }

        [Display(Name = "Total Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalCost { get; set; }

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
        public DateTime ModDate { get; set; }
    }
}
