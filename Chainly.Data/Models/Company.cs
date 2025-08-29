using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LocationLongitude { get; set; }
        public string LocationLatitude { get; set; }
        public string? Logo { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
