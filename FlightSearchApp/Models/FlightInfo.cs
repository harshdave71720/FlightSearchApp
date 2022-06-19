using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace FlightSearchApp.Models
{
    [Owned]
    public class FlightInfo
    {
        [Required]
        public string Origin { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public string Destination { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public double Price { get; set; }

        public string Provider { get; set; }
    }
}
