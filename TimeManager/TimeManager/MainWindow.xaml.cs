using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
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
        public MainWindow()
        {
            InitializeComponent();
            List<WorkItem> workitems = new List<WorkItem>
            {
                new WorkItem(1234, "Kek"),
                new WorkItem(12345, "Kek"),
                new WorkItem(123456, "Kek")
            };

            //workItemsList.ItemsSource = workitems;
            var x = new WorkitemService().GetItems();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (timer != null)
                timer.Close();
            timer = new Timer(1000);
            timer.Elapsed += (x, y) =>
            {
                this.Dispatcher.Invoke(() =>
                ((WorkItem)((Button)sender).DataContext).AddTime(TimeSpan.FromMilliseconds(1000)));
            };

            timer.Start();
        }

        private void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {
            if ((string)pauseBtn.Content == "Pause")
            {
                pauseBtn.Content = "Resume";
                timer.Stop();
            }
            else
            {
                pauseBtn.Content = "Pause";
                timer.Start();
            }
        }

        //private void OnUnPauseButtonClick(object sender, RoutedEventArgs e)
        //{
        //    unpauseBtn.Visibility = Visibility.Hidden;
        //    pauseBtn.Visibility = Visibility.Visible;
        //}

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

        public int Id { get; }

        public string Title { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
