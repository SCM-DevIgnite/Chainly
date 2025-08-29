using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.Models
{
    public class ProductionLine
    {
        public int Id { get; set; }
        public string LineName { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public string? Description { get; set; }
        public ICollection<Report> Reports { get; set; }

    }
}
