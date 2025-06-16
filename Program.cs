using System;
using System.IO;
using System.Xml.Linq;

namespace TransportPaymentSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Система обліку оплати проїзду ===\n");

            // Створюємо папки
            Directory.CreateDirectory("Data");
            Directory.CreateDirectory("Output");

            // Створюємо тестові файли якщо їх немає
            if (!File.Exists("Data/passengers.xml"))
            {
                CreateSampleFiles();
                Console.WriteLine("Створено тестові файли в папці Data\n");
            }

            try
            {
                // Завдання А
                Console.WriteLine("Завдання А: Підрахунок поїздок пасажирів");
                var tripCounts = XmlProcessor.GetPassengerTripCounts(
                    "Data/passengers.xml",
                    "Data/payments1.xml",
                    "Data/payments2.xml");

                tripCounts.Save("Output/task_a.xml");
                Console.WriteLine("Результат збережено в Output/task_a.xml");
                Console.WriteLine(tripCounts);

                // Завдання Б
                Console.WriteLine("\nЗавдання Б: Топ маршрути по місяцях");
                var monthlyTop = XmlProcessor.GetMonthlyTopRoutes(
                    "Data/passengers.xml",
                    "Data/categories.xml",
                    "Data/payments1.xml",
                    "Data/payments2.xml");

                monthlyTop.Save("Output/task_b.xml");
                Console.WriteLine("Результат збережено в Output/task_b.xml");
                Console.WriteLine(monthlyTop);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }

        static void CreateSampleFiles()
        {
            // Маршрути
            new XDocument(
                new XElement("Routes",
                    new XElement("Route", new XAttribute("Number", 1),
                        new XElement("CurrentStop", "Центр"),
                        new XElement("FinalStop", "Вокзал")),
                    new XElement("Route", new XAttribute("Number", 5),
                        new XElement("CurrentStop", "Університет"),
                        new XElement("FinalStop", "Аеропорт")),
                    new XElement("Route", new XAttribute("Number", 10),
                        new XElement("CurrentStop", "Лікарня"),
                        new XElement("FinalStop", "Парк"))
                )).Save("Data/routes.xml");

            // Пасажири
            new XDocument(
                new XElement("Passengers",
                    new XElement("Passenger", new XAttribute("Id", 1),
                        new XElement("Surname", "Петренко"),
                        new XElement("CategoryId", 1)),
                    new XElement("Passenger", new XAttribute("Id", 2),
                        new XElement("Surname", "Іваненко"),
                        new XElement("CategoryId", 2)),
                    new XElement("Passenger", new XAttribute("Id", 3),
                        new XElement("Surname", "Коваленко"),
                        new XElement("CategoryId", 3))
                )).Save("Data/passengers.xml");

            // Категорії
            new XDocument(
                new XElement("Categories",
                    new XElement("Category", new XAttribute("Id", 1),
                        new XElement("n", "Звичайний"),
                        new XElement("TripCost", 8)),
                    new XElement("Category", new XAttribute("Id", 2),
                        new XElement("n", "Студент"),
                        new XElement("TripCost", 4)),
                    new XElement("Category", new XAttribute("Id", 3),
                        new XElement("n", "Пенсіонер"),
                        new XElement("TripCost", 2))
                )).Save("Data/categories.xml");

            // Платежі файл 1
            new XDocument(
                new XElement("PaymentRecords",
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-10"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 1)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-15"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 1)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-20"),
                        new XElement("PassengerId", 2),
                        new XElement("RouteNumber", 5)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-02-05"),
                        new XElement("PassengerId", 3),
                        new XElement("RouteNumber", 10))
                )).Save("Data/payments1.xml");

            // Платежі файл 2
            new XDocument(
                new XElement("PaymentRecords",
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-02-10"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 5)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-02-15"),
                        new XElement("PassengerId", 2),
                        new XElement("RouteNumber", 5)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-03-05"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 10)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-03-10"),
                        new XElement("PassengerId", 3),
                        new XElement("RouteNumber", 1))
                )).Save("Data/payments2.xml");
        }
    }
}