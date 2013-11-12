namespace eMotive.Models.Objects.Signups
{
    public enum SlotStatus { Signup, Full, Closed, Clash, AlreadySignedUp }

    public class SlotState
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string Time { get; set; }
        public int TotalPlacesAvailable { get; set; }
        public int PlacesAvailable { get; set; }
        public bool Enabled { get; set; }

        public SlotStatus Status { get; set; } 
    }
}
