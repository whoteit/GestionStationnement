using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Threading;
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
        #region ConfigGeneral

        #region Properties

        private string _pendingIpAddress = "192.168.1.100";
        private string _pendingLogicalId = "1";
        private string _pendingFriendlyName = "A1";
        private bool _cancellationPending;
        private Publisher _publisher;
        private GetConfigService _cfgservice;

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

        private MessageHandler _messageHandler;
        public MessageHandler MessageHandler
        {
            get { return _messageHandler; }
            set
            {
                if (_messageHandler == value) return;
                _messageHandler = value;
                RaisePropertyChanged(() => MessageHandler);
            }

        }

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
        [DataMember]
        public ImageSource PlanImageSource
        {
            get { return _planImageSource; }
            set
            {
                _planImageSource = value;
                RaisePropertyChanged(() => PlanImageSource);

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

        private ICommand _startSensorPollCommand;
        public ICommand StartSensorPollCommand
        {
            get { return _startSensorPollCommand ?? (_startSensorPollCommand = new DelegateCommand(OnStartPoll)); }
        }

        private ICommand _stopSensorPollCommand;

        public ICommand StopSensorPollCommand
        {
            get { return _stopSensorPollCommand ?? (_stopSensorPollCommand = new DelegateCommand(OnStopPoll)); }
        }
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {

            SensorList = new ObservableCollection<Sensor>();
            RecipientList = new ObservableCollection<string>();
            SensorList.CollectionChanged += SensorList_CollectionChanged;
            MessageHandler = new MessageHandler();

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
                if (EmailOnError) SendEmail("Gestion Stationnement : Erreur", ex.Message);
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
                    if (temp != null)
                    {
                        PlanImageSource = new BitmapImage(new Uri(temp));
                        _imagePath = temp;
                    }
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
            try
            {
                var bytes = File.ReadAllBytes(_imagePath);
                _cfgservice = new GetConfigService { SensorList = SensorList, ImageSource = bytes};
                DirectoryService.Start(_cfgservice);
                _publisher = new Publisher();
                _publisher.Connect();
                MessageHandler.SendMessage("Service démarré avec succès aux ports 8080 et 8087. En attente de connexions de clients.");

            }
            catch (Exception ex)
            {
                MessageHandler.SendMessage("Une erreur s'est produite : " + ex.Message);
                if(EmailOnError)
                    SendEmail("Gestion de Stationnement : Erreur",ex.Message);
            }

            
        }

        private void OnStopService()
        {
            DirectoryService.Stop();
            _publisher.Disconnect();
            MessageHandler.SendMessage("Service arrêté avec succès.");
        }

        private void OnStartPoll()
        {
            _cancellationPending = false;
            ThreadPool.QueueUserWorkItem(o=> StartPollingOperation());
            MessageHandler.SendMessage("Sondage des capteurs démarré avec succès.");
        }

        private void OnStopPoll()
        {
            _cancellationPending = true;
            MessageHandler.SendMessage("Sondage de capteurs arrêté avec succès.");
        }

        private void StartPollingOperation()
        {
            while (!_cancellationPending)
            {
                foreach (var sensor in SensorList)
                {
                    if (_cancellationPending)
                        return;
                    try
                    {
                        sensor.IsOccupied = sensor.GetStatus();
                    }
                    catch (Exception ex)
                    {
                        MessageHandler.SendMessage(ex.Message);
                        if(EmailOnError)
                            SendEmail("Gestion de Stationnement : Erreur",ex.Message);
                    }
                }
                Thread.Sleep(5000);
            }

        }

        public Byte[] BufferFromImage(BitmapImage imageSource)
        {
            var stream = imageSource.StreamSource;
            Byte[] buffer = null;
            if (stream == null || stream.Length <= 0) return buffer;
            using (var br = new BinaryReader(stream))
            {
                buffer = br.ReadBytes((Int32)stream.Length);
            }

            return buffer;
        }

        #endregion

        #region Event Handlers

        private void SensorList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.NewItems != null)
                foreach (Sensor item in e.NewItems)
                    item.PropertyChanged += Unit_PropertyChanged;

            if (e.OldItems == null) return;
            foreach (Sensor item in e.OldItems)
                item.PropertyChanged -= Unit_PropertyChanged;
        }

        private void Unit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                var sensor = (Sensor)sender;

                if (e.PropertyName == "CoordinateX" || e.PropertyName == "CoordinateY" || e.PropertyName == "IsOccupied")
                {
                    if (_publisher == null) return;
                    _publisher.Publish(sensor.Guid, e.PropertyName, typeof (Sensor).GetProperty(e.PropertyName).GetValue(sensor).ToString());
                }

                if (e.PropertyName == "TimeInState")
                {
                    var timelimit = new TimeSpan(0, Convert.ToInt32(TempsLimite), 0);
                    if (sensor.TimeInState.CompareTo(timelimit) == 0 && sensor.IsOccupied && EmailOnTime)
                    {
                        var body = string.Format("La place {0} a été occupée plus longtemps que le temps limite autorisé.",sensor.FriendlyName);
                        SendEmail("Gestion Stationnement : Alerte", body);
                        MessageHandler.SendMessage(body);
                    }

                }
            

            }
            catch (Exception ex)
            {
                MessageHandler.SendMessage("Une erreure s'est produite lors de la communication avec le(s) client(s). " + ex.Message);
            }

        }

        #endregion

        #endregion

        #region Alertes

        #region Properties
        private string _tempsLimite = "90";
        public string TempsLimite
        {
            get { return _tempsLimite; }
            set
            {
                if (_tempsLimite == value) return;
                _tempsLimite = value;
                RaisePropertyChanged(() => TempsLimite);
            }
        }

        private string _addresseServeur = "smtp.gmail.com";
        public string AddresseServeur
        {
            get { return _addresseServeur; }
            set
            {
                if (_addresseServeur == value) return;
                _addresseServeur = value;
                RaisePropertyChanged(() => AddresseServeur);
            }
        }
        private string _portServer = "587";
        public string PortServer
        {
            get { return _portServer; }
            set
            {
                if (_portServer == value) return;
                _portServer = value;
                RaisePropertyChanged(() => PortServer);
            }
        }

        private string _adresseSource = "wassim.hoteit.92@gmail.com";
        public string AdresseSource
        {
            get { return _adresseSource; }
            set
            {
                if (_adresseSource == value) return;
                _adresseSource = value;
                RaisePropertyChanged(() => AdresseSource);
            }
        }
        private string _motDePasse;
        public string MotDePasse
        {
            get { return _motDePasse; }
            set
            {
                if (_motDePasse == value) return;
                _motDePasse = value;
                RaisePropertyChanged(() => MotDePasse);
            }
        }

        private string _pendingAddr;
        public string PendingAddr
        {
            get { return _pendingAddr; }
            set
            {
                if (_pendingAddr == value) return;
                _pendingAddr = value;
                RaisePropertyChanged(() => PendingAddr);
            }
        }

        private bool _emailOnError;
        public bool EmailOnError
        {
            get { return _emailOnError; }
            set
            {
                if (_emailOnError == value) return;
                _emailOnError = value;
                RaisePropertyChanged(() => EmailOnError);
            }
        }

        private bool _emailOnTime = true;
        public bool EmailOnTime
        {
            get { return _emailOnTime; }
            set
            {
                if (_emailOnTime == value) return;
                _emailOnTime = value;
                RaisePropertyChanged(() => EmailOnTime);
            }
        }

        private ObservableCollection<string> _recipientList;
        public ObservableCollection<string> RecipientList
        {
            get { return _recipientList; }
            set
            {
                if (_recipientList == value) return;
                _recipientList = value;
                RaisePropertyChanged(() => RecipientList);
            }
        }

        #endregion

        #region Commands
        private ICommand _addRecipientCommand;
        public ICommand AddRecipientCommand
        {
            get { return _addRecipientCommand ?? (_addRecipientCommand = new DelegateCommand(OnAddRecipient)); }
        }

        private ICommand _removeRecipientCommand;
        public ICommand RemoveRecipientCommand
        {
            get { return _removeRecipientCommand ?? (_removeRecipientCommand = new DelegateCommand(OnRemoveRecipient)); }
        }

        #endregion

        #region Command Handlers

        public void OnAddRecipient()
        {
            if (PendingAddr == "") return;
            RecipientList.Add(PendingAddr);
            PendingAddr = string.Empty;
        }

        public void OnRemoveRecipient()
        {
            if (PendingAddr == "") return;
            var rec = RecipientList.FirstOrDefault(s => s == PendingAddr);
            if (rec != null)
                RecipientList.Remove(rec);

        }

        private void SendEmail(string subject, string body)
        {
            var client = new SmtpClient
            {
                Host = AddresseServeur,
                Port = Convert.ToInt32(PortServer),
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(AdresseSource, MotDePasse),
                Timeout = 15000
            };


            foreach (var rec in RecipientList)
            {
                var from = new MailAddress(AdresseSource, "Gestion de Stationnement", System.Text.Encoding.UTF8);
                var to = new MailAddress(rec);

                var message = new MailMessage(from, to)
                {
                    Body = body,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    Subject = subject,
                    SubjectEncoding = System.Text.Encoding.UTF8
                };

                client.Send(message);


            }

        }

        #endregion

        #endregion

    }
}