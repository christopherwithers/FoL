using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dapper;
using Extensions;
using MySql.Data.MySqlClient;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects;

namespace eMotive.Services
{
    
    public class NotificationService : INotificationService
    {
        private readonly ICollection<Message> messages;

        private readonly string connectionString;
        private readonly bool doLogging;

        public NotificationService(string _connectionString, string _enableLogging)
        {
            connectionString = _connectionString;

            if (!bool.TryParse(_enableLogging, out doLogging))
                doLogging = false;

            messages = new Collection<Message>();
        }

        public void Log(string _log)
        {
            if (!string.IsNullOrEmpty(_log))
                messages.Add(new Message { MessageType = MessageType.Log, Details = _log });
        }

        public void AddError(string _error)
        {
            if(!string.IsNullOrEmpty(_error))
                messages.Add(new Message {MessageType = MessageType.Error, Details = _error});
        }

        public void AddIssue(string _issue)
        {
            if (!string.IsNullOrEmpty(_issue))
                messages.Add(new Message { MessageType = MessageType.Issue, Details = _issue });
        }

        public IEnumerable<string> FetchErrors()
        {
            var errorMessages = messages.Where(n => n.MessageType == MessageType.Error);

            return errorMessages.HasContent() ? errorMessages.Select(n => n.Details) : null;
        }

        public IEnumerable<string> FetchIssues()
        {
            var errorMessages = messages.Where(n => n.MessageType == MessageType.Issue);

            return errorMessages.HasContent() ? errorMessages.Select(n => n.Details) : null;
        }

        public void CommitDatabaseLog()
        {
            if (!doLogging) return;

            var dbLoggableErrors = messages.Where(n => n.MessageType == MessageType.Log).ToList();

            if (!dbLoggableErrors.HasContent()) return;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

              //  using (var transactionScope = new TransactionScope())
              //  {
                    var insertObj = dbLoggableErrors.Select(n => new {Occurred = DateTime.Now, Error = n.Details});
                    conn.Execute("INSERT INTO `Log` (`Occurred`, `Error`) VALUES (@Occurred, @Error);", insertObj);

               // }

            }
        }
    }
}
