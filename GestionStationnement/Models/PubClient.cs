using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using GestionStationnement.Helpers;
using System.Text;
using System.Threading.Tasks;

namespace GestionStationnement.Models
{
    class Publisher : ISensorUpdateServiceCallback
    {
        private InstanceContext _site;
        private SensorUpdateServiceClient _client;
        public void Connect()
        {
            _site = new InstanceContext(new Publisher());
            _client = new SensorUpdateServiceClient(_site);

        }

        public void Publish(Guid guid, string propertyname, string propertyvalue)
        {
            
            _client.PublishSensorUpdateAsync(guid, propertyname, propertyvalue);
        }

        public void Disconnect()
        {
            _client.Abort();
        }

        public void SensorUpdate(Sensor sensor)
        {
            throw new NotImplementedException();
        }
    }
}
