using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meo.Web.ViewModels
{
    /// <summary>
    /// View-model posted from the POS new-sale screen.
    /// Not mapped to a DB table — used only as the action parameter.
    /// </summary>
    public class PosTransactionViewModel
    {
        [Required]
        public int CusId { get; set; }

        // ── Line items ───────────────────────────────────────────────────────
        public List<PosLineItemViewModel> Items { get; set; } = new List<PosLineItemViewModel>();

        // ── Treatment / package service entries ──────────────────────────────
        public List<PosServiceViewModel> Services { get; set; } = new List<PosServiceViewModel>();

        // ── Totals ───────────────────────────────────────────────────────────
        [Required]
        public decimal SubTotal { get; set; }

        public decimal DiscAmt { get; set; }

        public int? DiscPercentage { get; set; }

        [Required]
        public decimal TotalAmt { get; set; }

        // ── Payment ──────────────────────────────────────────────────────────
        [Required]
        public int PaymentMethodTypeId { get; set; }

        /// <summary>Display name e.g. "Cash", "GIRO", "Card", "PayNow".</summary>
        [Required]
        public string PaymentMethodName { get; set; }

        public string PaymentReferenceNo { get; set; }

        // ── Credit usage ─────────────────────────────────────────────────────
        public bool ApplyCredit { get; set; }

        public decimal CreditUsed { get; set; }

        public string Remarks { get; set; }
    }

    public class PosLineItemViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Qty { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal DiscAmt { get; set; }

        [Required]
        public decimal TotalAmt { get; set; }

        /// <summary>Staff member tagged to this line item (EmpNo).</summary>
        [Required]
        public string EmpNo { get; set; }

        public string Remarks { get; set; }
    }

    public class PosServiceViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SessionQty { get; set; }

        [Required]
        public string EmpNo { get; set; }
    }
}
