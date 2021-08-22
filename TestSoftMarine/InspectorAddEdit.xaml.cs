using System.Windows;

namespace TestSoftMarine
{
    public partial class InspectorAddEdit : Window
    {
        public InspectorAddEditViewModel InspectorAddEditViewModel;
        public InspectorAddEdit(Inspector currentInspector = null)
        {
            InitializeComponent();
            InspectorAddEditViewModel       = new InspectorAddEditViewModel(currentInspector);
            DataContext                     = InspectorAddEditViewModel;
            InspectorAddEditViewModel.Close = Close;
        }
    }
}