﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using GestionStationnement.Helpers;

namespace GestionStationnement.Models
{
    public class Sensor : NotificationObject
    {
        #region Fields
        private int _logicalId;
        private Guid _guid;
        private string _ipAddress;
        private string _friendlyName;
        private bool _isOccupied;
        private double _coordinateX;
        private double _coordinateY;
        private readonly Stopwatch _stopwatch;
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
        public string FriendlyName
        {
            get { return _friendlyName; }
            set
            {
                if (_friendlyName != value)
                {
                    _friendlyName = value;
                    RaisePropertyChanged(() => FriendlyName);
                }
            }
        }
        public double CoordinateX
        {
            get { return _coordinateX; }
            set
            {
                if (_coordinateX == value) return;
                _coordinateX = value;
                RaisePropertyChanged(() => CoordinateX);
            }
        }

        public Guid Guid
        {
            get { return _guid; }
            set
            {
                if (_guid == value) return;
                _guid = value;
                RaisePropertyChanged(() => Guid);
            }
        }

        public double CoordinateY
        {
            get { return _coordinateY; }
            set
            {
                if (_coordinateY == value) return;
                _coordinateY = value;
                RaisePropertyChanged(() => CoordinateY);
            }
        }
        public bool IsOccupied
        {
            get { return _isOccupied; }
            set
            {
                if (_isOccupied == value) return;
                _isOccupied = value;
                _stopwatch.Restart();
                RaisePropertyChanged(() => IsOccupied);
            }
        }
        public TimeSpan TimeInState
        {
            get { return _stopwatch.Elapsed; }

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
        public Sensor(string ipAddress, string logicalId,string isOccupied, string friendlyName, string x, string y)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            IpAddress = ipAddress;
            FriendlyName = friendlyName;
            LogicalId = Convert.ToInt32(logicalId);
            IsOccupied = Convert.ToBoolean(isOccupied);
            CoordinateX = Convert.ToDouble(x);
            CoordinateY = Convert.ToDouble(y);
            Guid = Guid.NewGuid();
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
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            IpAddress = ipAddress;
            LogicalId = logicalId;
            IsOccupied = isOccupied;
            CoordinateX = x;
            CoordinateY = y;
            Guid = Guid.NewGuid();
        }


        #endregion
        #region Commands

        /// <summary>
        /// Probe controller for sensor status
        /// </summary>
        /// <returns></returns>
        public void GetStatus()
        {
            while (true)
            {
                var uri = new Uri(string.Format("https://{0}/GetStatus{1}", IpAddress, LogicalId));
                var webRequest = (HttpWebRequest) WebRequest.Create(uri);
                var webResponse = (HttpWebResponse) webRequest.GetResponse();

                using (var responseStream = webResponse.GetResponseStream())
                {
                    if (responseStream == null) return;
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var responseString = reader.ReadToEnd();
                    IsOccupied = Convert.ToBoolean(responseString);
                }
                Thread.Sleep(5000);
            }

        }
        #endregion
    }
}
