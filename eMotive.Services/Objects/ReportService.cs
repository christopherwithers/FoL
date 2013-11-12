using System.Collections.Generic;
using Dapper;
using MySql.Data.MySqlClient;
using eMotive.Models.Objects.Users;
using eMotive.Services.Interfaces;

namespace eMotive.Services.Objects
{
    public class ReportService : IReportService
    {
        private readonly string connectionString;

        public ReportService(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public IEnumerable<User> FetchUsersNotSignedUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT a.* FROM `users` a INNER JOIN `userhasroles` b ON a.`ID` = b.`UserId` WHERE b.RoleID=4 AND `ID` NOT IN (SELECT `IdUser` FROM `userhasslots`);";
                return connection.Query<User>(sql);
            }
        }
    }
}
