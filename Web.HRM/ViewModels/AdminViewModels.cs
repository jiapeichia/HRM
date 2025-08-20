using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Security;

namespace Meo.Web.ViewModels
{
    [Table("dbo.m_Type")]
    public class TypeViewModels
    {
        public TypeViewModels()
        {
        }

        public TypeViewModels(string type) : this()
        {
            TypeName = type;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "ID")]
        public int TypeId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Name")]
        public string TypeName { get; set; }

        [Display(Name = "Module")]
        public string Module { get; set; }

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
        public DateTime? ModDate { get; set; }

        public string InvFormat { get; set; }
    }

    public class PageAccessUpdateModel
    {
        public List<PageAccess> PageAccess { get; set; }
        public int SelectedRoleId { get; set; }
    }

    public class PageAccess
    {
        public int PageId { get; set; }

        public bool Active { get; set; }
    }

    public class AccessControl
    {
        public int PageAccess_id { get; set; }

        public string Emp_no { get; set; }

        public int PageId { get; set; }

        public string PageName { get; set; }

        public bool Active { get; set; }
    }

        [Table("dbo.PageAccess")]
    public class PageAccessViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int PageAccess_id { get; set; }

        [Display(Name = "User ID")]
        public string Emp_no { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Page ID")]
        public int PageId { get; set; }

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
        public DateTime ModDate { get; set; }
    }

    [Table("dbo.Page")]
    public class PageViewModels
    {
        public PageViewModels()
        {
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "#ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Button")]
        public string Name { get; set; }

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

    [Table("dbo.Role")]
    public class RoleViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Role ID")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Role")]
        public string RoleName { get; set; }

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
        public DateTime ModDate { get; set; }
    }

    [Table("dbo.Counter")]
    public class CounterViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Counter ID")]
        public int count_id { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Table Name")]
        public string count_name { get; set; }

        [Display(Name = "Counter No")]
        public int count_no { get; set; }

        [Display(Name = "Module")]
        public string module { get; set; }

        public string format { get; set; }
        public int year { get; set; }
    }

    [Table("dbo.AspUser")]
    public class AspUserViewModels
    {
         [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "User ID")]
        public string user_id { get; set; }

       // [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string password { get; set; }

        [Display(Name = "Email")]
        public string email { get; set; }

        [Display(Name = "User Name")]
        public string username { get; set; }

        [Display(Name = "Employee No")]
        public string emp_no { get; set; }

        [Display(Name = "Activate")]
        public bool active { get; set; }

        [Display(Name = "Status")]
        public bool status { get; set; }

        [Display(Name = "Add By")]
        public string add_by { get; set; }

        [Display(Name = "Update By")]
        public string mod_by { get; set; }

        [Display(Name = "Add Date")]
        [DataType(DataType.Date)]
        public DateTime? add_date { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime mod_date { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [MembershipPassword(MinRequiredNonAlphanumericCharacters = 1,
            MinNonAlphanumericCharactersError = "Your password needs to contain at least one symbol (!, @, #, etc).",
            ErrorMessage = "Your password must be 6 characters long and contain at least one symbol (!, @, #, etc).",
            MinRequiredPasswordLength = 6
        )]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Confirm password does not match with the new password.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgetPasswordViewModels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "ForgetPassword ID")]
        public Guid ForgetPassword_id { get; set; }

        [Display(Name = "Emp ID")]
        public Guid Emp_id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Add By")]
        public string Add_by { get; set; }

        [Display(Name = "Update By")]
        public string Mod_by { get; set; }

        [Display(Name = "Add Date")]
        [DataType(DataType.Date)]
        public DateTime? Add_date { get; set; }

        [Display(Name = "Last Update")]
        [DataType(DataType.Date)]
        public DateTime Mod_date { get; set; }
    }
}
