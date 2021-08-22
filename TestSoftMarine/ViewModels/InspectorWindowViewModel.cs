using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace TestSoftMarine
{
    public class InspectorsWindowViewModel : BaseViewModel
    {
        private Inspector                       _selectedInspector;
        private int                             _selectedInspectorIndex;
        private RelayCommand                    _addInspectorCommand;
        private RelayCommand                    _editInspectorCommand;
        private AsyncRelayCommand               _removeInspectorCommandAsync;

        public ObservableCollection<Inspector> Inspectors { get; set; }

        public Inspector SelectedInspector
        {
            get => _selectedInspector;
            set
            {
                _selectedInspector = value;
                OnPropertyChanged();
            }
        }

        public int SelectedInspectorIndex
        {
            get => _selectedInspectorIndex;
            set
            {
                _selectedInspectorIndex = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddInspectorCommand
        {
            get
            {
                return _addInspectorCommand ??
                       (_addInspectorCommand = new RelayCommand(x =>
                           {
                               var addEditForm = new InspectorAddEdit();
                               addEditForm.ShowDialog();

                               var t = SelectedInspectorIndex;
                               LoadInspectors();
                               SelectedInspectorIndex = t;
                           }));
            }
        }

        public RelayCommand EditInspectorCommand
        {
            get
            {
                return _editInspectorCommand ??
                       (_editInspectorCommand = new RelayCommand(x =>
                           {
                               if (SelectedInspector == null)
                               {
                                   MessageBox.Show("Выберите инспекцию для изменения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                               
                                   return;
                               }
                               var addEditForm = new InspectorAddEdit(SelectedInspector);
                               addEditForm.ShowDialog();

                               var t = SelectedInspectorIndex;
                               LoadInspectors();
                               SelectedInspectorIndex = t;
                           }));
            }
        }

        public AsyncRelayCommand RemoveInspectorCommandAsync
        {
            get
            {
                return _removeInspectorCommandAsync ??
                       (_removeInspectorCommandAsync = new AsyncRelayCommand(RemoveInspectorAsync, exception => throw new Exception()));
            }
        }

        public InspectorsWindowViewModel()
        {
            Inspectors = new ObservableCollection<Inspector>();
            
            LoadInspectors();

            SelectedInspector      = null;
            SelectedInspectorIndex = -1;
        }
        
        private void LoadInspectors()
        {
            Inspectors.Clear();

            using (var db = new SMTestDBContext())
                foreach (var x in db.Inspectors)
                    Inspectors.Add(x);
        }

        private async Task RemoveInspectorAsync()
        {
            if (SelectedInspector == null)
            {
                MessageBox.Show("Выберите инспектора для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                               
                return;
            }
                               
            if (MessageBox.Show("При удалении инспектора так же удалятся связанные с ним инспекции и заметки!\nВы действительно хотите удалить выделенного инспектора?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Asterisk,
                                MessageBoxResult.No) == MessageBoxResult.No)
                return;
                               
            using (var db = new SMTestDBContext())
            {
                db.Inspectors.Attach(SelectedInspector);
                db.Inspectors.Remove(SelectedInspector);
                await db.SaveChangesAsync();
                LoadInspectors();
            }
        }
    }
}