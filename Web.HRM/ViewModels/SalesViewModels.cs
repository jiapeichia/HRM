using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    [Table("dbo.s_Sales")]
    public class SalesViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Customer ID")]
        public int CusId { get; set; }

        [Display(Name = "Payment Method")]
        public int PaymentMethod { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "SubTotal")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? SubTotal { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? TotalAmt { get; set; } 

        [Display(Name = "Paid Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? PaidAmt { get; set; }

        [Display(Name = "Discount Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? DiscAmt { get; set; }

        [Display(Name = "Discount %")]
        public int? DiscPercentage { get; set; }

        [Display(Name = "Discount % Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? DiscPercentageAmt { get; set; }

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

        [Display(Name = "GIRO")]
        public bool GIRO { get; set; }

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

        [Display(Name = "Due Invoice No")]
        public string DueInvoice { get; set; }

        public List<SalesItemViewModels> SalesDetails { get; set; }
    }

    [Table("dbo.s_SalesItem")]
    public class SalesItemViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int SalesItemId { get; set; }

        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Display(Name = "Employee ID")]
        public string EmpNo { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Type")]
        public int TypeId { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Qty Balance")]
        public int? QtyBalance { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineTotal { get; set; }

        [Display(Name = "Discount Amt")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineDiscAmt { get; set; }

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

        [NotMapped]
        [Display(Name = "Expiry Duration")]
        public int? ExpiryPeriod { get; set; }

        [NotMapped]
        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Pay Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? PayAmt { get; set; }

        [Display(Name = "Backordered")]
        public bool IsBackordered { get; set; } = false;
    }

    public class SalesInvoiceViewModels
    {
        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Display(Name = "Credit")]
        public decimal CreditBal { get; set; }

        [Required]
        [Display(Name = "Customer ID")]
        public int CusId { get; set; } 

        [Display(Name = "Customer Name")]
        public string CusName { get; set; }

        [Display(Name = "Card No")]
        public string CardNo { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethodName { get; set; }

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

        [Display(Name = "Pay Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? PayAmt { get; set; }

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
        
        public int totalQty { get; set; }

        public decimal totalDisc { get; set; }

        public decimal subtotal { get; set; }

        public decimal? totalPayAmt { get; set; }

        public decimal? totalOutStanding { get; set; }

        public string dueInvoice { get; set; }

        public List<SalesItemUpdate> SalesDetails { get; set; }
        public List<SalesItemUpdate> ExchangeDetails { get; set; }
        public List<ReceiptAmountItem> GIROPaymentCounts { get; set; } = new List<ReceiptAmountItem>(); // GIRO Rereipt
    }

    public class SalesItemUpdate
    {
        [Display(Name = "ID")]
        public int SalesItemId { get; set; }

        [Display(Name = "Sales ID")]
        public string SalesId { get; set; }

        [Display(Name = "Employee Name")]
        public string EmpName { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineTotal { get; set; }

        [Display(Name = "Discount Amt")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Pay Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? LinePayAmt { get; set; }

        [Display(Name = "Outstanding Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? LineOutstandingAmt { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Exchange")]
        public bool Exchange { get; set; }
    }

    public class PaymentViewModels
    {
        [Required]
        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }
        public string OldSalesId { get; set; }

        [Display(Name = "Customer ID")]
        public int CusId { get; set; }

        [Display(Name = "Payment Method")]
        public int PaymentMethod { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? TotalAmt { get; set; }

        [Display(Name = "SubTotal")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? SubTotal { get; set; }

        [Required]
        [Display(Name = "Paid Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? PaidAmt { get; set; }

        [Display(Name = "Discount %")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? DiscPercentageAmt { get; set; }

        [Display(Name = "Discount %")]
        public int? DiscPercentage { get; set; }

        [Display(Name = "Discount Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? DiscAmt { get; set; }

        [Display(Name = "Balance Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? BalAmt { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Exchange")]
        public bool Exchange { get; set; }

        public bool IsTopUp { get; set; } = false;
        public bool IsGIRO { get; set; } = false;
    }

    public class UpdateSales
    {
        [Display(Name = "ID")]
        public int SalesItemId { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineTotal { get; set; }

        [Display(Name = "Discount Amt")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Total Amount")]
        public decimal? TotalAmt { get; set; }
    }

    public class AddSalesItem
    {
        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Display(Name = "ID")]
        public int SalesItemId { get; set; }

        [Display(Name = "Product ID")]
        public int ProductId { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineTotal { get; set; }

        [Display(Name = "Discount Amt")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Total Amount")]
        public decimal? TotalAmt { get; set; }

        [Display(Name = "Employee ID")]
        public string EmpNo { get; set; }

        [Display(Name = "Exchange")]
        public bool Exchange { get; set; }
    }

    [Table("dbo.s_Service")]
    public class Service
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "SalesItem Id")]
        public int SalesItemId { get; set; }

        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Display(Name = "CusId")]
        public int CusId { get; set; }

        [Display(Name = "Service")]
        public string ServiceName { get; set; }

        [Display(Name = "Course")]
        public int? Course { get; set; }

        [Display(Name = "Course left")]
        public int? CourseBal { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string Remarks { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime PurchaseDate { get; set; }

        [Display(Name = "Expiry Duration")]
        public int? ExpiryPeriod { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Due Flag")]
        public bool DueFlag { get; set; }

        [Display(Name = "Free Service")]
        public bool FreeService { get; set; }
    }

    [Table("dbo.s_ServiceHistory")]
    public class ServiceHistory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "SalesItem Id")]
        public int SalesItemId { get; set; }

        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Display(Name = "CusId")]
        public int CusId { get; set; }

        [Display(Name = "PICId")]
        public string PICId { get; set; }

        [Display(Name = "PIC")]
        public string PICName { get; set; }

        [Display(Name = "Service")]
        public string ServiceName { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string Remarks { get; set; }

        [Display(Name = "Service Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTime ServiceDate { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }
    }
}
