using System;

namespace FlightSearchApp.Services
{
    public interface IAirportTimeConverter
    {
        DateTime ConvertToUtc(string airportCode, DateTime dateTime);

        DateTime ConvertToLocal(string airportCode, DateTime dateTime);
    }
}
