using System.Windows;

namespace TestSoftMarine
{
    public partial class NoteAddEdit : Window
    {
        public NoteAddEditViewModel NoteAddEditViewModel;
        public NoteAddEdit(Inspection targetInspection, Note currentNote = null)
        {
            InitializeComponent();
            NoteAddEditViewModel       = new NoteAddEditViewModel(targetInspection, currentNote);
            DataContext                = NoteAddEditViewModel;
            NoteAddEditViewModel.Close = Close;
        }
    }
}