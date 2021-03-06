﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace GestionStationnement.Models
{
    static class DirectoryService
    {
        private static readonly Uri RetrieveInitialConfigUri = new Uri("http://localhost:8087/Retrievecfg");
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
               _configHost.Open();
             }
            _updateHost = new ServiceHost(typeof(SensorUpdateService));
            _updateHost.Open();
        }

        public static void Stop()
        {
            _configHost.Close();
            _updateHost.Close();
        }

    }

    [ServiceContract]
    public interface IGetSensorConfig
    {
        [OperationContract]
        ObservableCollection<Sensor> GetSensorConfig();

        [OperationContract]
        Byte[] GetImageSource();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class GetConfigService : IGetSensorConfig
    {
        public ObservableCollection<Sensor> SensorList { get; set; }
        public Byte[] ImageSource { get; set; }

        public ObservableCollection<Sensor> GetSensorConfig()
        {
            return SensorList;
        }

        public Byte[] GetImageSource()
        {
            return ImageSource;
        }

    }

    interface IUpdateCallback
    {
        [OperationContract(IsOneWay = true)]
        void SensorUpdate(Guid guid, string propertyname, string propertyvalue);
    }

    [ServiceContract(CallbackContract = typeof(IUpdateCallback), SessionMode = SessionMode.Required)]
    public interface ISensorUpdateService
    {
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void Subscribe();
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void Unsubscribe();
        [OperationContract(IsOneWay = false)]
        void PublishSensorUpdate(Guid guid, string propertyname, string propertyvalue);

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SensorUpdateService : ISensorUpdateService
    {
        public delegate void SensorUpdateEventHandler(object sender, ServiceEventArgs e);
        private readonly List<IUpdateCallback> _callbackList = new List<IUpdateCallback>();  
        public static event SensorUpdateEventHandler SensorUpdateEvent;
        private IUpdateCallback _serviceCallback;
        private SensorUpdateEventHandler _updateHandler;


        public void Subscribe()
        {
            _serviceCallback = OperationContext.Current.GetCallbackChannel<IUpdateCallback>();
            _callbackList.Add(_serviceCallback);
            _updateHandler = SensorUpdateHandler;
            SensorUpdateEvent += _updateHandler;
        }

        public void Unsubscribe()
        {
            _serviceCallback = OperationContext.Current.GetCallbackChannel<IUpdateCallback>();
            _callbackList.Remove(_serviceCallback);
            SensorUpdateEvent -= _updateHandler;
        }

        public void PublishSensorUpdate(Guid guid, string propertyname, string propertyvalue)
        {
            if (_callbackList.Count <= 0) return;
            var se = new ServiceEventArgs {Guid = guid, PropertyName = propertyname, PropertyValue = propertyvalue};
            SensorUpdateEvent(this, se);
        }

        private void SensorUpdateHandler(object sender, ServiceEventArgs se)
        {
            _serviceCallback.SensorUpdate(se.Guid, se.PropertyName, se.PropertyValue);
        }

    }

    public class ServiceEventArgs : EventArgs
    {
        public Guid Guid;
        public string PropertyName;
        public string PropertyValue;
    }

    
}
