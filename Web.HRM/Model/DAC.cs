using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Meo.Web.Model
{
    public class DAC
    {
        public static string con;
        /// <summary>
        /// Calls a stored procedure and return the result
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
        /// <param name="arrParam">Parameters required by the stored procedure</param>
        /// <returns>DataTable containing the result</returns>
        /// <remarks></remarks>

        public static DataTable ExecuteDataTable(string storedProcedureName, params SqlParameter[] arrParam)
        {
            DataTable dt = null;
            //Open the connection
            string s = con;

            SqlConnection cnn = new SqlConnection(WebConfigurationManager.ConnectionStrings["WebConnectionString"].ToString());
            {
                cnn.Open();
                //Define the commands
                SqlCommand cmd = new SqlCommand
                {
                    Connection = cnn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = storedProcedureName
                };

                //Handle the parameters
                if (arrParam != null)
                {
                    foreach (SqlParameter param in arrParam)
                    {
                        if (param.Value != null)
                            cmd.Parameters.Add(param);
                    }
                }

                //Define the data adapter and fill the dataset
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);

                cnn.Close();
            }
            return dt;
        }

        public static DataTable ExecuteDataTable(string storedProceduceName)
        {
            return ExecuteDataTable(storedProceduceName, null);
        }

        /// <summary>
        /// Creates a parameter
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="parameterValue">Value of the parameter</param>
        /// <returns>SqlParameter Object</returns>
        /// <remarks>The parameter name should be the same as the property name</remarks>
        public static SqlParameter Parameter(string parameterName, object parameterValue)
        {
            SqlParameter param = new SqlParameter
            {
                ParameterName = parameterName,
                Value = parameterValue
            };
            return param;
        }
    }
}
