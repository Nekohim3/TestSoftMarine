using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSoftMarine
{
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoteId { get;        set; }
        public string    Remark  { get; set; }
        public DateTime? FixDate { get; set; }
        public string    Comment { get; set; }

        public int        InspectionId { get; set; }
        [ForeignKey("InspectionId")]
        public virtual Inspection Inspection { get; set; }
    }
}
