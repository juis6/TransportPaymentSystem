using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace TransportPaymentSystem
{
    public static class XmlProcessor
    {
        /// <summary>
        /// Завдання А: Підрахунок кількості поїздок кожного пасажира за маршрутами
        /// </summary>
        public static XElement GetPassengerTripCounts(string passengersFile, params string[] paymentFiles)
        {
            // Завантажуємо пасажирів
            var passengersDoc = XDocument.Load(passengersFile);
            var passengers = passengersDoc.Root.Elements("Passenger")
                .Select(p => new
                {
                    Id = int.Parse(p.Attribute("Id").Value),
                    Surname = p.Element("Surname").Value
                })
                .ToList();

            // Завантажуємо всі платежі з усіх файлів
            var allPayments = new List<PaymentRecord>();
            foreach (var paymentFile in paymentFiles)
            {
                var paymentDoc = XDocument.Load(paymentFile);
                var payments = paymentDoc.Root.Elements("PaymentRecord")
                    .Select(pr => new PaymentRecord
                    {
                        Date = DateTime.Parse(pr.Element("Date").Value),
                        PassengerId = int.Parse(pr.Element("PassengerId").Value),
                        RouteNumber = int.Parse(pr.Element("RouteNumber").Value)
                    });
                allPayments.AddRange(payments);
            }

            // Групуємо платежі за пасажиром та маршрутом
            var tripCounts = from p in passengers
                             join payment in allPayments on p.Id equals payment.PassengerId into passengerPayments
                             from routeGroup in (from pp in passengerPayments
                                                 group pp by pp.RouteNumber into g
                                                 orderby g.Key
                                                 select new { RouteNumber = g.Key, Count = g.Count() })
                             orderby p.Surname, routeGroup.RouteNumber
                             select new { p.Surname, routeGroup.RouteNumber, routeGroup.Count };

            // Формуємо результат як XElement
            var result = new XElement("PassengerTripCounts",
                from tc in tripCounts
                group tc by tc.Surname into passengerGroup
                orderby passengerGroup.Key
                select new XElement("Passenger",
                    new XAttribute("Surname", passengerGroup.Key),
                    from route in passengerGroup
                    select new XElement("Route",
                        new XAttribute("Number", route.RouteNumber),
                        new XAttribute("TripCount", route.Count)
                    )
                )
            );

            return result;
        }

        /// <summary>
        /// Завдання Б: Визначення маршруту з найбільшою сумою оплати за кожен місяць
        /// </summary>
        public static XElement GetMonthlyTopRoutes(string passengersFile, string categoriesFile, params string[] paymentFiles)
        {
            // Завантажуємо пасажирів
            var passengersDoc = XDocument.Load(passengersFile);
            var passengers = passengersDoc.Root.Elements("Passenger")
                .Select(p => new
                {
                    Id = int.Parse(p.Attribute("Id").Value),
                    CategoryId = int.Parse(p.Element("CategoryId").Value)
                })
                .ToList();

            // Завантажуємо категорії
            var categoriesDoc = XDocument.Load(categoriesFile);
            var categories = categoriesDoc.Root.Elements("Category")
                .Select(c => new
                {
                    Id = int.Parse(c.Attribute("Id").Value),
                    Cost = decimal.Parse(c.Element("TripCost").Value)
                })
                .ToList();

            // Завантажуємо всі платежі
            var allPayments = new List<PaymentRecord>();
            foreach (var paymentFile in paymentFiles)
            {
                var paymentDoc = XDocument.Load(paymentFile);
                var payments = paymentDoc.Root.Elements("PaymentRecord")
                    .Select(pr => new PaymentRecord
                    {
                        Date = DateTime.Parse(pr.Element("Date").Value),
                        PassengerId = int.Parse(pr.Element("PassengerId").Value),
                        RouteNumber = int.Parse(pr.Element("RouteNumber").Value)
                    });
                allPayments.AddRange(payments);
            }

            // Обчислюємо суми за маршрутами по місяцях
            var monthlyRouteSums = from payment in allPayments
                                   join passenger in passengers on payment.PassengerId equals passenger.Id
                                   join category in categories on passenger.CategoryId equals category.Id
                                   group new { payment, cost = category.Cost } by new
                                   {
                                       Year = payment.Date.Year,
                                       Month = payment.Date.Month,
                                       RouteNumber = payment.RouteNumber
                                   } into g
                                   select new
                                   {
                                       g.Key.Year,
                                       g.Key.Month,
                                       g.Key.RouteNumber,
                                       TotalSum = g.Sum(x => x.cost)
                                   };

            // Знаходимо топ маршрути по місяцях
            var topRoutes = from mrs in monthlyRouteSums
                            group mrs by new { mrs.Year, mrs.Month } into monthGroup
                            let topRoute = monthGroup.OrderByDescending(x => x.TotalSum).First()
                            orderby monthGroup.Key.Year, monthGroup.Key.Month
                            select new
                            {
                                monthGroup.Key.Year,
                                monthGroup.Key.Month,
                                topRoute.RouteNumber,
                                topRoute.TotalSum
                            };

            // Формуємо результат
            var result = new XElement("MonthlyTopRoutes",
                from tr in topRoutes
                select new XElement("Month",
                    new XAttribute("Year", tr.Year),
                    new XAttribute("Month", tr.Month),
                    new XElement("TopRoute",
                        new XAttribute("Number", tr.RouteNumber),
                        new XAttribute("TotalSum", tr.TotalSum.ToString("F2", CultureInfo.InvariantCulture))
                    )
                )
            );

            return result;
        }
    }
}