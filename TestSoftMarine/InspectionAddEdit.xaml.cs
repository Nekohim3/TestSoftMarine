using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestSoftMarine
{
    /// <summary>
    /// Логика взаимодействия для InspectionAddEdit.xaml
    /// </summary>
    public partial class InspectionAddEdit : Window
    {
        public InspectionAddEditViewModel InspectionAddEditViewModel;
        public InspectionAddEdit(Inspection inspection = null)
        {
            InitializeComponent();
            InspectionAddEditViewModel       = new InspectionAddEditViewModel(inspection);
            DataContext                      = InspectionAddEditViewModel;
            InspectionAddEditViewModel.Close = Close;
        }
    }
}
