using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrcaTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
            this.Robots = new ObservableCollection<RobotModel>();
            foreach (var r in RobotModel.Generate(10, 100, 200, 200)) {
                this.Robots.Add(r);
            }
            
            var t = new Task(() => {
                while (true) {
                    foreach (var r in this.Robots) {
                        r.CalculateNextMove();
                        r.Move();
                        Thread.Sleep(10);
                    }
                }

            }, TaskCreationOptions.LongRunning);
            t.Start();
        }

        //private TimeSpan _transmissionPeriod = TimeSpan.FromMilliseconds(50);
        //public TimeSpan TransmissionPeriod {
        //    get { return _transmissionPeriod; }
        //    set {
        //        _transmissionPeriod = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        private ObservableCollection<RobotModel> _Robots;
        public ObservableCollection<RobotModel> Robots {
            get { return _Robots; }
            set {
                _Robots = value;
                NotifyPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Implementation

        private void Reset_Click_1(object sender, RoutedEventArgs e) {
            this.Robots = new ObservableCollection<RobotModel>();
            foreach (var r in RobotModel.Generate(10, 100, 200, 200)) {
                this.Robots.Add(r);
                RobotModel.validator.Reset();
            }
            
        }
    }
}
