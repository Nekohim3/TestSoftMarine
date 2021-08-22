using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace TestSoftMarine
{
    public class SMTestDBContext : DbContext
    {
        public SMTestDBContext() : base("SMTestDB")
        {
            
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Inspector> Inspectors { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
    }
}
