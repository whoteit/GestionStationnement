﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace GestionStationnement.Models
{
    static class DirectoryService
    {
        private static readonly Uri RetrieveInitialConfigUri = new Uri("http://localhost:8086/Retrievecfg");
        private static readonly Uri UpdateUri = new Uri("http://localhost:8081/");
        private static ServiceHost _configHost;
        private static ServiceHost _updateHost;

        public static void Start(GetConfigService configService)
        {
            _configHost = new ServiceHost(configService, RetrieveInitialConfigUri);
            {
                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    MetadataExporter = {PolicyVersion = PolicyVersion.Policy15},
                    
                };

                _configHost.Description.Behaviors.Add(smb);
                //_configHost.Open();
             }
            _updateHost = new ServiceHost(typeof(SensorUpdateService));
            // Start the server
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

    interface IUpdateCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnSensorUpdate(Sensor sensor);
    }

    [ServiceContract(CallbackContract = typeof(IUpdateCallback))]
    public interface ISensorUpdateService
    {
        [OperationContract]
        void SensorUpdate(Sensor sensor);

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SensorUpdateService : ISensorUpdateService
    {
        private static readonly Dictionary<string, OperationContext> Subscribers = new Dictionary<string, OperationContext>();
        public void SensorUpdate(Sensor sensor)
        {
            if (Subscribers.Count <= 0) return;
            foreach (var proxy in Subscribers.Select(item => item.Value.GetCallbackChannel<IUpdateCallback>()))
            {
                proxy.OnSensorUpdate(sensor);
            }
        }
    }

    

   

}
