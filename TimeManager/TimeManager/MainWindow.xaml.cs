using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeManager.Services;

namespace TimeManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer timer;
        private WorkItem _active;

        public MainWindow()
        {
            InitializeComponent();
            List<WorkItem> workitems = new List<WorkItem>
            {
                new WorkItem(1234, "Kek"),
                new WorkItem(12345, "Kek2"),
                new WorkItem(123456, "Kek3")
            };

            //workItemsList.ItemsSource = workitems;
            var x = new WorkitemService().GetItems();
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            foreach (WorkItem item in workItemsList.ItemsSource)
                item.ResetTime();
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var temp = (WorkItem)btn.DataContext;
            timer.Dispose();
            timer = null;
            _active = null;
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var temp = (WorkItem)btn.DataContext;

            if (_active != null && temp != _active)
                _active.IsRunning = false;

            _active = temp;

            if (timer == null)
                timer = new Timer(_ => Dispatcher.Invoke(() => _active.AddTime(TimeSpan.FromSeconds(1))), null, 0, 1000);
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
