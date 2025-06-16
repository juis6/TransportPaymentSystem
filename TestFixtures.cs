using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace TransportPaymentSystem.Tests
{
    /// <summary>
    /// Class Fixture для створення тестових даних один раз для всіх тестів
    /// </summary>
    public class TestDataFixture : IDisposable
    {
        public string TestDirectory { get; }
        public string PassengersFile { get; }
        public string CategoriesFile { get; }
        public string RoutesFile { get; }
        public string PaymentsFile1 { get; }
        public string PaymentsFile2 { get; }

        public TestDataFixture()
        {
            // Створюємо тимчасову директорію для тестових даних
            TestDirectory = Path.Combine(Path.GetTempPath(), $"TransportTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(TestDirectory);

            // Шляхи до файлів
            PassengersFile = Path.Combine(TestDirectory, "passengers.xml");
            CategoriesFile = Path.Combine(TestDirectory, "categories.xml");
            RoutesFile = Path.Combine(TestDirectory, "routes.xml");
            PaymentsFile1 = Path.Combine(TestDirectory, "payments1.xml");
            PaymentsFile2 = Path.Combine(TestDirectory, "payments2.xml");

            // Створюємо тестові дані
            CreateTestData();
        }

        private void CreateTestData()
        {
            // Маршрути
            new XDocument(
                new XElement("Routes",
                    new XElement("Route", new XAttribute("Number", 1),
                        new XElement("CurrentStop", "Центральна"),
                        new XElement("FinalStop", "Вокзал")),
                    new XElement("Route", new XAttribute("Number", 2),
                        new XElement("CurrentStop", "Парк"),
                        new XElement("FinalStop", "Університет")),
                    new XElement("Route", new XAttribute("Number", 3),
                        new XElement("CurrentStop", "Ринок"),
                        new XElement("FinalStop", "Аеропорт"))
                )).Save(RoutesFile);

            // Пасажири
            new XDocument(
                new XElement("Passengers",
                    new XElement("Passenger", new XAttribute("Id", 1),
                        new XElement("Surname", "Антонов"),
                        new XElement("CategoryId", 1)),
                    new XElement("Passenger", new XAttribute("Id", 2),
                        new XElement("Surname", "Борисенко"),
                        new XElement("CategoryId", 2)),
                    new XElement("Passenger", new XAttribute("Id", 3),
                        new XElement("Surname", "Василенко"),
                        new XElement("CategoryId", 1)),
                    new XElement("Passenger", new XAttribute("Id", 4),
                        new XElement("Surname", "Григоренко"),
                        new XElement("CategoryId", 3))
                )).Save(PassengersFile);

            // Категорії
            new XDocument(
                new XElement("Categories",
                    new XElement("Category", new XAttribute("Id", 1),
                        new XElement("Name", "Звичайний"),
                        new XElement("TripCost", 10)),
                    new XElement("Category", new XAttribute("Id", 2),
                        new XElement("Name", "Студент"),
                        new XElement("TripCost", 5)),
                    new XElement("Category", new XAttribute("Id", 3),
                        new XElement("Name", "Пенсіонер"),
                        new XElement("TripCost", 3))
                )).Save(CategoriesFile);

            // Платежі файл 1
            new XDocument(
                new XElement("PaymentRecords",
                    // Січень
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-05"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 1)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-10"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 1)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-15"),
                        new XElement("PassengerId", 2),
                        new XElement("RouteNumber", 2)),
                    // Лютий
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-02-05"),
                        new XElement("PassengerId", 3),
                        new XElement("RouteNumber", 3)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-02-10"),
                        new XElement("PassengerId", 4),
                        new XElement("RouteNumber", 3))
                )).Save(PaymentsFile1);

            // Платежі файл 2
            new XDocument(
                new XElement("PaymentRecords",
                    // Січень
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-20"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 2)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-25"),
                        new XElement("PassengerId", 3),
                        new XElement("RouteNumber", 1)),
                    // Лютий
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-02-15"),
                        new XElement("PassengerId", 2),
                        new XElement("RouteNumber", 3)),
                    // Березень
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-03-05"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 3)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-03-10"),
                        new XElement("PassengerId", 4),
                        new XElement("RouteNumber", 2))
                )).Save(PaymentsFile2);
        }

        public void Dispose()
        {
            // Очищуємо тестові дані
            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory, true);
            }
        }
    }

    /// <summary>
    /// Test Fixture для окремих тестових сценаріїв
    /// </summary>
    public class SinglePaymentTestFixture : IDisposable
    {
        public string TestDirectory { get; }
        public XDocument SinglePaymentDoc { get; }
        public string SinglePaymentFile { get; }

        public SinglePaymentTestFixture()
        {
            TestDirectory = Path.Combine(Path.GetTempPath(), $"SingleTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(TestDirectory);

            SinglePaymentFile = Path.Combine(TestDirectory, "single_payment.xml");

            // Створюємо один платіж для тестування
            SinglePaymentDoc = new XDocument(
                new XElement("PaymentRecords",
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-04-01"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 5))
                ));
            SinglePaymentDoc.Save(SinglePaymentFile);
        }

        public void Dispose()
        {
            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory, true);
            }
        }
    }
}