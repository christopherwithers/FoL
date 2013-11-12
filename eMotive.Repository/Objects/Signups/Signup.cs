using System;
using System.Collections.Generic;

namespace eMotive.Repository.Objects.Signups
{
    public class Signup
    {
        public int id { get; set; }
        public DateTime Date { get; set; }
        public DateTime CloseDate { get; set; }
        public Group Group { get; set; }
        public string AcademicYear { get; set; }
        public bool Closed { get; set; }

        public IEnumerable<Slot> Slots { get; set; }
    }
}
