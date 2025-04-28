using System;
using System.ComponentModel;

namespace TaskSchedulerApp.Models
{
    public class TaskModel : INotifyPropertyChanged
    {
        private string _title;
        private TimeSpan _estimatedDuration;
        private DateTime? _scheduledTime;
        private bool _completed;

        public int Id { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public TimeSpan EstimatedDuration
        {
            get => _estimatedDuration;
            set
            {
                _estimatedDuration = value;
                OnPropertyChanged("EstimatedDuration");
            }
        }

        public DateTime? ScheduledTime
        {
            get => _scheduledTime;
            set
            {
                _scheduledTime = value;
                OnPropertyChanged("ScheduledTime");
            }
        }
        private DateTime _taskDate = DateTime.Today;

        public DateTime TaskDate
        {
            get => _taskDate;
            set
            {
                _taskDate = value;
                OnPropertyChanged(nameof(TaskDate));
            }
        }
        public bool IsOverdue => TaskDate < DateTime.Today && !Completed;
        public bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                OnPropertyChanged("Completed");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
