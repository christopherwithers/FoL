using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Objects
{//https://github.com/ServiceStack/ServiceStack.OrmLite
    //http://stackoverflow.com/questions/14480237/servicestack-ormlite-repository-pattern
    //https://groups.google.com/forum/#!msg/servicestack/1pA41E33QII/R-trWwzYgjEJ
    //http://xunitpatterns.com/Obscure%20Test.html#General
    //http://xunitpatterns.com/Obscure%20Test.html#General
    public class MySqlUserRepository : IUserRepository
    {
        private readonly string connectionString;
        private readonly string userFields;

        public MySqlUserRepository(string _connectionString)
        {
            connectionString = _connectionString;
            userFields = "`id`, `username`, `forename`, `surname`, `email`, `enabled`, `archived`";
        }

        public User New()
        {
            return new User();
        }

        public User Fetch(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `users` WHERE `id`=@id;", userFields);

                return connection.Query<User>(sql, new {id = _id}).SingleOrDefault();
            }
        }

        public IEnumerable<User> Fetch(IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `users` WHERE `id` in @ids;", userFields);

                return connection.Query<User>(sql, new { ids = _ids });
            }
        }

        public bool Create(User _user, out int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = "INSERT INTO `users` (`username`, `forename`, `surname`, `email`, `enabled`, `archived`) VALUES (@username, @forename, @surname, @email, @enabled, @archived);";

                var sqlParams = new
                    {
                        username = _user.Username,
                        forename = _user.Forename,
                        surname = _user.Surname,
                        email = _user.Email,
                        enabled = _user.Enabled,
                        archived = _user.Archived
                    };
                var transaction = connection.BeginTransaction();

                var success = connection.Execute(sql, sqlParams) > 0;

                var id =
                    connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);", transaction)
                              .SingleOrDefault();

                _id = Convert.ToInt32(id);

                transaction.Commit();

                return success & id > 0;
            }
        }

        public bool Update(User _user)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = "UPDATE `users` SET `username` = @username, `forename` = @forename, `surname`= @surname, `email` = @email, `enabled` = @enabled, `archived` = @archived WHERE `id` = @id";
                var sqlParams = new
                {
                    username = _user.Username,
                    forename = _user.Forename,
                    surname = _user.Surname,
                    email = _user.Email,
                    enabled = _user.Enabled,
                    archived = _user.Archived,
                    id = _user.ID
                };

                return connection.Execute(sql, sqlParams) > 0;
            }
        }

        public bool Delete(User _user)
        {
            _user.Enabled = false;
            _user.Surname = string.Empty;
            _user.Forename = string.Empty;
            _user.Archived = false;

            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = "UPDATE `users` SET `username` = @username, `forename` = @forename, `surname`= @surname, `email` = @email, `enabled` = @enabled, `archived` = @archived WHERE `id` = @id";
                var sqlParams = new
                {
                    username = _user.Username,
                    forename = string.Empty,
                    surname = string.Empty,
                    email = _user.Email,
                    enabled = false,
                    archived = false,
                    id = _user.ID
                };

                return connection.Execute(sql, sqlParams) > 0;
            }
        }
    }
}
