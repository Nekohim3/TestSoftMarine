using System.Windows;

namespace TestSoftMarine
{
    public partial class InspectorsWindow : Window
    {
        public InspectorsWindowViewModel InspectorsWindowViewModel = new InspectorsWindowViewModel();
        public InspectorsWindow()
        {
            InitializeComponent();
            DataContext = InspectorsWindowViewModel;
        }
    }
}