using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Meo.Web.Model
{
    public class UserLoginModel
    {
        #region Constructor
        public UserLoginModel() { }
        #endregion

        #region Const Column Name
        private const string CN_emp_no = "emp_no";
        private const string CN_email = "email";
        private const string CN_username = "username";
        private const string CN_Password = "Password";
        private const string CN_OldPassword = "OldPassword";
        private const string CN_NewPassword = "NewPassword";
        private const string CN_create_by = "create_by";
        private const string CN_update_date = "update_date";
        #endregion

        #region Properties
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string emp_no { get; set; }
        public string email { get; set; }
        public string username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string create_by { get; set; }
        public DateTime update_date { get; set; }
        #endregion

        public static DataTable LoadApsUserLogin(string username, string password)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = DAC.ExecuteDataTable("LoadApsUserLogin",
                    DAC.Parameter("USERNAME", username.ToUpper()),
                    //DAC.Parameter("EMAIL", email),
                    DAC.Parameter("PASSWORD", password));

                return dt;
            }
            catch (Exception ex)
            {
                // error_page
                throw new Exception(ex.ToString());
            }
        }

        //public static DataTable SP_HR_UserLogin(string email, string password)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt = DAC.ExecuteDataTable("SP_HR_UserLogin",
        //            DAC.Parameter("EMAIL", email),
        //            DAC.Parameter("PASSWORD", password));

        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        // error_page
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public static DataTable SP_HR_LoadOrgChart(string empid)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt = DAC.ExecuteDataTable("SP_HR_LoadOrgChart",
        //           DAC.Parameter("EMPID", empid));
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public static DataTable SP_HR_LoadGoalCategory()
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt = DAC.ExecuteDataTable("SP_HR_LoadGoalCategory");
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public static DataTable SP_HR_LoadHeader(string empid)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt = DAC.ExecuteDataTable("SP_HR_LoadHeader",
        //           DAC.Parameter("EMPID", empid));
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public static DataTable SP_HR_LoadTaskSummary(string performanceid)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt = DAC.ExecuteDataTable("SP_HR_LoadTaskSummary",
        //           DAC.Parameter("@PERFORMANCEID", performanceid));
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public static DataTable SP_HR_LoadCoreValueCategory()
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt = DAC.ExecuteDataTable("SP_HR_LoadCoreValueCategory");
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}
    }
}
