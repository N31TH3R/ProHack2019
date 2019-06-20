using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

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
                new WorkItem{Id = 44444, Title = "Kek"},
                new WorkItem{Id = 65656, Title = "Lol"},
                new WorkItem{Id = 567567567, Title = "jjjjjjjs"}
            };

            workItemsList.ItemsSource = workitems;
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
        private int _id;
        private string _title;
        private TimeSpan _time;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                NotifyPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        public string Time => _time.ToString();

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
