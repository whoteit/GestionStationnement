using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace GestionStationnement.Models
{
    static class DirectoryService
    {
        private static readonly Uri RetrieveInitialConfigUri = new Uri("http://localhost:8080/RetrieveConfig");
        private static readonly Uri UpdateUri = new Uri("net.tcp://localhost");
        private static ServiceHost _configHost;
        private static ServiceHost _updateHost;

        public static void Start(GetConfigService configService)
        {
            _configHost = new ServiceHost(configService, RetrieveInitialConfigUri);
            {
                // Enable metadata publishing.
                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    MetadataExporter = {PolicyVersion = PolicyVersion.Policy15},
                    
                };
                _configHost.Description.Behaviors.Add(smb);
                _configHost.Open();
             }

            var updatebinding = new NetTcpBinding();
            _updateHost = new ServiceHost(typeof(MyService), UpdateUri);
            _updateHost.AddServiceEndpoint(typeof(IMyContract), updatebinding, "");
            _updateHost.Open();

        }

        public static void Stop()
        {
            _configHost.Close();
        }

    }

    [ServiceContract]
    public interface IGetSensorConfig
    {
        [OperationContract]
        ObservableCollection<Sensor> GetSensorConfig();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class GetConfigService : IGetSensorConfig
    {
        public ObservableCollection<Sensor> SensorList { get; set; }
        
        public ObservableCollection<Sensor> GetSensorConfig()
        {
            return SensorList;
        }
    }

    public interface IMyContractCallback
    {
        [OperationContract]
        void OnCallback();
    }

    [ServiceContract(CallbackContract = typeof(IMyContractCallback))]
    public interface IMyContract
    {
        [OperationContract]
        Sensor UpdateSensor(Guid guid);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MyService : IMyContract
    {
        public ObservableCollection<Sensor> SensorList { get; set; }
        public Sensor UpdateSensor(Guid guid)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>();
            callback.OnCallback();
            return SensorList.First(sensor => sensor.Guid == guid);
        }
    }


}
