using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using FlightSearchApp.Repositories;
using FlightSearchApp.Services;
using FlightSearchApp.Models;
using System.Linq;
using FlightSearchApp.Configurations;

namespace FlightSearchApp.Tests.Services
{
    [TestFixture]
    public class FlightServiceTests
    {
        private FlightSearchConfiguration _flightSearchConfiguration => new FlightSearchConfiguration() { MinHoursBetweenFlights = 1, MaxHoursBetweenFlights = 12 };
        
        [Test]
        public void GetFlights_ReturnDirectFlightsIfPresent()
        {
            // Arrange
            IFlightRepository flightRepository = new InMemoryFlightRepository();
            List<Flight> flights = new List<Flight>()
            {
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "C",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(2, 0, 0)),
                        Price = 200,
                        Provider = "Provider3"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(1, 0, 0)),
                        Price = 100,
                        Provider = "Provider1"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                         Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(3, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                }
            };
            flightRepository.AddFlights(flights);

            FlightService sut = new FlightService(GetAirportTimeConverter(), flightRepository, _flightSearchConfiguration);

            // Act
            var result = sut.GetFlightPlans(new FlightSearch
            {
                Origin = "A",
                Destination = "B",
                DepartureDate = DateTime.Now.Date,
                SortBy = SortByOptions.Price
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            AssertFlightPlan(result[0], flights[1].Info);
            AssertFlightPlan(result[1], flights[2].Info);
        }

        [Test]
        public void GetFlights_ReturnInDirectFlightsIfPresent()
        {
            // Arrange
            IFlightRepository flightRepository = new InMemoryFlightRepository();
            List<Flight> flights = new List<Flight>()
            {
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "D",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(2, 0, 0)),
                        Price = 200,
                        Provider = "Provider3"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(1, 0, 0)),
                        Price = 100,
                        Provider = "Provider1"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "B",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(3, 0, 0)),
                        Destination = "C",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(5, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                }
            };
            flightRepository.AddFlights(flights);

            FlightService sut = new FlightService(GetAirportTimeConverter(), flightRepository, _flightSearchConfiguration);

            // Act
            var result = sut.GetFlightPlans(new FlightSearch
            {
                Origin = "A",
                Destination = "C",
                DepartureDate = DateTime.Now.Date,
                SortBy = SortByOptions.Price
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            AssertFlightPlan(result[0], flights[1].Info, flights[2].Info);
        }

        [Test]
        public void GetFlights_IgnoresFlightsOnOtherDatesThanDeparture()
        {
            // Arrange
            IFlightRepository flightRepository = new InMemoryFlightRepository();
            List<Flight> flights = new List<Flight>()
            {
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "D",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(2, 0, 0)),
                        Price = 200,
                        Provider = "Provider3"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now.AddDays(1),
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(1, 0, 0)),
                        Price = 100,
                        Provider = "Provider1"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "B",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(3, 0, 0)),
                        Destination = "C",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(5, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                }
            };
            flightRepository.AddFlights(flights);

            FlightService sut = new FlightService(GetAirportTimeConverter(), flightRepository, _flightSearchConfiguration);

            // Act
            var result = sut.GetFlightPlans(new FlightSearch
            {
                Origin = "A",
                Destination = "C",
                DepartureDate = DateTime.Now.Date,
                SortBy = SortByOptions.Price
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetFlights_IgnoresConnectingFlightsOnTimeDifferenceMaxedOut()
        {
            // Arrange
            IFlightRepository flightRepository = new InMemoryFlightRepository();
            List<Flight> flights = new List<Flight>()
            {
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "D",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(2, 0, 0)),
                        Price = 200,
                        Provider = "Provider3"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(1, 0, 0)),
                        Price = 100,
                        Provider = "Provider1"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "B",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(15, 0, 0)),
                        Destination = "C",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(5, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                }
            };
            flightRepository.AddFlights(flights);

            FlightService sut = new FlightService(GetAirportTimeConverter(), flightRepository, _flightSearchConfiguration);

            // Act
            var result = sut.GetFlightPlans(new FlightSearch
            {
                Origin = "A",
                Destination = "C",
                DepartureDate = DateTime.Now.Date,
                SortBy = SortByOptions.Price
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetFlights_IgnoresConnectingFlightsOnTimeDifferenceLessThanMin()
        {
            // Arrange
            IFlightRepository flightRepository = new InMemoryFlightRepository();
            List<Flight> flights = new List<Flight>()
            {
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "D",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(2, 0, 0)),
                        Price = 200,
                        Provider = "Provider3"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(1, 0, 0)),
                        Price = 100,
                        Provider = "Provider1"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "B",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(1, 30, 0)),
                        Destination = "C",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(5, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                }
            };
            flightRepository.AddFlights(flights);

            FlightService sut = new FlightService(GetAirportTimeConverter(), flightRepository, _flightSearchConfiguration);

            // Act
            var result = sut.GetFlightPlans(new FlightSearch
            {
                Origin = "A",
                Destination = "C",
                DepartureDate = DateTime.Now.Date,
                SortBy = SortByOptions.Price
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetFlights_IgnoresCircularFlightPlans()
        {
            // Arrange
            IFlightRepository flightRepository = new InMemoryFlightRepository();
            List<Flight> flights = new List<Flight>()
            {
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(1, 0, 0)),
                        Price = 100,
                        Provider = "Provider1"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "B",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(3, 0, 0)),
                        Destination = "C",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(5, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "C",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(7, 0, 0)),
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(8, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                }
            };
            flightRepository.AddFlights(flights);

            FlightService sut = new FlightService(GetAirportTimeConverter(), flightRepository, _flightSearchConfiguration);

            // Act
            var result = sut.GetFlightPlans(new FlightSearch
            {
                Origin = "A",
                Destination = "B",
                DepartureDate = DateTime.Now.Date,
                SortBy = SortByOptions.Price
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            AssertFlightPlan(result[0], flights[0].Info);
        }

        [Test]
        public void GetFlights_ReturnsBothDirectAndConnectingFlights()
        {
            // Arrange
            IFlightRepository flightRepository = new InMemoryFlightRepository();
            List<Flight> flights = new List<Flight>()
            {
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now,
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(1, 0, 0)),
                        Price = 700,
                        Provider = "Provider1"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(3, 0, 0)),
                        Destination = "C",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(5, 0, 0)),
                        Price = 300,
                        Provider = "Provider2"
                    }
                },
                new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "C",
                        DepartureTime = DateTime.Now.Add(new TimeSpan(7, 0, 0)),
                        Destination = "B",
                        ArrivalTime = DateTime.Now.Add(new TimeSpan(8, 0, 0)),
                        Price = 200,
                        Provider = "Provider2"
                    }
                },
                 new Flight
                {
                    Info = new FlightInfo
                    {
                        Origin = "A",
                        DepartureTime = DateTime.Now.AddDays(1),
                        Destination = "B",
                        ArrivalTime = DateTime.Now.AddDays(1).Add(new TimeSpan(1, 0, 0)),
                        Price = 700,
                        Provider = "Provider1"
                    }
                }
            };
            flightRepository.AddFlights(flights);

            FlightService sut = new FlightService(GetAirportTimeConverter(), flightRepository, _flightSearchConfiguration);

            // Act
            var result = sut.GetFlightPlans(new FlightSearch
            {
                Origin = "A",
                Destination = "B",
                DepartureDate = DateTime.Now.Date,
                SortBy = SortByOptions.Price
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            AssertFlightPlan(result[0], flights[1].Info, flights[2].Info);
            AssertFlightPlan(result[1], flights[0].Info);
        }

        private void AssertFlightPlan(FlightPlan plan, params FlightInfo[] expectedFlights)
        {
            Assert.AreEqual(plan.Length, expectedFlights.Length);
            Assert.That(plan.DepartureTime, Is.EqualTo(expectedFlights[0].DepartureTime).Within(TimeSpan.FromSeconds(0.1)));
            double cost = 0;
            var actualFlights = plan.Flights.ToArray();
            for (int i = 0; i < expectedFlights.Length; i++)
            {
                cost += expectedFlights[i].Price;
                Assert.AreEqual(expectedFlights[i], actualFlights[i]);
            }

            Assert.AreEqual(cost, plan.Cost);
        }

        private IAirportTimeConverter GetAirportTimeConverter()
        {
            var airportTimeConvertor = new Mock<IAirportTimeConverter>();
            airportTimeConvertor.Setup(x => x.ConvertToUtc(It.IsAny<string>(), It.IsAny<DateTime>())).Returns((string s, DateTime x) => x);
            airportTimeConvertor.Setup(x => x.ConvertToLocal(It.IsAny<string>(), It.IsAny<DateTime>())).Returns((string s, DateTime x) => x);
            return airportTimeConvertor.Object;
        }
    }
}
