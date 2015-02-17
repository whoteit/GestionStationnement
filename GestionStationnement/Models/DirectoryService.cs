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
        private static ServiceHost _host;

        public static void Start(GetConfigService configService)
        {
            _host = new ServiceHost(configService, RetrieveInitialConfigUri);
            {
                // Enable metadata publishing.
                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    MetadataExporter = {PolicyVersion = PolicyVersion.Policy15},
                    
                };
                _host.Description.Behaviors.Add(smb);

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                _host.Open();

            }
        }

        public static void Stop()
        {
            _host.Close();
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


}
