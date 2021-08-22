using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TestSoftMarine
{
    public class NoteAddEditViewModel : BaseViewModel
    {
        private string            _remark;
        private string            _fixDate;
        private string            _comment;
        private Inspection        _targetInspection;
        private Note              _currentNote;
        private AsyncRelayCommand _saveNoteCommandAsync;
        private RelayCommand      _cancelCommand;

        public Action Close { get; set; }

        public string Remark
        {
            get => _remark;
            set
            {
                ValidateProperty(value);
                _remark = value;
                OnPropertyChanged("Remark");
            }
        }

        public string FixDate
        {
            get => _fixDate;
            set
            {
                ValidateProperty(value);
                _fixDate = value;
                OnPropertyChanged("FixDate");
            }
        }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged();
            }
        }

        public Inspection TargetInspection
        {
            get => _targetInspection;
            set
            {
                _targetInspection = value;
                OnPropertyChanged();
            }
        }

        public Note CurrentNote
        {
            get => _currentNote;
            set
            {
                _currentNote = value;

                if (_currentNote == null)
                {
                    _currentNote = new Note() { InspectionId = TargetInspection.InspectionId };
                    FixDate      = "";
                }
                else
                    FixDate = _currentNote.FixDate?.ToShortDateString();

                Remark  = _currentNote.Remark;
                Comment = _currentNote.Comment;

                OnPropertyChanged();
            }
        }

        public AsyncRelayCommand SaveNoteCommandAsync
        {
            get
            {
                return _saveNoteCommandAsync ??
                       (_saveNoteCommandAsync = new AsyncRelayCommand(SaveNoteAsync, exception => throw new Exception()));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                       (_cancelCommand = new RelayCommand(x => { Close(); }));
            }
        }

        public NoteAddEditViewModel(Inspection targetInspection, Note currentNote)
        {
            ValidationRules.Add(nameof(Remark),  new List<ValidationRule>() { new NoteRemarkValidationRule() });
            ValidationRules.Add(nameof(FixDate), new List<ValidationRule>() { new NoteFixDateValidationRule() });

            TargetInspection = targetInspection;
            CurrentNote      = currentNote;
        }

        private async Task SaveNoteAsync()
        {
            if (HasErrors)
                return;

            CurrentNote.Remark       = Remark;
            CurrentNote.InspectionId = TargetInspection.InspectionId;
            CurrentNote.Comment      = Comment;

            if (string.IsNullOrEmpty(FixDate))
                CurrentNote.FixDate = null;
            else
                CurrentNote.FixDate = DateTime.Parse(FixDate);


            using (var db = new SMTestDBContext())
            {
                var note = await db.Notes.FindAsync(CurrentNote.NoteId);

                if (note == null)
                {
                    db.Notes.Add(CurrentNote);
                }
                else
                {
                    db.Entry(note).CurrentValues.SetValues(CurrentNote);
                    db.Entry(note).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                Close();
            }
        }

    }

    public class NoteRemarkValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value != null && value?.ToString().Length != 0 ? new ValidationResult(true, null) : new ValidationResult(false, "Введите замечание");
        }
    }

    public class NoteFixDateValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || DateTime.TryParse(value?.ToString(), out var _) ? new ValidationResult(true, null) : new ValidationResult(false, "Введите дату правильно");
        }
    }
}