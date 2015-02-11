using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Documents;
using System.Windows.Media;
using GestionStationnement.Helpers;

namespace GestionStationnement.Models
{
    public class Controller: NotificationObject
    {
        #region Fields
        private IPAddress _ipAddress;
        private List<Sensor> _list;
        #endregion


        #region Constructor

        public Controller()
        {
            
        }
        #endregion
    }
}
