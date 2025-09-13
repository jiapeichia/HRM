using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    [Table("dbo.m_Stock")]
    public class Stock
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Qty Available")]
        public int? QtyAvailable { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

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

    [Table("dbo.m_StockReceive")]
    public class StockReceives
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "PO No")]
        public string PONo { get; set; }

        [Display(Name = "PO Date")]
        [DataType(DataType.Date)]
        public DateTime PODate { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [Display(Name = "Qty")]
        public int? Qty { get; set; }

        [Display(Name = "Total Discount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalDis { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

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

    [Table("dbo.m_ItemReceive")]
    public class ItemReceive
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "PO Id")]
        public int POId { get; set; }

        [Display(Name = "PO No")]
        public string PONo { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

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

    public class UpdateItemReceive
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "PO Id")]
        public int POId { get; set; }

        [Display(Name = "PO No")]
        public string PONo { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }

    public class PurchaseOrder
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "PO No")]
        public string PONo { get; set; }

        [Display(Name = "PO Date")]
        [DataType(DataType.Date)]
        public DateTime PODate { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [Display(Name = "Qty")]
        public int? Qty { get; set; }

        //[Display(Name = "Cost")]
        //[DisplayFormat(DataFormatString = "{0:N2}")]
        //public decimal Cost { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

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

        [Display(Name = "Qty")]
        public int totalQty { get; set; }

        [Display(Name = "Total Disc")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal totalDisc { get; set; }

        [Display(Name = "Sub Total")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal subtotal { get; set; }

        public List<UpdateItemReceive> ItemReceives { get; set; }
    }

    [Table("dbo.m_StockReturn")]
    public class StockReturns
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "CN No")]
        public string CNNo { get; set; }

        [Display(Name = "CN Date")]
        [DataType(DataType.Date)]
        public DateTime CNDate { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [Display(Name = "Qty")]
        public int? Qty { get; set; }

        [Display(Name = "LineDis")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalDisc { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

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

    [Table("dbo.m_ItemReturn")]
    public class ItemReturn
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "CN Id")]
        public int CNId { get; set; }

        [Display(Name = "CN No")]
        public string CNNo { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Cost")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Add By")]
        public string AddBy { get; set; }

        [Display(Name = "Mod By")]
        public string ModBy { get; set; }

        [Display(Name = "Add Date")]
        [DataType(DataType.Date)]
        public DateTime? AddDate { get; set; }
        
        [Display(Name = "Mod Date")]
        [DataType(DataType.Date)]
        public DateTime? ModDate { get; set; }
    }

    public class UpdateItemReturn
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "CN Id")]
        public int CNId { get; set; }

        [Display(Name = "CN No")]
        public string CNNo { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "LineDis")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineDiscAmt { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }
    public class CreditNote
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "CN No")]
        public string CNNo { get; set; }

        [Display(Name = "CN Date")]
        [DataType(DataType.Date)]
        public DateTime CNDate { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [Display(Name = "Qty")]
        public int? Qty { get; set; }

        [Display(Name = "TotalDisc")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalDisc { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

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

        [Display(Name = "Qty")]
        public int totalQty { get; set; }

        [Display(Name = "Total Disc")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal totalDisc { get; set; }

        [Display(Name = "Sub Total")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal subtotal { get; set; }

        public List<UpdateItemReturn> ItemReturns { get; set; }
    }

    [Table("dbo.Foc")]
    public class Foc
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Customer")]
        public int CusId { get; set; }

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
        public DateTime AddDate { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime ModDate { get; set; }
    }

    [Table("dbo.FocDetails")]
    public class FocDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int Id { get; set; }
        
        [Display(Name = "Foc ID")]

        public int FocId { get; set; }

        [Display(Name = "Employee")]
        public string EmpNo { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineTotal { get; set; }

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
        public DateTime AddDate { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime ModDate { get; set; }
    }

    public class FocDetailsUpdate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Foc ID")]

        public int FocId { get; set; }

        [Display(Name = "Employee")]
        public string EmpName { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Qty")]
        public int Qty { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Line Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal LineTotal { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }

    public class FocSalon //: Foc
    {
        public int totalQty { get; set; }
        public string cusName { get; set; }
        public int Id { get; set; }

        public int CusId { get; set; }

        public string Remarks { get; set; }

        public bool Active { get; set; }

        public bool Status { get; set; }

        public string AddBy { get; set; }

        public string ModBy { get; set; }

        [DataType(DataType.Date)]
        public DateTime AddDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ModDate { get; set; }
        public List<FocDetailsUpdate> FocDetails { get; set; }
    }
}
