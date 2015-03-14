using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace GestionStationnement.Helpers
{
    public interface IMessageHandler
    {
        void SendMessage(string msg);
    }

    public class MessageHandler : IMessageHandler
    {
        public ObservableCollection<LogMessage> Messages { get; set; }

        public MessageHandler()
        {
            Messages = new ObservableCollection<LogMessage>();
        }
        public void SendMessage(string msg)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => Messages.Add(new LogMessage(msg))));
        }
    }

    public class LogMessage
    {
        private string _message;
        private string _date;

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                }
            }
        }

        public string Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                }
            }
        }

        public LogMessage(string message)
        {
            Date = DateTime.Now.ToShortTimeString();
            Message = message;
        }

    }
}
