using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.Models
{
    public class SupplierMaterial
    {
        
        public int MaterialId { get; set; }
        public Material Material { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

    }
}
