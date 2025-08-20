using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Meo.Web.ViewModels
{
     [Table("dbo.Employee")]
    public class EmployeeViewModels
    {
        public EmployeeViewModels()
        {
            this.Emp_id = Guid.NewGuid();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Employee ID")]
        public Guid Emp_id { get; set; }

        [Display(Name = "Role ID")]
        public int RoleId { get; set; }

        //[UIHint("FileUpload")]
        [Display(Name = "Upload Image")]
        public string ImagePath { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Employee No")]
        public string EmpNo { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        //[Required]
        [Display(Name = "Identity Card No")]
        public string IcNo { get; set; }

        [Display(Name = "Hired Date")]
        [DataType(DataType.Date)]
        public DateTime ? HiredDate { get; set; }

        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Remote("IsUserEmailAvailable", "Employee", HttpMethod = "POST", ErrorMessage = "Email already in use")]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not valid")]
        [Display(Name = "Email(O)")]
        public string OEmail { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Confirmation Date")]
        [DataType(DataType.Date)]
        public DateTime ? ConfirmationDate { get; set; }

        [Display(Name = "Resignation Date")]
        [DataType(DataType.Date)]
        public DateTime ? ResignationDate { get; set; }

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
        public DateTime ? AddDate { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime ? ModDate { get; set; }

        //[DataType(DataType.Password)]
        //public string Password { get; set; }
    }

    public class EmployeeDetailsViewModel
    {
        [Display(Name = "Employee ID")]
        public Guid Emp_id { get; set; }

        [Display(Name = "Role ID")]
        public int RoleId { get; set; }

        //[UIHint("FileUpload")]
        [Display(Name = "Upload Image")]
        public string ImagePath { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Employee No")]
        public string EmpNo { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        //[Required]
        [Display(Name = "Identity Card No")]
        public string IcNo { get; set; }

        //[Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hired Date")]
        public DateTime ? HiredDate { get; set; }

        //[Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Remote("IsUserEmailAvailable", "Employee", HttpMethod = "POST", ErrorMessage = "Email already in use")]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not valid")]
        [Display(Name = "Email(O)")]
        public string OEmail { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Confirmation Date")]
        [DataType(DataType.Date)]
        public DateTime? ConfirmationDate { get; set; }

        [Display(Name = "Resignation Date")]
        [DataType(DataType.Date)]
        public DateTime? ResignationDate { get; set; }

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
}
