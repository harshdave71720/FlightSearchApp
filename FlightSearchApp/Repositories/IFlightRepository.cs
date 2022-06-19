using FlightSearchApp.Models;
using System;
using System.Collections.Generic;

namespace FlightSearchApp.Repositories
{
    public interface IFlightRepository
    {
        void AddFlights(IEnumerable<Flight> flights);

        IEnumerable<Flight> GetAllFlights(string source, DateTime startTime, TimeSpan minWait, TimeSpan maxWait);

        IEnumerable<Flight> GetAllFlights(string source, DateTime startTime);

        void ClearData();
    }
}
