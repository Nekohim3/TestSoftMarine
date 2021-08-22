using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSoftMarine
{
    public class Inspection
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int       InspectionId { get; set; }
        
        public       string    Name         { get; set; }
        public       DateTime  Date         { get; set; }
        public       string    Comment      { get; set; }
        
        public       int       InspectorId  { get; set; }
        [ForeignKey("InspectorId")]
        public virtual Inspector Inspector { get; set; }

        public virtual ICollection<Note> Notes { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int NotesCount { get; set; }
    }
}
