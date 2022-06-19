using FlightSearchApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightSearchApp.Repositories
{
    public class InMemoryFlightRepository : IFlightRepository
    {
        private List<Flight> _flights = new List<Flight>();
        private static int _id = 1;
        public void AddFlights(IEnumerable<Flight> flights)
        {
            foreach (var flight in flights)
            { 
                flight.Id = _id++;
                _flights.Add(flight);
            }
        }

        public IEnumerable<Flight> GetAllFlights(string source, DateTime startTime, TimeSpan minWait, TimeSpan maxWait)
        {
            return _flights
                    .Where(
                        f => f.Info.Origin.Equals(source, StringComparison.OrdinalIgnoreCase) 
                            && f.Info.DepartureTime > startTime.Add(minWait)
                            && f.Info.DepartureTime < startTime.Add(maxWait)
                          );
        }

        public IEnumerable<Flight> GetAllFlights(string source, DateTime startTime)
        {
            return _flights
                    .Where(
                        f => f.Info.Origin.Equals(source, StringComparison.OrdinalIgnoreCase) 
                            && f.Info.DepartureTime.Date == startTime
                          );
        }

        public void ClearData()
        {
            _flights = new List<Flight>();
        }
    }
}
