using System.Collections.Generic;
using TimeZoneConverter;
using System;
using System.IO;
using System.Linq;
using FlightSearchApp.Configurations;

namespace FlightSearchApp.Services
{
    public class AirportTimeConverter : IAirportTimeConverter
    {
        private IanaAirportConfig _ianaAirportConfig;
        private Dictionary<string, TimeZoneInfo> _airPortTimeZones;

        public AirportTimeConverter(IanaAirportConfig _ianaAirportConfig)
        {
            _airPortTimeZones = new Dictionary<string, TimeZoneInfo>();
            InitializeTimeZoneInformation(@_ianaAirportConfig.IanaAirPortCodesPath);
        }

        public DateTime ConvertToUtc(string airportCode, DateTime dateTime)
        {
            if (!_airPortTimeZones.ContainsKey(airportCode))
                throw new Exception($"Windows Time Zone Not Found For Airport : {airportCode}");

            return TimeZoneInfo.ConvertTimeToUtc(dateTime, _airPortTimeZones[airportCode]);
        }

        public DateTime ConvertToLocal(string airportCode, DateTime dateTime)
        {
            if (!_airPortTimeZones.ContainsKey(airportCode))
                throw new Exception($"Windows Time Zone Not Found For Airport : {airportCode}");

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _airPortTimeZones[airportCode]);
        }

        private void InitializeTimeZoneInformation(string codeFilePath)
        {
            StreamReader reader = new StreamReader(codeFilePath);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().Trim().Split('\t').Select(s => s.Trim()).ToArray();
                string code = line[0];
                string zone = line[line.Length - 1];
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TZConvert.IanaToWindows(zone));

                if (timeZoneInfo == null)
                    throw new Exception($"Windows Time Zone Not Found For Airport : {code} and Zone : {zone}");
                _airPortTimeZones.Add(code, timeZoneInfo);
            }
        }
    }
}
