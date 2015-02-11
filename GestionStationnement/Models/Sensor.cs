using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionStationnement.Helpers;

namespace GestionStationnement.Models
{
    public class Sensor : NotificationObject
    {
        #region Fields
        private int _logicalId;
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

        public Sensor(string logicalId,string isOccupied,string x, string y)
        {
            LogicalId = Convert.ToInt32(logicalId);
            IsOccupied = Convert.ToBoolean(isOccupied);
            CoordinateX = Convert.ToDouble(x);
            CoordinateY = Convert.ToDouble(y);
        }
        #endregion
        #region Commands

        public bool GetStatus()
        {
            //Probe controller for status using logicalID;
            return true;
        }
        #endregion
    }
}
