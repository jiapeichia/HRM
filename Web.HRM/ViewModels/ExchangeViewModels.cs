using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    [Table("dbo.Exchange")]
    public class Exchange
    {
        public Exchange()
        {
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Sales Id")]
        public string SalesId { get; set; }   // new sales id

        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Product Type")]
        public string ProductType { get; set; }

        [Display(Name = "Qty")]
        public int? Quantity { get; set; }

        [Display(Name = "Selling Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? LineTotal { get; set; }

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

    public class ExchangableItem
    {
        [Display(Name = "Product Code")]
        public string Code { get; set; }

        [Display(Name = "Product Name")]
        public string Name { get; set; }
    }

    public class ExchangeItemViewModel
    {
        public List<ExchangeIn> itemsToIn { get; set; }
        public List<ExchangeOut> itemsToOut { get; set; }
        public string SalesId { get; set; }
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
    }

    public class ExchangeIn
    {
        public string InCode { get; set; }
        public int InQty { get; set; }
    }

    public class ExchangeOut
    {
        public string OutCode { get; set; }
        public int OutQty { get; set; }
    }
}
