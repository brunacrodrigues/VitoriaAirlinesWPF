using System.Reflection.PortableExecutable;

namespace VitoriaAirlinesLibrary.Models
{
    public class Airport
    {
        public int Id { get; set; }
        public string IATA { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
