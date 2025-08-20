using System.ComponentModel.DataAnnotations;

namespace Web.HRM.Controllers
{
    public class NodeModel
    {
        public string id { get; set; }
        public string pid { get; set; }

        public string img { get; set; }

        public string fullName { get; set; }

        public string email { get; set; }

        public string title { get; set; }
    }
}