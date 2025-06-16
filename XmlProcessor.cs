using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TransportPaymentSystem
{
    public static class XmlProcessor
    {
        // Завдання А: Кількість поїздок кожного пасажира по маршрутах
        public static XElement GetPassengerTripCounts(
            string passengersFile,
            string paymentsFile1,
            string paymentsFile2)
        {
            // Читаємо пасажирів
            var passengersXml = XDocument.Load(passengersFile);
            var passengers = passengersXml.Root?.Elements("Passenger")
                .Select(p => new {
                    Id = (int?)p.Attribute("Id") ?? 0,
                    Surname = p.Element("Surname")?.Value ?? string.Empty
                })
                .ToList() ?? new List<dynamic>();

            // Читаємо платежі з двох файлів
            var payments1 = XDocument.Load(paymentsFile1).Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>();
            var payments2 = XDocument.Load(paymentsFile2).Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>();
            var allPayments = payments1.Concat(payments2)
                .Select(p => new {
                    PassengerId = (int?)p.Element("PassengerId") ?? 0,
                    RouteNumber = (int?)p.Element("RouteNumber") ?? 0
                })
                .ToList();

            // Групуємо та рахуємо
            var result = new XElement("PassengerTripCounts");

            var passengerTrips = passengers
                .OrderBy(p => p.Surname) // Сортуємо пасажирів за прізвищем
                .Select(passenger => new {
                    passenger,
                    trips = allPayments
                        .Where(pay => pay.PassengerId == passenger.Id)
                        .GroupBy(pay => pay.RouteNumber)
                        .OrderBy(g => g.Key) // Сортуємо маршрути за номером
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

        // Завдання Б: Маршрут з найбільшою сумою оплати по місяцях
        public static XElement GetMonthlyTopRoutes(
            string passengersFile,
            string categoriesFile,
            string paymentsFile1,
            string paymentsFile2)
        {
            // Читаємо дані
            var passengersXml = XDocument.Load(passengersFile);
            var categoriesXml = XDocument.Load(categoriesFile);
            var payments1Xml = XDocument.Load(paymentsFile1);
            var payments2Xml = XDocument.Load(paymentsFile2);

            // Пасажири з категоріями
            var passengers = passengersXml.Root?.Elements("Passenger")
                .Select(p => new {
                    Id = (int?)p.Attribute("Id") ?? 0,
                    CategoryId = (int?)p.Element("CategoryId") ?? 0
                })
                .ToList() ?? new List<dynamic>();

            // Категорії з цінами
            var categories = categoriesXml.Root?.Elements("Category")
                .Select(c => new {
                    Id = (int?)c.Attribute("Id") ?? 0,
                    Cost = (decimal?)c.Element("TripCost") ?? 0m
                })
                .ToList() ?? new List<dynamic>();

            // Всі платежі
            var allPayments = (payments1Xml.Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>())
                .Concat(payments2Xml.Root?.Elements("PaymentRecord") ?? Enumerable.Empty<XElement>())
                .Select(p => new {
                    Date = DateTime.Parse(p.Element("Date")?.Value ?? DateTime.Now.ToString()),
                    PassengerId = (int?)p.Element("PassengerId") ?? 0,
                    RouteNumber = (int?)p.Element("RouteNumber") ?? 0
                })
                .ToList();

            // Рахуємо суму по маршрутах для кожного місяця
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

            // Створюємо XML
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