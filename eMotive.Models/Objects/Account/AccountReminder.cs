using FluentValidation.Attributes;
using eMotive.Models.Validation.Account;

namespace eMotive.Models.Objects.Account
{
    [Validator(typeof(AccountReminderValidator))]
    public class AccountReminder
    {
        public string EmailAddress { get; set; }
    }
}
