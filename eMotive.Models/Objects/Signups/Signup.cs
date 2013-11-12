﻿using System;
using System.Collections.Generic;

namespace eMotive.Models.Objects.Signups
{
    public class Signup
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public DateTime CloseDate { get; set; }
        public Group Group { get; set; }
        public string AcademicYear { get; set; }
        public bool Closed { get; set; }

        public ICollection<Slot> Slots { get; set; }
    }
}
