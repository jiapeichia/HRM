using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meo.Web.ViewModels
{
    // ── pos.Payment ──────────────────────────────────────────────────────────
    [Table("pos.Payment")]
    public class PosPaymentViewModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SalesId { get; set; }

        [Required]
        public int PaymentMethod { get; set; }   // m_Type.TypeId (Module='PaymentType')

        [Required]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string ReferenceNo { get; set; }  // card last-4, PayNow ref, cheque no, etc.

        [Required]
        [MaxLength(20)]
        public string CreatedBy { get; set; }    // EmpNo

        public DateTime CreatedDate { get; set; }
    }

    // ── pos.GiroBilling ──────────────────────────────────────────────────────
    [Table("pos.GiroBilling")]
    public class PosGiroBillingViewModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GiroId { get; set; }

        [Required]
        public int CusId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SalesId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Billing Month")]
        public DateTime BillingMonth { get; set; }  // store as first day of month

        [Required]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        [Display(Name = "Paid")]
        public bool IsPaid { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PaidDate { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        [Required]
        [MaxLength(20)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(20)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    // ── pos.AuditLog ─────────────────────────────────────────────────────────
    [Table("pos.AuditLog")]
    public class PosAuditLogViewModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; }

        [Required]
        [MaxLength(100)]
        public string RecordId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Action { get; set; }   // INSERT / UPDATE / VOID / DELETE

        public string OldValues { get; set; }  // JSON

        public string NewValues { get; set; }  // JSON

        [Required]
        [MaxLength(20)]
        public string EmpNo { get; set; }

        public DateTime ActionDate { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }
    }
}
