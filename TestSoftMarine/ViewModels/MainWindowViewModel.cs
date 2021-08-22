using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace TestSoftMarine
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<Inspection> Inspections { get; set; }
        public  ObservableCollection<Note>       Notes       { get; set; }
        public  ObservableCollection<Inspector>  Inspectors  { get; set; }

        private int _selectedInspectionIndex;
        public int SelectedInspectionIndex
        {
            get => _selectedInspectionIndex;
            set
            {
                _selectedInspectionIndex = value;
                
                OnPropertyChanged();
            }
        }
        
        private Inspection _selectedInspection = new Inspection();
        public Inspection SelectedInspection
        {
            get => _selectedInspection;
            set
            {
                _selectedInspection = value;

                if (_selectedInspection != null)
                    LoadNotes(value.InspectionId);
                else
                    Notes.Clear();

                OnPropertyChanged();
            }
        }
        
        private Inspection _selectedNote = new Inspection();
        public Inspection SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged();
            }
        }
        
        private Inspector _selectedInspector = new Inspector();
        public Inspector SelectedInspector
        {
            get => _selectedInspector;
            set
            {
                _selectedInspector = value;

                LoadInspection(_selectedInspector?.InspectorId ?? -1, _nameFilter);

                OnPropertyChanged();
            }
        }
        
        private string _nameFilter;
        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value;

                LoadInspection(_selectedInspector?.InspectorId ?? -1, _nameFilter);

                OnPropertyChanged();
            }
        }

        private RelayCommand _editInspectionCommand;
        public RelayCommand EditInspectionCommand
        {
            get
            {
                return _editInspectionCommand ??
                       (_editInspectionCommand = new RelayCommand(x =>
                           {
                               if (SelectedInspection == null)
                               {
                                   MessageBox.Show("Выберите инспекцию для изменения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                               
                                   return;
                               }
                               var addEditForm = new InspectionAddEdit(SelectedInspection);
                               addEditForm.ShowDialog();

                               var t = SelectedInspectionIndex;
                               LoadInspection(SelectedInspector?.InspectorId ?? -1, NameFilter);
                               SelectedInspectionIndex = t;
                           }));
            }
        }

        private RelayCommand _addInspectionCommand;
        public RelayCommand AddInspectionCommand
        {
            get
            {
                return _addInspectionCommand ??
                       (_addInspectionCommand = new RelayCommand(x =>
                           {
                               var addEditForm = new InspectionAddEdit();
                               addEditForm.ShowDialog();

                               var t = SelectedInspectionIndex;
                               LoadInspection(SelectedInspector?.InspectorId ?? -1, NameFilter);
                               SelectedInspectionIndex = t;
                           }));
            }
        }

        private AsyncRelayCommand _removeInspectionCommandAsync;

        public AsyncRelayCommand RemoveInspectionCommandAsync
        {
            get
            {
                return _removeInspectionCommandAsync ??
                       (_removeInspectionCommandAsync = new AsyncRelayCommand(RemoveInspectionAsync, exception => throw new Exception()));
            }
        }

        private RelayCommand _openInspectorsWindow;
        public RelayCommand OpenInspectorsWindow
        {
            get
            {
                return _openInspectorsWindow ??
                       (_openInspectorsWindow = new RelayCommand(x =>
                           {
                               var inspectorsForm = new InspectorsWindow();
                               inspectorsForm.ShowDialog();
                               
                               LoadInspector();
                               LoadInspection(SelectedInspector?.InspectorId ?? -1, NameFilter);
                           }));
            }
        }

        public MainWindowViewModel()
        {
            Inspections = new ObservableCollection<Inspection>();
            Notes       = new ObservableCollection<Note>();
            Inspectors  = new ObservableCollection<Inspector>();

            SelectedInspection      = null;
            SelectedInspectionIndex = -1;
            
            LoadInspector();
        }

        private void LoadInspector()
        {
            Inspectors.Clear();

            SelectedInspector = new Inspector() { Name = "Все", InspectorId = -1 };
            Inspectors.Add(SelectedInspector);

            using (var db = new SMTestDBContext())
                foreach (var x in db.Inspectors.Include(x => x.Inspections))
                    Inspectors.Add(x);
        }

        private void LoadInspection(int inspectorId = -1, string name = "")
        {
            Inspections.Clear();

            using (var db = new SMTestDBContext())
                foreach (var x in db.Inspections.Where(x => (inspectorId == -1          || x.InspectorId == inspectorId) &&
                                                            (string.IsNullOrEmpty(name) || x.Name.Contains(name))).Include(x => x.Inspector).Include(x => x.Notes))
                    Inspections.Add(x);
        }

        private void LoadNotes(int inspectionId)
        {
            Notes.Clear();

            using (var db = new SMTestDBContext())
                foreach (var x in db.Notes.Where(x => x.InspectionId == inspectionId).Include(x => x.Inspection))
                    Notes.Add(x);
        }
        
        private async Task RemoveInspectionAsync()
        {
            if (SelectedInspection == null)
            {
                MessageBox.Show("Выберите инспекцию для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                               
                return;
            }
                               
            if (MessageBox.Show("При удалении инспекции так же удалятся связанные с ней заметки!\nВы действительно хотите удалить выделенную инспекцию?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Asterisk,
                                MessageBoxResult.No) == MessageBoxResult.No)
                return;
                               
            using (var db = new SMTestDBContext())
            {
                db.Inspections.Attach(SelectedInspection);
                db.Inspections.Remove(SelectedInspection);
                await db.SaveChangesAsync();
                LoadInspection(SelectedInspector?.InspectorId ?? -1, NameFilter);
            }
        }
    }
}