using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GestionStationnement.Helpers;
using GestionStationnement.Models;


namespace GestionStationnement.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Properties

        private string _pendingIpAddress = "192.168.1.100";
        private string _pendingLogicalId = "1";
        private string _pendingFriendlyName = "A1";
        private Publisher _publisher;

        private GetConfigService cfgservice;
       // private SensorUpdateService updtservice;

        public string PendingIpAddress
        {
            get { return _pendingIpAddress; }
            set { _pendingIpAddress = value; RaisePropertyChanged(()=> PendingIpAddress); }
        }

        public string PendingLogicalId
        {
            get { return _pendingLogicalId; }
            set { _pendingLogicalId = value; RaisePropertyChanged(() => PendingLogicalId); }
        }

        public string PendingFriendlyName
        {
            get { return _pendingFriendlyName; }
            set { _pendingFriendlyName = value; RaisePropertyChanged(() => PendingFriendlyName); }
        }

        private ObservableCollection<Sensor> _sensorlist;
        private string _imagePath;

        [DataMember]
        public ObservableCollection<Sensor> SensorList
        {
            get { return _sensorlist; }
            set
            {
                if (_sensorlist != value)
                {
                    _sensorlist = value;
                    RaisePropertyChanged(() => SensorList);
                }
            }
        }

        private ImageSource _planImageSource;
        public ImageSource PlanImageSource
        {
            get { return _planImageSource; }
            set
            {
                if (_planImageSource != value)
                {
                    _planImageSource = value;
                    RaisePropertyChanged(() => PlanImageSource);
                }
            }
        }

        #endregion

        #region Commands

        private ICommand _loadPlanSourceCommand;
        public ICommand LoadPlanSourceCommand
        {
            get { return _loadPlanSourceCommand ?? (_loadPlanSourceCommand = new DelegateCommand(OnLoadImage)); }
        }

        private ICommand _addSensorTestCommand;
        public ICommand AddSensorTestCommand
        {
            get { return _addSensorTestCommand ?? (_addSensorTestCommand = new DelegateCommand(OnAddSensor)); }
        }

        private ICommand _saveConfigCommand;
        public ICommand SaveConfigCommand
        {
            get { return _saveConfigCommand ?? (_saveConfigCommand = new DelegateCommand(OnSaveConfig)); }
        }

        private ICommand _loadConfigCommand;
        public ICommand LoadConfigCommand
        {
            get { return _loadConfigCommand ?? (_loadConfigCommand = new DelegateCommand(OnLoadConfig)); }
        }

        private ICommand _startServiceCommand;
        public ICommand StartServiceCommand
        {
            get { return _startServiceCommand ?? (_startServiceCommand = new DelegateCommand(OnStartService)); }
        }

        private ICommand _stopServiceCommand;
        public ICommand StopServiceCommand
        {
            get { return _stopServiceCommand ?? (_stopServiceCommand = new DelegateCommand(OnStopService)); }
        }

        #endregion

        #region Ctor
        public MainWindowViewModel()
        {

            SensorList = new ObservableCollection<Sensor>();
            SensorList.CollectionChanged += SensorList_CollectionChanged;

        }
        #endregion

        #region Command Handlers

        private void OnLoadImage()
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    FileName = "",
                    Filter =  "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
                };

                // Show open file dialog box
                dlg.ShowDialog();
                var filePath = dlg.FileName;


                if (File.Exists(filePath))
                {
                    PlanImageSource = new BitmapImage(new Uri(filePath));
                    RaisePropertyChanged(()=>PlanImageSource);
                    _imagePath = filePath;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnAddSensor()
        {
            var sensor = new Sensor(PendingIpAddress, PendingLogicalId, "true", PendingFriendlyName, "20", "20");
            SensorList.Add(sensor);

        }

        private void OnSaveConfig()
        {
            using (var file = new StreamWriter("Repertoire.cfg"))
            {
                file.WriteLine(_imagePath);
                foreach (var sensor in SensorList)
                {
                    file.WriteLine("{0},{1},{2},{3},{4},{5}", sensor.IpAddress, sensor.LogicalId, sensor.IsOccupied, sensor.FriendlyName, sensor.CoordinateX, sensor.CoordinateY);
                }
            }
        }

        private void OnLoadConfig()
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    FileName = "",
                    Filter = "Config File (*.cfg)| *.cfg"
                };

                // Show open file dialog box
                dlg.ShowDialog();
                dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;


                var filePath = dlg.FileName;


                if (!File.Exists(filePath)) return;
                using (var sr = new StreamReader(filePath))
                {
                    SensorList.Clear();
                    var temp = sr.ReadLine();
                    PlanImageSource = new BitmapImage(new Uri(temp));
                    _imagePath = temp;
                    RaisePropertyChanged(() => PlanImageSource);

                    while (sr.Peek() >= 0)
                    {
                        var readLine = sr.ReadLine();
                        if (!string.IsNullOrEmpty(readLine))
                        {
                            SensorList.Add(new Sensor(readLine.Split(',')[0], readLine.Split(',')[1], readLine.Split(',')[2], readLine.Split(',')[3], readLine.Split(',')[4], readLine.Split(',')[5]));
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void OnStartService()
        {
            cfgservice = new GetConfigService { SensorList = SensorList };
            DirectoryService.Start(cfgservice);
            _publisher = new Publisher();
            _publisher.Connect();
            
        }

        private void OnStopService()
        {
            DirectoryService.Stop();
            _publisher.Disconnect();
        }

        #endregion

        #region Event Handlers


        private void SensorList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.NewItems != null)
                foreach (Sensor item in e.NewItems)
                    item.PropertyChanged += Unit_PropertyChanged;

            if (e.OldItems != null)
                foreach (Sensor item in e.OldItems)
                    item.PropertyChanged -= Unit_PropertyChanged;
        }

        private void Unit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                var sensor = (Sensor) sender;
_publisher.Publish(sensor.Guid, e.PropertyName,
                    typeof (Sensor).GetProperty(e.PropertyName).GetValue(sensor).ToString());
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        #endregion

    }
}