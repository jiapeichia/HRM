using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meo.Web.ViewModels
{
    public class SalesReportViewModels
    {
        [Display(Name = "ID")]
        public int SalesItemId { get; set; }

        [Display(Name = "Sales ID")]
        public string SalesId { get; set; }

        [Display(Name = "Employee No")]
        public string EmpNo { get; set; }

        [Display(Name = "PIC Name")]
        public string PICName { get; set; }

        [Display(Name = "Product ID")]
        public int ProductId { get; set; }

        [Display(Name = "Type")]
        public int TypeId { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "Discount Amt")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Price after Discount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmt { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Customer")]
        public int CusId { get; set; }

        [Display(Name = "Card No")]
        public string CardNo { get; set; }

        public string ImagePath { get; set; }
    }

    public class ServiceReportViewModels
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "SalesItem Id")]
        public int SalesItemId { get; set; }

        [Display(Name = "Invoice No")]
        public string SalesId { get; set; }

        [Display(Name = "CusId")]
        public int CusId { get; set; }

        [Display(Name = "Card No")]
        public string CardNo { get; set; }

        [Display(Name = "PICId")]
        public string PICId { get; set; }

        [Display(Name = "PIC Name")]
        public string PICName { get; set; }

        [Display(Name = "Service")]
        public string ServiceName { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string Remarks { get; set; }

        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        public string ImagePath { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "Total Discount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalDiscAmt { get; set; }

        [Display(Name = "Price after Discount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmt { get; set; }
    }

    public class ReportSearchContent
    {
        [Display(Name = "Emp No")]
        public string EmpNo { get; set; }

        public string reportType { get; set; }

        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "To")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        //public DateTime SalesDate { get; set; }
    }

    public class DailySalesReport
    {
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime SalesDate { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Total { get; set; }

        [Display(Name = "Paid by Cash")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal CashTotal { get; set; }

        [Display(Name = "Paid by Card")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal CardTotal { get; set; }

        [Display(Name = "Paid by Bank")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal BankTotal { get; set; }

        [Display(Name = "Paid by E-Wallet")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal EWalletTotal { get; set; }

        [Display(Name = "Paid by Others")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal OthersTotal { get; set; }

        public List<SalesViewModels> Sales { get; set; }
    }

}
