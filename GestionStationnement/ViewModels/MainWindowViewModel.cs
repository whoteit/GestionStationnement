using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GestionStationnement.Helpers;
using GestionStationnement.Models;

namespace GestionStationnement.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Properties

        private ObservableCollection<Sensor> _sensorlist;
        private string _imagePath;

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
        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            _loadPlanSourceCommand = LoadPlanSourceCommand;
            _addSensorTestCommand = AddSensorTestCommand;
            _saveConfigCommand = SaveConfigCommand;
            _loadConfigCommand = LoadConfigCommand;
            SensorList = new ObservableCollection<Sensor>();
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
            var test = new Sensor("1","true","0","0");
            SensorList.Add(test);
        }

        private void OnSaveConfig()
        {
            using (var file = new StreamWriter("Repertoire.cfg"))
            {
                file.WriteLine(_imagePath);
                foreach (var sensor in SensorList)
                {
                    file.WriteLine("{0},{1},{2},{3}",sensor.LogicalId,sensor.IsOccupied,sensor.CoordinateX,sensor.CoordinateY);
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


                if (File.Exists(filePath))
                {
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
                            if (readLine.Length != 0)
                            {
                                SensorList.Add(new Sensor(readLine.Split(',')[0],readLine.Split(',')[1], readLine.Split(',')[2], readLine.Split(',')[3]));
                            }

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        #endregion

        
    }
}