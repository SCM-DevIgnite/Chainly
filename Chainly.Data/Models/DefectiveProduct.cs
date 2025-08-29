using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.Models
{
    public class DefectiveProduct
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public Report Report { get; set; }
        public DateTime Time { get; set; }
        public int ProductId { get; set; }
        public string DefectiveType { get; set; }
    }
}
