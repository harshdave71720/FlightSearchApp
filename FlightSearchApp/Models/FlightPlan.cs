using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightSearchApp.Models
{
    public class FlightPlan
    {
        public double Cost { get; private set; }

        public double DurationInHours { get; private set; }

        public IEnumerable<FlightInfo> Flights => _flights;

        private LinkedList<FlightInfo> _flights { get; set; } = new LinkedList<FlightInfo>();

        private int _length;

        public int Length => _length;

        public DateTime DepartureTime 
        {
            get
            {
                if (_flights?.Count() == 0)
                    throw new Exception();

                return _flights.First().DepartureTime;
            }
        }
        
        public void AddFlightToBeginning(FlightInfo flight)
        {
            _flights.AddFirst(flight);
            Cost += flight.Price;
            DurationInHours += flight.ArrivalTime.Subtract(flight.DepartureTime).TotalHours;
            _length++;
        }
    }
}
