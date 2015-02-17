using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using GestionStationnement.Helpers;

namespace GestionStationnement.Models
{
    public class Sensor : NotificationObject
    {
        #region Fields
        private int _logicalId;
        private string _ipAddress;
        private bool _isOccupied;
        private double _coordinateX;
        private double _coordinateY;
        #endregion

        #region Properties

        public int LogicalId
        {
            get { return _logicalId; }
            set
            {
                if (_logicalId != value)
                {
                    _logicalId = value;
                    RaisePropertyChanged(()=>LogicalId);
                }
            }
        }
        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    RaisePropertyChanged(() => IpAddress);
                }
            }
        }
        public double CoordinateX
        {
            get { return _coordinateX; }
            set
            {
                if (_coordinateX != value)
                {
                    _coordinateX = value;
                    RaisePropertyChanged(() => CoordinateX);
                }
            }
        }
        public double CoordinateY
        {
            get { return _coordinateY; }
            set
            {
                if (_coordinateY != value)
                {
                    _coordinateY = value;
                    RaisePropertyChanged(() => CoordinateY);
                }
            }
        }
        public bool IsOccupied
        {
            get { return _isOccupied; }
            set
            {
                if (_isOccupied != value)
                {
                    _isOccupied = value;
                    RaisePropertyChanged(() => IsOccupied);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Used to load sensors from a cfg file
        /// </summary>
        /// <param name="logicalId"></param>
        /// <param name="isOccupied"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Sensor(string logicalId,string isOccupied,string x, string y)
        {
            LogicalId = Convert.ToInt32(logicalId);
            IsOccupied = Convert.ToBoolean(isOccupied);
            CoordinateX = Convert.ToDouble(x);
            CoordinateY = Convert.ToDouble(y);
        }

        /// <summary>
        /// Constructor using default parameters
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="logicalId"></param>
        /// <param name="isOccupied"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Sensor(string ipAddress, int logicalId, bool isOccupied, double x, double y)
        {
            IpAddress = ipAddress;
            LogicalId = logicalId;
            IsOccupied = isOccupied;
            CoordinateX = x;
            CoordinateY = y;
        }

        public Sensor()
        {
            
        }


        #endregion
        #region Commands

        /// <summary>
        /// Probe controller for sensor status
        /// </summary>
        /// <returns></returns>
        public bool GetStatus()
        {
            //Probe controller for status using logicalID;
            return true;
        }
        #endregion
    }
}
