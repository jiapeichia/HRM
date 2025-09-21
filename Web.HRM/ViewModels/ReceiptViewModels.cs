using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meo.Web.ViewModels
{
    public class ReceiptModel
    {
        public string StoreName { get; set; }
        public string RegNo { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Tel { get; set; }
        public string Date { get; set; }
        public string SalesId { get; set; }
        public string CardNo { get; set; }
        public string PaymentMethod { get; set; }
        public int CRTotalAmount { get; set; } // for Credit
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalDisc { get; set; }
        public decimal SubTotal { get; set; }
        public string TotalBalanceString { get; set; }
        public decimal CreditBal { get; set; }
        public List<ReceiptItem> ReceiptItems { get; set; }

        public List<ReceiptAmountItem> GIROPaymentCounts { get; set; } = new List<ReceiptAmountItem>();

        public string ReceiptHeader { get; set; }
        public bool IsGIRO { get; set; }
    }

    public class ServiceReceipt
    {
        public string ReceiptHeader { get; set; }
        public string StoreName { get; set; }
        public string RegNo { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Tel { get; set; }
        public string Date { get; set; }
        public string SalesId { get; set; }
        public string CardNo { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalance { get; set; }
        public string TotalBalanceString { get; set; }
        public decimal CreditBal { get; set; }

        public int courseBal { get; set; }
        public ServiceHistory ServiceHistory { get; set; }

        public List<ReceiptAmountItem> GIROPaymentCounts { get; set; } = new List<ReceiptAmountItem>();

    }
    public class Receipt
    {
        [Display(Name = "Sales ID")]
        public string SalesId { get; set; }

        [Display(Name = "Cus ID")]
        public int CusId { get; set; }

        [Display(Name = "Payment Type")]
        public int PaymentMethodId { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? TotalAmt { get; set; }

        [Display(Name = "SubTotal")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? SubTotal { get; set; }

        [Display(Name = "Paid Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? PaidAmt { get; set; }

        [Display(Name = "Discount Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? DiscAmt { get; set; }

        [Display(Name = "Discount % Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? DiscPercentageAmt { get; set; }

        [Display(Name = "Balance Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? BalAmt { get; set; }

        [Display(Name = "Exchange")]
        public bool Exchange { get; set; }

        [Display(Name = "IsGIRO")]
        public bool IsGIRO { get; set; }

        public List<ReceiptItem> ReceiptItems { get; set; }
        public List<ReceiptAmountItem> GIROPaymentCounts { get; set; } = new List<ReceiptAmountItem>();
    }

    public class ReceiptItem
    {
        [Display(Name = "Product ID")]
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Qty")]
        public int Quantity { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "Line Discount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineDiscount { get; set; }
    }

    public class ReceiptAmountItem
    {
        public string SaledId { get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }
        public int PaidCount { get; set; }
        public decimal? TotalPaid { get; set; }
    }
}
