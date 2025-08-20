using Meo.Web.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Web.HRM.Model
{
    public class DataLogModel
    {
        public static DataTable SP_HR_StageApprovalLog(string approvallog_id,string id)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = DAC.ExecuteDataTable("SP_HR_StageApprovalLog",
                   DAC.Parameter("APPROVALLOGID", approvallog_id),
                   DAC.Parameter("PERFORMANCEID", id));
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}