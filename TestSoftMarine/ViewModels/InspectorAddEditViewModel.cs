using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestSoftMarine
{
    public class InspectorAddEditViewModel : BaseViewModel
    {
        private string            _inspectorId;
        private string            _inspectorName;
        private AsyncRelayCommand _saveInspectorCommandAsync;
        private RelayCommand      _cancelCommand;
        private Inspector         _currentInspector;
        private bool              _inspectorIdEditIsEnabled;
        private RelayCommand      _generateInspectorId;

        public string InspectorId
        {
            get => _inspectorId;
            set
            {
                ValidateProperty(value);
                _inspectorId = value;
                OnPropertyChanged();
            }
        }

        public string InspectorName
        {
            get => _inspectorName;
            set
            {
                ValidateProperty(value);
                _inspectorName = value;
                OnPropertyChanged();
            }
        }

        public Inspector CurrentInspector
        {
            get => _currentInspector;
            set
            {
                _currentInspector = value;

                if (_currentInspector == null)
                    _currentInspector = new Inspector();

                InspectorName = _currentInspector.Name;
                InspectorId   = _currentInspector.InspectorId.ToString();

                InspectorIdEditIsEnabled = _currentInspector.InspectorId <= 0;
                OnPropertyChanged();
            }
        }

        public bool InspectorIdEditIsEnabled
        {
            get => _inspectorIdEditIsEnabled;
            set
            {
                _inspectorIdEditIsEnabled = value;
                OnPropertyChanged();
            }
        }

        public AsyncRelayCommand SaveInspectorCommandAsync
        {
            get
            {
                return _saveInspectorCommandAsync ??
                       (_saveInspectorCommandAsync = new AsyncRelayCommand(SaveInspectorAsync , exception => throw new Exception()));
            }
        }

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

        public RelayCommand GenerateInspectorId
        {
            get
            {
                return _generateInspectorId ??
                       (_generateInspectorId = new RelayCommand(x =>
                           {
                               List<Inspector> inspectorList;
                               var             inspectorId = 0;
                               using (var db = new SMTestDBContext())
                                   inspectorList = db.Inspectors.ToList();
                               
                               while (true)
                               {
                                   inspectorId = new Random().Next(100000, 999999);
                                   if(inspectorList.Find(c => c.InspectorId == inspectorId) == null)
                                       break;
                               }

                               InspectorId = inspectorId.ToString();
                           }));
            }
        }

        public Action Close { get; set; }

        public InspectorAddEditViewModel(Inspector currentInspector)
        {
            ValidationRules.Add(nameof(InspectorId),   new List<ValidationRule>() { new InspectorIdValidationRule() });
            ValidationRules.Add(nameof(InspectorName), new List<ValidationRule>() { new InspectorNameValidationRule() });
            
            CurrentInspector = currentInspector;
        }

        public async Task SaveInspectorAsync()
        {
            if (HasErrors)
                return;

            using (var db = new SMTestDBContext())
            {
                var inspector = await db.Inspectors.FindAsync(int.Parse(InspectorId));

                if (CurrentInspector.InspectorId == 0 && inspector != null)
                {
                    MessageBox.Show("Инспектор с таким ID уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                CurrentInspector.InspectorId = int.Parse(InspectorId);
                CurrentInspector.Name        = InspectorName;
                
                if (inspector == null)
                {
                    db.Inspectors.Add(CurrentInspector);
                }
                else
                {
                    db.Entry(inspector).CurrentValues.SetValues(CurrentInspector);
                    db.Entry(inspector).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                Close();
            }
        }
    }
    
    public class InspectorIdValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value == null || string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, "Введите номер инспектора");
            
            if(!int.TryParse(value.ToString(), out var inspectorId))
                return new ValidationResult(false, "Номер инспектора должен быть числом");

            if (inspectorId <= 0)
                return new ValidationResult(false, "Номер инспектора должен быть больше 0");

            return new ValidationResult(true, null);
        }
    }
    
    public class InspectorNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "Введите имя инспектора");

            var name = value.ToString();
            
            if(name.Length == 0)
                return new ValidationResult(false, "Введите имя инспектора");
            
            if(name.Length > 50)
                return new ValidationResult(false, "Имя инспектора не должно превышать 50 символов");

            return new ValidationResult(true, null);
        }
    }
}