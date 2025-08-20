using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    [Table("dbo.m_Supplier")]
    public class SupplierViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Supplier ID")]
        public string SupplierId { get; set; }

        [Display(Name = "Company Name")]
        public string Name { get; set; }

        [Display(Name = "Contact No")]
        public string ContactNo { get; set; }

        [Display(Name = "Contact No.2")]
        public string ContactNo2 { get; set; }

        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

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
    }

    [Table("dbo.m_Product")]
    public class ProductViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "ID")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Code")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Description")]
        public string ProductName { get; set; }

        [Display(Name = "Category")] // Product or service
        public int TypeId { get; set; }

        [Display(Name = "Cost")]
        [Range(typeof(decimal), "0.00", "99999999.00")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Cost { get; set; }

        [Display(Name = "Selling Price")]
        [Range(typeof(decimal), "0.00", "99999999.00")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Display(Name = "Course")]
        public int? Unit { get; set; }

        [Display(Name = "Credit")]
        public int? Credit { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

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

        //[Display(Name = "Use credit to buy?")]
        //public bool CreditBuy { get; set; }
    }

    public class ProductGIRO
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Code")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Description")]
        public string ProductName { get; set; }

        [Display(Name = "Category")] // Product or service
        public string TypeName { get; set; }

        [Display(Name = "Cost")]
        [Range(typeof(decimal), "0.00", "99999999.00")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Cost { get; set; }

        [Display(Name = "Selling Price")]
        [Range(typeof(decimal), "0.00", "99999999.00")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        public int QtyAvailable { get; set; }

        //[Display(Name = "Use credit to buy?")]
        //public bool CreditBuy { get; set; }

        [Display(Name = "Allow GIRO?")]
        public bool GIROBuy { get; set; }

        [Display(Name = "1st Payment Amount")]
        public int FirstPayAmt { get; set; }
    }

    public class TopUp
    {
        [Display(Name = "ID")]   
        public int ProductId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Code")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Description")]
        public string ProductName { get; set; }

        [Display(Name = "Category")] // Product or service
        public int TypeId { get; set; }

        [Display(Name = "Cost")]
        [Range(typeof(decimal), "0.00", "99999999.00")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Cost { get; set; }

        [Display(Name = "Selling Price")]
        [Range(typeof(decimal), "1.00", "99999999.00")]
        [Required(ErrorMessage = "Selling Price value must be at least 1.")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Display(Name = "Course")]
        public int? Unit { get; set; }

        [Required]
        [Display(Name = "Credit")]
        public int? Credit { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

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

        //[Display(Name = "Use credit to buy?")]
        //public bool CreditBuy { get; set; }
    }

    public class ServiceVM
    {
        [Display(Name = "ID")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Code")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Description")]
        public string ProductName { get; set; }

        [Display(Name = "Category")] // Product or service
        public int TypeId { get; set; }

        [Display(Name = "Cost")]
        [Range(typeof(decimal), "0.00", "99999999.00")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Cost { get; set; }

        [Display(Name = "Selling Price")]
        [Range(typeof(decimal), "1.00", "99999999.00")]
        [Required(ErrorMessage = "Selling Price value must be at least 1.")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Course value must be at least 1")]
        [Display(Name = "Course")]
        public int? Unit { get; set; }

        [Display(Name = "Credit")]
        public int? Credit { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

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

        //[Display(Name = "Use credit to buy?")]
        //public bool CreditBuy { get; set; }
    }

    [Table("dbo.BackupPath")]
    public class DBBackupViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Folder Path")]
        public string Path { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }
    }
}
