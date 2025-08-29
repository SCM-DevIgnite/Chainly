using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal EmissionValue { get; set; }
        public string Unit { get; set; }
        public ICollection<SupplierMaterial> SupplierMaterials { get; set; }
    }
}
