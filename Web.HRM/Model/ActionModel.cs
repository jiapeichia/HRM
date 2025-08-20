using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meo.Web.Model
{
    public class ActionModel
    {
        public bool ActionSuccess { get; set; }
        public string Message { get; set; }
        public string ActionResult { get; set; }
        public ActionModel()
        {

        }
    }
}
