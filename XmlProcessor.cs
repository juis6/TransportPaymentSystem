using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TransportPaymentSystem
{
    public static class XmlProcessor
    {
        // �������� �: ʳ������ ������ ������� �������� �� ���������
        public static XElement GetPassengerTripCounts(
            string passengersFile,
            string paymentsFile1,
            string paymentsFile2)
        {
            // ������ ��������
            var passengersXml = XDocument.Load(passengersFile);
            var passengers = passengersXml.Root?.Elements("Passenger")
                .Select(p => new {
                    Id = (int?)p.Attribute("Id") ?? 0,
                    Surname = p.Element("Surname")?.Value ?? string.Empty
                })
                .ToList() ?? new List<dynamic>();

            // ������ ������ � ���� �����
            var payments1 = XDocument.Load(paymentsFile1).Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>();
            var payments2 = XDocument.Load(paymentsFile2).Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>();
            var allPayments = payments1.Concat(payments2)
                .Select(p => new {
                    PassengerId = (int?)p.Element("PassengerId") ?? 0,
                    RouteNumber = (int?)p.Element("RouteNumber") ?? 0
                })
                .ToList();

            // ������� �� ������
            var result = new XElement("PassengerTripCounts");

            var passengerTrips = passengers
                .OrderBy(p => p.Surname) // ������� �������� �� ��������
                .Select(passenger => new {
                    passenger,
                    trips = allPayments
                        .Where(pay => pay.PassengerId == passenger.Id)
                        .GroupBy(pay => pay.RouteNumber)
                        .OrderBy(g => g.Key) // ������� �������� �� �������
                        .Select(g => new { Route = g.Key, Count = g.Count() })
                })
                .Where(pt => pt.trips.Any());

            foreach (var pt in passengerTrips)
            {
                var passengerEl = new XElement("Passenger",
                    new XAttribute("Id", pt.passenger.Id),
                    new XAttribute("Surname", pt.passenger.Surname));

                foreach (var trip in pt.trips)
                {
                    passengerEl.Add(new XElement("Route",
                        new XAttribute("Number", trip.Route),
                        new XAttribute("TripCount", trip.Count)));
                }

                result.Add(passengerEl);
            }

            return result;
        }

        // �������� �: ������� � ��������� ����� ������ �� ������
        public static XElement GetMonthlyTopRoutes(
            string passengersFile,
            string categoriesFile,
            string paymentsFile1,
            string paymentsFile2)
        {
            // ������ ���
            var passengersXml = XDocument.Load(passengersFile);
            var categoriesXml = XDocument.Load(categoriesFile);
            var payments1Xml = XDocument.Load(paymentsFile1);
            var payments2Xml = XDocument.Load(paymentsFile2);

            // �������� � ����������
            var passengers = passengersXml.Root?.Elements("Passenger")
                .Select(p => new {
                    Id = (int?)p.Attribute("Id") ?? 0,
                    CategoryId = (int?)p.Element("CategoryId") ?? 0
                })
                .ToList() ?? new List<dynamic>();

            // ������� � ������
            var categories = categoriesXml.Root?.Elements("Category")
                .Select(c => new {
                    Id = (int?)c.Attribute("Id") ?? 0,
                    Cost = (decimal?)c.Element("TripCost") ?? 0m
                })
                .ToList() ?? new List<dynamic>();

            // �� ������
            var allPayments = (payments1Xml.Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>())
                .Concat(payments2Xml.Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>())
                .Select(p => new {
                    Date = DateTime.Parse(p.Element("Date")?.Value ?? DateTime.Now.ToString()),
                    PassengerId = (int?)p.Element("PassengerId") ?? 0,
                    RouteNumber = (int?)p.Element("RouteNumber") ?? 0
                })
                .ToList();

            // ������ ���� �� ��������� ��� ������� �����
            var monthlyData = allPayments
                .Select(payment => {
                    var passenger = passengers.FirstOrDefault(p => p.Id == payment.PassengerId);
                    if (passenger == null) return null;

                    var category = categories.FirstOrDefault(c => c.Id == passenger.CategoryId);
                    if (category == null) return null;

                    return new
                    {
                        Year = payment.Date.Year,
                        Month = payment.Date.Month,
                        Route = payment.RouteNumber,
                        Cost = category.Cost
                    };
                })
                .Where(x => x != null)
                .GroupBy(x => new { x!.Year, x.Month, x.Route })
                .Select(g => new {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Route,
                    Total = g.Sum(x => x!.Cost)
                })
                .GroupBy(x => new { x.Year, x.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new {
                    g.Key.Year,
                    g.Key.Month,
                    TopRoute = g.OrderByDescending(x => x.Total).First()
                });

            // ��������� XML
            var result = new XElement("MonthlyTopRoutes");

            foreach (var month in monthlyData)
            {
                result.Add(new XElement("Month",
                    new XAttribute("Year", month.Year),
                    new XAttribute("Month", month.Month),
                    new XElement("TopRoute",
                        new XAttribute("Number", month.TopRoute.Route),
                        new XAttribute("TotalAmount", month.TopRoute.Total))));
            }

            return result;
        }
    }
}