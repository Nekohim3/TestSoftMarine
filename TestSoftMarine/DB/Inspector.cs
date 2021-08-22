using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSoftMarine
{
    public class Inspector
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int    InspectorId { get; set; }
        public string Name        { get; set; }
        
        public virtual ICollection<Inspection> Inspections { get; set; }

        public string DisplayHelper => ToString();
        
        public override string ToString()
        {
            return $"{Name} {(InspectorId == -1 ? "" : $"({InspectorId})")}";
        }
    }
}
