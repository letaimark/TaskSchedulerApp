using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TaskSchedulerApp.Models;



namespace TaskSchedulerApp;

public partial class MainWindow : Window
{
    private ObservableCollection<TaskModel> pendingTasks = new ObservableCollection<TaskModel>();
    private ObservableCollection<TaskModel> scheduledTasks = new ObservableCollection<TaskModel>();

    
    private DispatcherTimer pomodoroTimer;
    private TimeSpan pomodoroTimeLeft;

    public MainWindow()
    {
        InitializeComponent();

       
        lstTasks.ItemsSource = pendingTasks;
        lstScheduledTasks.ItemsSource = scheduledTasks;

        InitializePomodoroTimer();
    }


    private void InitializePomodoroTimer()
    {
        pomodoroTimeLeft = TimeSpan.FromMinutes(1);
        txtPomodoroTimer.Text = pomodoroTimeLeft.ToString(@"mm\:ss");
        pomodoroTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        pomodoroTimer.Tick += PomodoroTimer_Tick;
    }

    private void PomodoroTimer_Tick(object sender, EventArgs e)
    {
        if (pomodoroTimeLeft.TotalSeconds > 0)
        {
            pomodoroTimeLeft = pomodoroTimeLeft.Subtract(TimeSpan.FromSeconds(1));
            txtPomodoroTimer.Text = pomodoroTimeLeft.ToString(@"mm\:ss");
        }
        else
        {
            pomodoroTimer.Stop();
            MessageBox.Show("Pomodoro session completed!");
        }
    }

    private void PomodoroStart_Click(object sender, RoutedEventArgs e)
    {
        pomodoroTimer.Start();
    }

    private void PomodoroPause_Click(object sender, RoutedEventArgs e)
    {
        pomodoroTimer.Stop();
    }

    private void PomodoroReset_Click(object sender, RoutedEventArgs e)
    {
        pomodoroTimer.Stop();
        pomodoroTimeLeft = TimeSpan.FromMinutes(25);
        txtPomodoroTimer.Text = pomodoroTimeLeft.ToString(@"mm\:ss");
    }

   
    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        string title = txtTaskTitle.Text;
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Please enter a task title.");
            return;
        }

        if (!int.TryParse(txtTaskDuration.Text, out int durationMinutes))
        {
            MessageBox.Show("Please enter a valid duration.");
            return;
        }
        DateTime selectedDate = dpTaskDate.SelectedDate ?? DateTime.Today;

        TaskModel task = new TaskModel()
        {
            Id = pendingTasks.Count + scheduledTasks.Count + 1,
            Title = title,
            EstimatedDuration = TimeSpan.FromMinutes(durationMinutes),
            TaskDate = selectedDate,
            Completed = false
        };

        pendingTasks.Add(task);
        txtTaskTitle.Clear();
        txtTaskDuration.Clear();
        dpTaskDate.SelectedDate = DateTime.Today;
    }
    private async void ScheduleTask(TaskModel task)
    {
        scheduledTasks.Add(task);

        GoogleCalendarService calendarService = new GoogleCalendarService();

        
        await calendarService.AddTaskToGoogleCalendar(task);

        MessageBox.Show($"Task '{task.Title}' added to Google Calendar!");
    }
    private void lstTasks_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            ListBox listBox = sender as ListBox;
            if (listBox.SelectedItem == null)
                return;

            TaskModel task = listBox.SelectedItem as TaskModel;
            DataObject data = new DataObject("task", task);
            DragDrop.DoDragDrop(listBox, data, DragDropEffects.Move);
        }
    }

    private void lstScheduledTasks_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("task"))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
    }

    private void lstScheduledTasks_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("task"))
        {
            TaskModel task = e.Data.GetData("task") as TaskModel;
            if (task != null)
            {
                if (pendingTasks.Contains(task))
                    pendingTasks.Remove(task);

                task.ScheduledTime = DateTime.Now;
                scheduledTasks.Add(task);
            }
        }
    }

    private void MarkCompleted_Click(object sender, RoutedEventArgs e)
    {
        if (lstScheduledTasks.SelectedItem is TaskModel task)
        {
            task.Completed = true;
            MessageBox.Show($"Task '{task.Title}' marked as completed.");
        }
        else
        {
            MessageBox.Show("Please select a scheduled task to mark as completed.");
        }
    }

    private void RefreshAnalytics_Click(object sender, RoutedEventArgs e)
    {
        int totalTasks = pendingTasks.Count + scheduledTasks.Count;
        int completed = 0;
        int overdueTasks = scheduledTasks.Count(task => task.TaskDate < DateTime.Today && !task.Completed);
        foreach (var task in scheduledTasks)
        {
            if (task.Completed)
                completed++;
        }
        txtAnalytics.Text = $"Total Tasks: {totalTasks}\n" +
                            $"Completed Tasks: {completed}\n" +
                            $"Pending Tasks: {pendingTasks.Count}\n" +
                            $"Overdue Tasks: {overdueTasks}";
    }

  
}