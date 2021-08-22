using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestSoftMarine
{
    public class InspectionAddEditViewModel : BaseViewModel
    {
        public Action                          Close             { get; set; }
        public ObservableCollection<Note>      Notes             { get; set; }
        public ObservableCollection<Inspector> Inspectors        { get; set; }

        private bool _notesEditIsEnable;
        public bool NotesEditIsEnable
        {
            get => _notesEditIsEnable;
            set
            {
                _notesEditIsEnable = value; 
                OnPropertyChanged();
            }
        }

        private string _inspectionDate;
        public string InspectionDate
        {
            get => _inspectionDate;
            set
            {
                ValidateProperty(value);
                
                _inspectionDate = value;
                OnPropertyChanged();
            }
        }

        private string _inspectionName;
        public string InspectionName
        {
            get => _inspectionName;
            set
            {
                ValidateProperty(value);

                _inspectionName = value;
                OnPropertyChanged();
            }
        }
        
        private Inspector _selectedInspector;
        public Inspector SelectedInspector
        {
            get => _selectedInspector;
            set
            {
                ValidateProperty(value);
                _selectedInspector = value;

                OnPropertyChanged();
            }
        }

        private Inspection _currentInspection;
        public Inspection CurrentInspection
        {
            get => _currentInspection;
            set
            {
                _currentInspection = value;

                if (_currentInspection != null)
                {
                    LoadNotes(_currentInspection.InspectionId);
                    InspectionDate = _currentInspection.Date.ToShortDateString();
                }
                else
                {
                    _currentInspection = new Inspection();
                    InspectionDate     = DateTime.Now.ToShortDateString();
                }

                SelectedInspector = Inspectors.FirstOrDefault(x => x.InspectorId == CurrentInspection.InspectorId);
                InspectionName    = CurrentInspection.Name;
                
                NotesEditIsEnable = _currentInspection.InspectionId != 0;

                OnPropertyChanged();
            }
        }

        private Note _selectedNote;
        public Note SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged();
            }
        }

        private int _selectedNoteIndex;
        public int SelectedNoteIndex
        {
            get => _selectedNoteIndex;
            set
            {
                _selectedNoteIndex = value; 
                OnPropertyChanged();
            }
        }

        private AsyncRelayCommand _saveInspectionCommandAsync;
        public AsyncRelayCommand SaveInspectionCommandAsync
        {
            get
            {
                return _saveInspectionCommandAsync ??
                       (_saveInspectionCommandAsync = new AsyncRelayCommand(SaveInspectionAsync , exception => throw new Exception()));
            }
        }
        
        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                       (_cancelCommand = new RelayCommand(x =>
                           {
                               Close();
                           }));
            }
        }
        
        private RelayCommand _addNoteCommand;
        public RelayCommand AddNoteCommand
        {
            get
            {
                return _addNoteCommand ??
                       (_addNoteCommand = new RelayCommand(x =>
                           {
                               var addEditForm = new NoteAddEdit(CurrentInspection);
                               addEditForm.ShowDialog();
                               
                               var t = SelectedNoteIndex;
                               LoadNotes(CurrentInspection.InspectionId);
                               SelectedNoteIndex = t;
                           }));
            }
        }
        
        private AsyncRelayCommand _removeNoteCommandAsync;
        public AsyncRelayCommand RemoveNoteCommandAsync
        {
            get
            {
                return _removeNoteCommandAsync ??
                       (_removeNoteCommandAsync = new AsyncRelayCommand(RemoveNoteAsync, exception => throw new Exception()));
            }
        }
        
        private RelayCommand      _editNoteCommand;
        public RelayCommand EditNoteCommand
        {
            get
            {
                return _editNoteCommand ??
                       (_editNoteCommand = new RelayCommand(x =>
                           {
                               if (SelectedNote == null)
                               {
                                   MessageBox.Show("Выберите заметку для изменения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                               
                                   return;
                               }
                               var addEditForm = new NoteAddEdit(CurrentInspection, SelectedNote);
                               addEditForm.ShowDialog();
                               
                               var t = SelectedNoteIndex;
                               LoadNotes(CurrentInspection.InspectionId);
                               SelectedNoteIndex = t;
                           }));
            }
        }


        public InspectionAddEditViewModel(Inspection inspection)
        {
            ValidationRules.Add(nameof(InspectionName),    new List<ValidationRule>() { new InspectionNameValidationRule() });
            ValidationRules.Add(nameof(SelectedInspector), new List<ValidationRule>() { new SelectedInspectorValidationRule() });
            ValidationRules.Add(nameof(InspectionDate),    new List<ValidationRule>() { new InspectionDateValidationRule() });
            
            Notes      = new ObservableCollection<Note>();
            Inspectors = new ObservableCollection<Inspector>();

            SelectedNoteIndex = -1;
            LoadInspector();
            
            CurrentInspection = inspection;
        }

        private void LoadInspector()
        {
            Inspectors.Clear();

            using (var db = new SMTestDBContext())
                foreach (var x in db.Inspectors.Include(x => x.Inspections))
                    Inspectors.Add(x);
            
        }

        private void LoadNotes(int inspectionId)
        {
            Notes.Clear();

            using (var db = new SMTestDBContext())
                foreach (var x in db.Notes.Where(x => x.InspectionId == inspectionId).Include(x => x.Inspection))
                    Notes.Add(x);
        }

        private async Task SaveInspectionAsync()
        {
            if (HasErrors)
                return;

            CurrentInspection.Name        = InspectionName;
            CurrentInspection.InspectorId = SelectedInspector.InspectorId;
            CurrentInspection.Date        = DateTime.Parse(InspectionDate);

            using (var db = new SMTestDBContext())
            {
                var inspection = await db.Inspections.FindAsync(CurrentInspection.InspectionId);

                if (inspection == null)
                {
                    db.Inspections.Add(CurrentInspection);
                }
                else
                {
                    db.Entry(inspection).CurrentValues.SetValues(CurrentInspection);
                    db.Entry(inspection).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
            }

            NotesEditIsEnable = _currentInspection.InspectionId != 0;
        }

        private async Task RemoveNoteAsync()
        {
            if (SelectedNote == null)
            {
                MessageBox.Show("Выберите заметку для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                return;
            }

            if (MessageBox.Show("Вы действительно хотите удалить выделенную заметку?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Asterisk,
                                MessageBoxResult.No) == MessageBoxResult.No)
                return;

            using (var db = new SMTestDBContext())
            {
                db.Notes.Attach(SelectedNote);
                db.Notes.Remove(SelectedNote);
                await db.SaveChangesAsync();
                LoadNotes(CurrentInspection.InspectionId);
            }
        }
    }

    public class InspectionNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value?.ToString();

            return str?.Length > 0 && str.Length < 50 ? new ValidationResult(true, null) : new ValidationResult(false, "Введите название инспекции (не более 50 символов)");
        }
    }

    public class SelectedInspectorValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value != null ? new ValidationResult(true, null) : new ValidationResult(false, "Выберите инспектора");
        }
    }

    public class InspectionDateValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return DateTime.TryParse(value?.ToString(), out var _) ? new ValidationResult(true, null) : new ValidationResult(false, "Введите дату правильно");
        }
    }
}