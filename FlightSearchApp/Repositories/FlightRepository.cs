using FlightSearchApp.DbContexts;
using FlightSearchApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightSearchApp.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly FlightDbContext _flightDbContext;

        public FlightRepository(FlightDbContext flightDbContext)
        {
            _flightDbContext = flightDbContext;
        }

        public void AddFlights(IEnumerable<Flight> flights)
        {
            _flightDbContext.Flights.AddRange(flights);
            _flightDbContext.SaveChanges();
        }

        public void ClearData()
        {
            _flightDbContext.Flights.RemoveRange(_flightDbContext.Flights.ToArray());
            _flightDbContext.SaveChanges();
        }

        public IEnumerable<Flight> GetAllFlights(string source, DateTime startTime, TimeSpan minWait, TimeSpan maxWait)
        {
            return _flightDbContext.Flights
                    .Where(
                        f => f.Info.Origin.Equals(source)
                            && f.Info.DepartureTime > startTime.Add(minWait)
                            && f.Info.DepartureTime < startTime.Add(maxWait)
                          ).ToList();
        }

        public IEnumerable<Flight> GetAllFlights(string source, DateTime startTime)
        {
            return _flightDbContext.Flights
                    .Where(
                        f => f.Info.Origin.Equals(source)
                            && f.Info.DepartureTime.Date == startTime
                          ).ToList();
        }
    }
}
