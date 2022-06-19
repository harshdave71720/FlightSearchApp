using FlightSearchApp.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace FlightSearchApp.Dtos
{
    public class FlightSearchDto
    {
        [Required]
        public string Origin { get; set; }

        [Required]
        public string Destination { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public string SortBy { get; set; }

        public FlightSearch ToModel()
        {
            if (Enum.TryParse(SortBy, true, out SortByOptions option))
            {
                return new FlightSearch
                {
                    Origin = Origin,
                    Destination = Destination,
                    DepartureDate = DepartureDate.Date,
                    SortBy = option
                };
            }
            else 
            {
                throw new Exception("Cannot Parse Sort By Option Provided");
            }
        }
    }
}
