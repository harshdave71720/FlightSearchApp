using FlightSearchApp.Configurations;
using FlightSearchApp.Models;
using FlightSearchApp.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FlightSearchApp.Services
{
    public class FlightService : IFlightService
    {
        private readonly IAirportTimeConverter _airportTimeConverter;
        private readonly IFlightRepository _flightRepository;
        private readonly int _maxFlightsAllowed = 3;
        private readonly FlightSearchConfiguration _flightSearchConfiguration;

        public FlightService(IAirportTimeConverter airportTimeConverter
            , IFlightRepository flightRepository
            , FlightSearchConfiguration flightSearchConfiguration)
        {
            _airportTimeConverter = airportTimeConverter;
            _flightRepository = flightRepository;
            _flightSearchConfiguration = flightSearchConfiguration;
        }

        public void AddFlights(string providerName, Stream stream)
        {
            var flights = new List<Flight>();
            using (var reader = new StreamReader(stream))
            {
                // Skipping the line containing headers
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var nextLine = reader.ReadLine().Trim();
                    var seperator = nextLine.Contains('|') ? '|' : ',';
                    var flightInfo = nextLine.Split(seperator).Select(s => s.Trim()).ToArray();
                    flights.Add(new Flight
                    {
                        Info = new FlightInfo
                        {
                            Origin = flightInfo[0],
                            DepartureTime = ConvertToUtcDate(flightInfo[0], FixDateFormat(flightInfo[1])),
                            Destination = flightInfo[2],
                            ArrivalTime = ConvertToUtcDate(flightInfo[2], FixDateFormat(flightInfo[3])),
                            Price = (double)decimal.Parse(flightInfo[4], NumberStyles.Currency, CultureInfo.CreateSpecificCulture("en-US")),
                            Provider = providerName
                        }
                    });
                }
                reader.Close();
            }
            _flightRepository.AddFlights(flights);
        }

        private DateTime ConvertToUtcDate(string airportCode, string date)
        {
            return _airportTimeConverter
                .ConvertToUtc(
                    airportCode,
                    DateTime.ParseExact(
                        date,
                        "MM/dd/yyyy HH:mm:ss",
                        provider: null,
                        System.Globalization.DateTimeStyles.None));
        }

        //public List<List<Flight>> GetFlights(FlightSearch flightSearch)
        //{
        //    List<List<Flight>> allRoutes = new List<List<Flight>>();
        //    var finalFlights = new List<Flight>();
        //    var flights = _flightRepository
        //                        .GetAllFlights(flightSearch.Origin, flightSearch.DepartureDate)
        //                        .Where(f => 
        //                                    f.Destination
        //                                    .Equals(flightSearch.Destination, StringComparison.OrdinalIgnoreCase))
        //                        .ToList();
        //    foreach (var flight in flights)
        //    {
        //        if (PopulateNextFlights(flight, flightSearch, 1))
        //            finalFlights.Add(flight);
        //    }

        //    return finalFlights;
        //}

        //private bool PopulateNextFlights(Flight flight, FlightSearch flightSearch, int flightSequenceNumber)
        //{
        //    if (flightSequenceNumber > _maxFlightsAllowed)
        //        return false;

        //    if (flight.Destination.Equals(flightSearch.Destination, StringComparison.OrdinalIgnoreCase))
        //    {
        //        return true;
        //    }

        //    var nextFlights = _flightRepository
        //                            .GetAllFlights
        //                            (
        //                                flight.Destination, 
        //                                flight.ArrivalTime,
        //                                flightSearch.MinTimeBetweenFlights, 
        //                                flightSearch.MaxTimeBetweenFlights
        //                            );

        //    foreach (var nextFlight in nextFlights)
        //    {
        //        if (PopulateNextFlights(nextFlight, flightSearch, flightSequenceNumber + 1))
        //            flight.NextFlights.Add(nextFlight);
        //    }

        //    return true;
        //}

        public List<FlightPlan> GetFlightPlans(FlightSearch flightSearch)
        {
            List<FlightPlan> flightPlans = new List<FlightPlan>();
            var InitialFlights = _flightRepository
                                .GetAllFlights(flightSearch.Origin, flightSearch.DepartureDate).ToList();

            foreach (var flight in InitialFlights)
            {
                var nextPlans = GetNextFlightPlans(flight, flightSearch, 1);
                if (nextPlans == null)
                    continue;
                foreach (var plan in nextPlans)
                {
                    flightPlans.Add(plan);
                }
            }

            return flightSearch.SortBy == SortByOptions.Price
                ? flightPlans.OrderBy(f => f.Cost).ToList()
                : flightPlans.OrderBy(f => f.DepartureTime).ToList();
        }

        private IEnumerable<FlightPlan> GetNextFlightPlans(Flight flight, FlightSearch flightSearch, int flightSequenceNumber)
        {
            if (flightSequenceNumber > _maxFlightsAllowed)
                return null;

            if (flight.Info.Destination.Equals(flightSearch.Destination, StringComparison.OrdinalIgnoreCase))
            {
                var plan = new FlightPlan();
                var info = flight.Info;
                plan.AddFlightToBeginning(info);
                info.DepartureTime = _airportTimeConverter.ConvertToLocal(info.Origin, info.DepartureTime);
                info.ArrivalTime = _airportTimeConverter.ConvertToLocal(info.Destination, info.ArrivalTime);
                return new List<FlightPlan>() { plan };
            }

            var nextFlights = _flightRepository
                                    .GetAllFlights
                                    (
                                        flight.Info.Destination,
                                        flight.Info.ArrivalTime,
                                        TimeSpan.FromHours(_flightSearchConfiguration.MinHoursBetweenFlights),
                                        TimeSpan.FromHours(_flightSearchConfiguration.MaxHoursBetweenFlights)
                                    ).ToList();

            var flightPlans = new List<FlightPlan>();
            foreach (var nextFlight in nextFlights)
            {
                var nextPlans = GetNextFlightPlans(nextFlight, flightSearch, flightSequenceNumber + 1);
                if (nextPlans == null)
                    continue;
                foreach (var plan in nextPlans)
                {
                    var info = flight.Info;
                    plan.AddFlightToBeginning(info);
                    info.DepartureTime = _airportTimeConverter.ConvertToLocal(info.Origin, info.DepartureTime);
                    info.ArrivalTime = _airportTimeConverter.ConvertToLocal(info.Destination, info.ArrivalTime);
                    flightPlans.Add(plan);
                }
            }

            return flightPlans.Count > 0 ? flightPlans : null;
        }

        private string FixDateFormat(string dateString)
        {
            string fixedDate = "";
            string digitsVisited = "";
            for (int i = 0; i < dateString.Length; i++)
            {
                if (!IsDigit(dateString[i]))
                {
                    if (digitsVisited.Length == 0)
                    {
                        fixedDate += $"00{dateString[i]}";
                    }
                    else if (digitsVisited.Length == 1)
                    {
                        fixedDate += $"0{digitsVisited}{dateString[i]}";
                    }
                    else
                    {
                        fixedDate += $"{digitsVisited}{dateString[i]}";
                    }
                    digitsVisited = "";
                }
                else
                {
                    digitsVisited += dateString[i];
                }
            }

            fixedDate += digitsVisited;
            return fixedDate;
        }

        public void ClearData()
        {
            _flightRepository.ClearData();
        }

        private bool IsDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }
    }
}
