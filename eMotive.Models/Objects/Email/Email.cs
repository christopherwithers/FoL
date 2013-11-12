using FluentValidation.Attributes;
using eMotive.Models.Validation.Email;

namespace eMotive.Models.Objects.Email
{
    [Validator(typeof(EmailValidator))]
    public class Email
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}
