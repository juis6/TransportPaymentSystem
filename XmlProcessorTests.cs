using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace TransportPaymentSystem.Tests
{
    public class XmlProcessorTests : IDisposable
    {
        private readonly string _testFolder = "TestData";

        public XmlProcessorTests()
        {
            Directory.CreateDirectory(_testFolder);
            CreateTestFiles();
        }

        private void CreateTestFiles()
        {
            // ��������
            new XDocument(
                new XElement("Passengers",
                    new XElement("Passenger", new XAttribute("Id", 1),
                        new XElement("Surname", "��������"),
                        new XElement("CategoryId", 1)),
                    new XElement("Passenger", new XAttribute("Id", 2),
                        new XElement("Surname", "��������"),
                        new XElement("CategoryId", 2))
                )).Save(Path.Combine(_testFolder, "passengers.xml"));

            // �������
            new XDocument(
                new XElement("Categories",
                    new XElement("Category", new XAttribute("Id", 1),
                        new XElement("n", "���������"),
                        new XElement("TripCost", 8)),
                    new XElement("Category", new XAttribute("Id", 2),
                        new XElement("n", "�������"),
                        new XElement("TripCost", 4))
                )).Save(Path.Combine(_testFolder, "categories.xml"));

            // ������ 1
            new XDocument(
                new XElement("PaymentRecords",
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-10"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 5)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-15"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 5))
                )).Save(Path.Combine(_testFolder, "payments1.xml"));

            // ������ 2
            new XDocument(
                new XElement("PaymentRecords",
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-01-20"),
                        new XElement("PassengerId", 2),
                        new XElement("RouteNumber", 10)),
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-02-10"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 10))
                )).Save(Path.Combine(_testFolder, "payments2.xml"));
        }

        [Fact]
        public void TestPassengerTripCounts()
        {
            // Act
            var result = XmlProcessor.GetPassengerTripCounts(
                Path.Combine(_testFolder, "passengers.xml"),
                Path.Combine(_testFolder, "payments1.xml"),
                Path.Combine(_testFolder, "payments2.xml"));

            // Assert
            Assert.NotNull(result);

            // ���������� �� � ��� ��� ���� ��������
            var passengers = result.Elements("Passenger").ToList();
            Assert.Equal(2, passengers.Count);

            // ���������� ���������� �� �������� (�������� ������)
            var firstPassenger = passengers.First();
            Assert.Equal("��������", firstPassenger.Attribute("Surname")?.Value);

            // ���������� �� � �������� 2 ������ ��������� 5
            var petrenko = passengers.Last();
            Assert.Equal("��������", petrenko.Attribute("Surname")?.Value);
            var route5 = petrenko.Elements("Route").FirstOrDefault(r => r.Attribute("Number")?.Value == "5");
            Assert.NotNull(route5);
            Assert.Equal("2", route5.Attribute("TripCount")?.Value);
        }

        [Fact]
        public void TestMonthlyTopRoutes()
        {
            // Act
            var result = XmlProcessor.GetMonthlyTopRoutes(
                Path.Combine(_testFolder, "passengers.xml"),
                Path.Combine(_testFolder, "categories.xml"),
                Path.Combine(_testFolder, "payments1.xml"),
                Path.Combine(_testFolder, "payments2.xml"));

            // Assert
            Assert.NotNull(result);

            // ���������� �� � ��� ��� ���� ������
            var months = result.Elements("Month").ToList();
            Assert.Equal(2, months.Count);

            // ���������� �����
            var january = months.First();
            Assert.Equal("2024", january.Attribute("Year")?.Value);
            Assert.Equal("1", january.Attribute("Month")?.Value);

            // ���������� �� � ��� �������
            var topRoute = january.Element("TopRoute");
            Assert.NotNull(topRoute);
            Assert.NotNull(topRoute.Attribute("Number"));
            Assert.NotNull(topRoute.Attribute("TotalAmount"));
        }

        public void Dispose()
        {
            if (Directory.Exists(_testFolder))
                Directory.Delete(_testFolder, true);
        }
    }
}