using ServiceStack.DataAnnotations;

namespace eMotive.Repository.Objects.Users
{
    public class User
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Index(Unique = true)]
        public string Username { get; set; }

        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public bool Archived { get; set; }
    }
}
