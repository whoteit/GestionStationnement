using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace NotifyService
{
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
        public ObservableCollection<Sensor> SensorList { get; set; }
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
