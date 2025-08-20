using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meo.Web.Model
{
    public class PageModel
    {
        #region Constructor
        public PageModel() { }
        #endregion

        public static DataTable CheckPageAccess(string empno, string page)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = DAC.ExecuteDataTable("CheckPageAccess",
                   DAC.Parameter("EMPNO", empno),
                   DAC.Parameter("PAGE", page));
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static DataTable CheckPage(string empno)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = DAC.ExecuteDataTable("CheckPage",
                   DAC.Parameter("EMPNO", empno));
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
