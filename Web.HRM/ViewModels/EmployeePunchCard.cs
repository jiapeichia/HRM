using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meo.Web.ViewModels
{
    [Table("dbo.PunchCard")]
    public class PunchCardViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Punch Card ID")]
        public int PunchCardId { get; set; }

        [Required]
        [Display(Name = "Employee No")]
        public string EmpNo { get; set; }

        [Required]
        [Display(Name = "Date")]
        [Column(TypeName = "Date")]
        //[DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Clock In")]
        [DataType(DataType.Date)]
        public DateTime? ClockIn { get; set; }

        [Display(Name = "Clock Out")]
        [DataType(DataType.Date)]
        public DateTime? ClockOut { get; set; }

        [Display(Name = "Clock In")]
        [DataType(DataType.Date)]
        public DateTime? ClockIn2 { get; set; }

        [Display(Name = "Clock Out")]
        [DataType(DataType.Date)]
        public DateTime? ClockOut2 { get; set; }

        [Display(Name = "Clock In")]
        [DataType(DataType.Date)]
        public DateTime? ClockIn3 { get; set; }

        [Display(Name = "Clock Out")]
        [DataType(DataType.Date)]
        public DateTime? ClockOut3 { get; set; }
    }

    public class CombinedPunchCardReportViewModels
    {
        public string EmpNo { get; set; }
        public string EmployeeName { get; set; }
        public List<DateTime> Dates { get; set; }
    }

    public class PunchCardReportViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Punch Card ID")]
        public int PunchCardId { get; set; }

        [Display(Name = "Employee No")]
        public string EmpNo { get; set; }

        [Display(Name = "Date")]
        [Column(TypeName = "Date")]
        [DataType(DataType.Date)] 
        public DateTime Date { get; set; } 

        [Display(Name = "1st Clock In")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        public DateTime? ClockIn { get; set; }

        [Display(Name = "1st Clock Out")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        public DateTime? ClockOut { get; set; }

        [Display(Name = "2nd Clock In")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        [DataType(DataType.Date)]
        public DateTime? ClockIn2 { get; set; }

        [Display(Name = "2nd Clock Out")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        [DataType(DataType.Date)]
        public DateTime? ClockOut2 { get; set; }

        [Display(Name = "3rd Clock In")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        [DataType(DataType.Date)]
        public DateTime? ClockIn3 { get; set; }

        [Display(Name = "3rd Clock Out")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        [DataType(DataType.Date)]
        public DateTime? ClockOut3 { get; set; }
    }
}
