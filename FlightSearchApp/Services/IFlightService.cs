using FlightSearchApp.Models;
using System.Collections.Generic;
using System.IO;

namespace FlightSearchApp.Services
{
    public interface IFlightService
    {
        void AddFlights(string providerName, Stream file);

        List<FlightPlan> GetFlightPlans(FlightSearch flightSearch);

        void ClearData();
    }
}
