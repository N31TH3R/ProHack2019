using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using TimeManager.Services;

namespace TimeManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            workItemsList.ItemsSource = new WorkitemService().GetItems();
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            foreach (WorkItem item in workItemsList.ItemsSource)
                item.ResetTime();

            var context = (WindowViewModel)DataContext;
            context.Total = TimeSpan.Zero;
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            var context = (WindowViewModel)DataContext;
            context.StopTimer();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            new WorkitemService().PatchItems(workItemsList.ItemsSource.OfType<WorkItem>().ToList()).Wait();
            foreach (WorkItem item in workItemsList.ItemsSource)
                item.ResetTime();

            var context = (WindowViewModel)DataContext;
            context.Total = TimeSpan.Zero;
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var temp = (WorkItem)btn.DataContext;
            var context = (WindowViewModel)DataContext;
            context.StartTimer(temp);
        }
    }
    public class WindowViewModel : INotifyPropertyChanged
    {
        private TimeSpan _total;
        private Timer _timer;
        private WorkItem _active;

        public TimeSpan Total
        {
            get => _total;
            set
            {
                _total = value;
                NotifyPropertyChanged();
            }
        }

        public void StopTimer()
        {
            _timer.Dispose();
            _timer = null;
            _active = null;
        }

        public void StartTimer(WorkItem item)
        {
            if (_active != null && _active != item)
                _active.IsRunning = false;

            _active = item;

            if (_timer == null)
                _timer = new Timer(_ =>
                {
                    _active.AddTime(TimeSpan.FromSeconds(1));
                    Total = Total.Add(TimeSpan.FromSeconds(1));
                }, null, 0, 1000);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class WorkItem : INotifyPropertyChanged
    {
        public WorkItem(int id, string title)
        {
            Id = id;
            Title = title;
        }

        public WorkItem(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workitem)
        {
            trackerWorkitem = workitem;
            Id = workitem.Id ?? 0;
            Title = workitem.Fields["System.Title"]?.ToString();
        }

        public Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem trackerWorkitem { get; private set; }

        private TimeSpan _time;
        private bool _isRunning;

        public int Id { get; }

        public string Title { get; }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                NotifyPropertyChanged();
            }

        }
        public float hours => (float)_time.Hours + ((float)(100 * _time.Minutes / 60) / 100);

        public string Time
        {
            get => _time.ToString();
            set
            {
                if (TimeSpan.TryParseExact(value, "hh\\:mm\\:ss", CultureInfo.CurrentCulture.DateTimeFormat, out var parsedTime))
                {
                    _time = parsedTime;
                    NotifyPropertyChanged();
                }
            }
        }

        public void AddTime(TimeSpan ts)
        {
            _time = _time.Add(ts);
            NotifyPropertyChanged("Time");
        }

        public void ResetTime()
        {
            _time = TimeSpan.Zero;
            NotifyPropertyChanged("Time");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
