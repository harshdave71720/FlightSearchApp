using System;

namespace FlightSearchApp.Models
{
    public class FlightSearch
    {
        public string Origin { get; set; }

        public string Destination { get; set; }

        public DateTime DepartureDate { get; set; }

        public SortByOptions SortBy { get; set; }
    }
}
