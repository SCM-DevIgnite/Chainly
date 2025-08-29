using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int productionLineId { get; set; }
        public ProductionLine ProductionLine { get; set; }
        public DateTime ReportDate { get; set; }
        public int GoodProducts { get; set; }

        public virtual ICollection<DefectiveProduct> DefectiveProducts { get; set; } = new List<DefectiveProduct>();


        [NotMapped]
        public int DefectiveProductsCount => DefectiveProducts?.Count ?? 0;

        [NotMapped]
        public int TotalProducts => GoodProducts + DefectiveProductsCount;

        [NotMapped]
        public double DefectiveRatio => TotalProducts == 0 ? 0 :
            (double)DefectiveProductsCount / TotalProducts;

    }
}
