using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Extensions;
using MySql.Data.MySqlClient;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Signups;

namespace eMotive.Repository.Objects
{
    public class MySqlSignupRepository : ISignupRepository
    {
        private readonly string connectionString;

        public MySqlSignupRepository(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public IEnumerable<Group> FetchGroups()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                return connection.Query<Group>("SELECT * FROM `groups`;");
            }
        }

        public IEnumerable<Group> FetchGroups(IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                return connection.Query<Group>("SELECT * FROM `groups` WHERE `id` IN @ids;", new { ids = _ids });
            }
        }

        public IEnumerable<Signup> FetchSignupsByGroup(IEnumerable<int> _groupIds)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //Fetch all signup information which is assigned to any of the passed _groupIds
                var sql = "SELECT * FROM `signup` a INNER JOIN `groups` b ON a.`idGroup` = b.`id` WHERE `idGroup` IN  @ids;";
                var signUps = connection.Query<Signup, Group, Signup>(sql, (signupDTO, groupDTO) => { signupDTO.Group = groupDTO; return signupDTO; }, new { ids = _groupIds });

                if (signUps.HasContent())
                {
                    //get all signupIds
                    var qIds = signUps.Select(n => n.id);

                    //select all slots which belong to the passed in signup ids
                    sql = "SELECT * FROM `slot` WHERE `idSignUp` in @ids;";
                    var slots = connection.Query<Slot>(sql, new { ids = qIds });

                    if (slots.HasContent())
                    {
                        //get all slot ids
                        qIds = slots.Select(n => n.id);

                        //pull out any applicants assigned to any of the slot ids we have passed in
                       // sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.id=b.id WHERE a.`idSlot` IN @ids;";
                        sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.idUser=b.id WHERE a.`idSlot` IN @ids;";
                        var userSignups = connection.Query<UserSignup>(sql, new { ids = qIds });

                        if (userSignups.HasContent())
                        {
                            //build a dictionary of userSignups against slot id
                            var signDict = userSignups.GroupBy(k => k.IdSlot, v => v).ToDictionary(k => k.Key, v => v.ToList());

                            //loop through slots and add all userSignups to a slot if a match is found
                            foreach (var slot in slots)
                            {
                                foreach (var userSignup in signDict)
                                {
                                    if (slot.id == userSignup.Key)
                                        slot.UsersSignedUp = userSignup.Value;

                                }
                            }
                        }

                        //build a dictionary of slots against signupid
                        var slotDict = slots.GroupBy(k => k.IdSignUp, v => v).ToDictionary(k => k.Key, v => v.ToList());

                        //loop through signups and add all slots to a signup if a match is found
                        foreach (var si in signUps)
                        {
                            foreach (var slot in slotDict)
                            {
                                if (si.id == slot.Key)
                                    si.Slots = slot.Value;
                            }
                        }
                    }
                }

                return signUps;

            }
        }

        public IEnumerable<Signup> FetchAll()
        {
            var groups = FetchGroups();

            if (!groups.HasContent())
                return null;

            return FetchSignupsByGroup(groups.Select(n => n.ID));
        }

        public Signup FetchSignup(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //Fetch the signup which has the passed id
                var sql = "SELECT * FROM `signup` a INNER JOIN `groups` b ON a.`idGroup` = b.`id` WHERE a.`id` = @id;";
                var signUp = connection.Query<Signup, Group, Signup>(sql, (signupDTO, groupDTO) => { signupDTO.Group = groupDTO; return signupDTO; }, new { id = _id }).SingleOrDefault();

                if (signUp != null)
                {

                    //select all slots which belong to the passed in signup ids
                    sql = "SELECT * FROM `slot` WHERE `idSignUp` = @id;";
                    var slots = connection.Query<Slot>(sql, new { id = signUp.id });

                    if (slots.HasContent())
                    {
                        //get all slot ids
                        var qIds = slots.Select(n => n.id);

                        //pull out any applicants assigned to any of the slot ids we have passed in
                        //    sql = string.Format("SELECT * FROM `{0}`.`applicant_has_slots` WHERE `idSlot` IN @ids;", DatabaseName);

                        sql = @"SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate` FROM `UserHasSlots` a INNER JOIN `users` b ON a.idUser=b.id WHERE a.`idSlot` IN @ids;";

                        var userSignups = connection.Query<UserSignup>(sql, new { ids = qIds });

                        if (userSignups.HasContent())
                        {
                            //build a dictionary of userSignups against slot id
                            var signDict = userSignups.GroupBy(k => k.IdSlot, v => v).ToDictionary(k => k.Key, v => v.ToList());

                            //loop through slots and add all userSignups to a slot if a match is found
                            foreach (var slot in slots)
                            {
                                foreach (var userSignup in signDict)
                                {
                                    if (slot.id == userSignup.Key)
                                        slot.UsersSignedUp = userSignup.Value;

                                }
                            }
                        }

                        signUp.Slots = slots;

                    }
                }

                return signUp;

            }
        }

        public int GetSignupIdFromSlot(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string query = "SELECT CAST(`idSignUp` AS UNSIGNED INTEGER) FROM `slot` WHERE `id`=@idSlot;";

                return Convert.ToInt32(connection.Query<ulong>(query, new { idSlot = _id }).Single());
            }
        }

        public Group FetchSignupGroup(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT b.`Id`, b.`Name`, b.`DisabilitySignups` FROM `signup` a INNER JOIN `groups` b ON a.`idGroup`=b.`id` WHERE a.`id`=@signupId";

                return connection.Query<Group>(sql, new {signupId = _id}).SingleOrDefault();
            }
        }

        public UserSignup FetchUserSignup(int _userId, int _groupId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT a.*, b.`Description`, b.`idSignUp` FROM `userhasslots` a INNER JOIN `slot` b ON a.`idSlot` = b.`id` WHERE idSlot IN (SELECT `id` FROM `slot` WHERE idSignUp IN (SELECT `id` FROM `signup` WHERE idGroup=@idGroup)) AND idUser=@idUser";

                return connection.Query<UserSignup>(sql, new { idGroup = _groupId, idUser = _userId }).SingleOrDefault();
            }
        }

        public UserSignup FetchUserSignup(int _userId, IEnumerable<int> _groupIds)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT a.*, b.`Description`, b.`idSignUp` FROM `userhasslots` a INNER JOIN `slot` b ON a.`idSlot` = b.`id` WHERE idSlot IN (SELECT `id` FROM `slot` WHERE idSignUp IN (SELECT `id` FROM `signup` WHERE idGroup IN @idGroups)) AND idUser=@idUser";

                return connection.Query<UserSignup>(sql, new { idGroups = _groupIds, idUser = _userId }).SingleOrDefault();
            }
        }


        public bool SignupToSlot(int _idSlot, int _idUser, DateTime _signupDate, out int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var query = "SELECT CAST(COUNT(*) AS UNSIGNED INTEGER) FROM `userhasslots` WHERE `idSlot`=@idSlot AND `idUser`=@idUser;";
                //user has already signed up for this slot! We should never get here, but this is a failsafe
                if (Convert.ToInt32(connection.Query<ulong>(query, new { idSlot = _idSlot, idUser = _idUser }).SingleOrDefault()) > 0)
                {
                    _id = -1;
                    return false;
                }

                query = "INSERT INTO `userhasslots` (`idSlot`,`idUser`,`SignUpDate`) VALUES (@idSlot, @idUser, @SignupDate);";

                var rowsAffected = connection.Execute(query, new { idSlot = _idSlot, idUser = _idUser, SignupDate = _signupDate });

                if (rowsAffected > 0)
                {
                    query = "SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);";

                    var id = connection.Query<ulong>(query).Single();

                    if (id > 0)
                    {
                        _id = Convert.ToInt32(id);
                        return true;
                    }
                }

                _id = -1;
                return false;
            }
        }

        public bool CancelSignupToSlot(int _idSlot, int _idUser)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string query = "DELETE FROM `userhasslots` WHERE `idSlot`=@idSlot AND `idUser`=@idUser;";

                var rowsAffected = connection.Execute(query, new { idSlot = _idSlot, idUser = _idUser });

                return rowsAffected > 0;
            }
        }

        public bool AddUserToGroup(int _userId, int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "INSERT INTO `userhasgroups` (`IdGroup`,`IdUser`) VALUES (@idGroup, @idUser);";

                var success = connection.Execute(sql, new { idGroup = _id, idUser = _userId }) > 0;

                return success;
            }
        }
        /*                    var id = connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    _id = Convert.ToInt32(id);
                    var insertObj = _user.Roles.Select(n => new { UserId = id, RoleId = n.ID });
                    sql = "INSERT INTO `UserHasRoles` (`UserID`, `RoleId`) VALUES (@UserId, @RoleId);";*/
        public bool AddUserToGroups(int _userId, IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "INSERT INTO `userhasgroups` (`IdGroup`,`IdUser`) VALUES (@idGroup, @idUser);";
                var insertObj = _ids.Select(n => new { idUser = _userId, idGroup = n });
                var success = connection.Execute(sql, insertObj) > 0;

                return success;
            }     
        }
    }
}
