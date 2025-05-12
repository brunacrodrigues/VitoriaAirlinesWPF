namespace VitoriaAirlinesLibrary.Models
{
    public class Airplane
    {
        public int Id { get; set; }

        public string Model { get; set; }

        public bool IsActive { get; set; }

        public int ExecutiveSeats { get; set; }

        public int EconomicSeats { get; set; }

        public List<Seat> Seats { get; set; }

        public string StatusText
        {
            get
            {
                return this.IsActive ? "Active" : "Inactive";
            }

        }
    }
}
