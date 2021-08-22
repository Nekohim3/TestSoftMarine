using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace TestSoftMarine
{
    public class BaseViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private   Dictionary<string, List<string>>            Errors          { get; }
        protected Dictionary<string, List<ValidationRule>>    ValidationRules { get; }
        
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler              PropertyChanged;
        
        public  bool HasErrors                              => Errors.Any();
        private bool PropertyHasErrors(string propertyName) => Errors.TryGetValue(propertyName, out var propertyErrors) && propertyErrors.Any();
        
        protected BaseViewModel()
        {
            Errors = new Dictionary<string, List<string>>();
            ValidationRules = new Dictionary<string, List<ValidationRule>>();
        }

        protected bool ValidateProperty<TValue>(TValue propertyValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return true;

            Errors.Remove(propertyName);
            OnErrorsChanged(propertyName);

            if (!ValidationRules.TryGetValue(propertyName, out var propertyValidationRules)) return true;

            propertyValidationRules
                   .Select(validationRule => validationRule.Validate(propertyValue, CultureInfo.CurrentCulture))
                   .Where(result => !result.IsValid)
                   .ToList()
                   .ForEach(invalidResult => AddError(propertyName, invalidResult.ErrorContent as string));

            return !PropertyHasErrors(propertyName);
        }

        private void AddError(string propertyName, string errorMessage, bool isWarning = false)
        {
            if (!Errors.TryGetValue(propertyName, out List<string> propertyErrors))
            {
                propertyErrors = new List<string>();
                Errors[propertyName] = propertyErrors;
            }

            if (propertyErrors.Contains(errorMessage)) return;

            if (isWarning)
            {
                propertyErrors.Add(errorMessage);
            }
            else
            {
                propertyErrors.Insert(0, errorMessage);
            }

            OnErrorsChanged(propertyName);
        }

        public IEnumerable GetErrors(string propertyName) =>
                string.IsNullOrWhiteSpace(propertyName) 
                        ? Errors.SelectMany(entry => entry.Value)
                        : Errors.TryGetValue(propertyName, out var errors)
                                ? errors
                                : new List<string>();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

    }
}