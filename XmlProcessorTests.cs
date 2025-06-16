using System;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace TransportPaymentSystem.Tests
{
    /// <summary>
    /// ����� � ������������� Class Fixture
    /// </summary>
    public class XmlProcessorTests : IClassFixture<TestDataFixture>
    {
        private readonly TestDataFixture _fixture;

        public XmlProcessorTests(TestDataFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void GetPassengerTripCounts_ReturnsCorrectStructure()
        {
            // Act
            var result = XmlProcessor.GetPassengerTripCounts(
                _fixture.PassengersFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PassengerTripCounts", result.Name.LocalName);
            Assert.True(result.HasElements);
        }

        [Fact]
        public void GetPassengerTripCounts_CorrectlyCountsTrips()
        {
            // Act
            var result = XmlProcessor.GetPassengerTripCounts(
                _fixture.PassengersFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert
            // ���������� ������� ������ ��� �������� (Id=1)
            var antonov = result.Elements("Passenger")
                .FirstOrDefault(p => p.Attribute("Surname")?.Value == "�������");

            Assert.NotNull(antonov);

            // �������: ������� 1 - 2 ����, ������� 2 - 1 ���, ������� 3 - 1 ���
            var route1 = antonov.Elements("Route")
                .FirstOrDefault(r => r.Attribute("Number")?.Value == "1");
            Assert.NotNull(route1);
            Assert.Equal("2", route1.Attribute("TripCount")?.Value);
        }

        [Fact]
        public void GetPassengerTripCounts_SortsPassengersBySurname()
        {
            // Act
            var result = XmlProcessor.GetPassengerTripCounts(
                _fixture.PassengersFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert
            var surnames = result.Elements("Passenger")
                .Select(p => p.Attribute("Surname")?.Value)
                .ToList();

            Assert.Equal(4, surnames.Count);
            Assert.Equal("�������", surnames[0]);
            Assert.Equal("���������", surnames[1]);
            Assert.Equal("���������", surnames[2]);
            Assert.Equal("����������", surnames[3]);
        }

        [Fact]
        public void GetPassengerTripCounts_SortsRoutesByNumber()
        {
            // Act
            var result = XmlProcessor.GetPassengerTripCounts(
                _fixture.PassengersFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert
            var antonov = result.Elements("Passenger")
                .FirstOrDefault(p => p.Attribute("Surname")?.Value == "�������");

            var routeNumbers = antonov.Elements("Route")
                .Select(r => int.Parse(r.Attribute("Number")?.Value ?? "0"))
                .ToList();

            // ����������, �� �������� ���������� �� ����������
            Assert.Equal(routeNumbers.OrderBy(n => n).ToList(), routeNumbers);
        }

        [Fact]
        public void GetMonthlyTopRoutes_ReturnsCorrectStructure()
        {
            // Act
            var result = XmlProcessor.GetMonthlyTopRoutes(
                _fixture.PassengersFile,
                _fixture.CategoriesFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("MonthlyTopRoutes", result.Name.LocalName);
            Assert.True(result.HasElements);
        }

        [Fact]
        public void GetMonthlyTopRoutes_CalculatesCorrectTotals()
        {
            // Act
            var result = XmlProcessor.GetMonthlyTopRoutes(
                _fixture.PassengersFile,
                _fixture.CategoriesFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert
            // ���������� ����� 2024
            var january = result.Elements("Month")
                .FirstOrDefault(m => m.Attribute("Year")?.Value == "2024" &&
                                   m.Attribute("Month")?.Value == "1");

            Assert.NotNull(january);

            var topRoute = january.Element("TopRoute");
            Assert.NotNull(topRoute);

            // � ���: ������� 1 �� �������� ���� (3 ������: 2x10 + 1x10 = 30)
            Assert.Equal("1", topRoute.Attribute("Number")?.Value);
            Assert.Equal("30.00", topRoute.Attribute("TotalSum")?.Value);
        }

        [Fact]
        public void GetMonthlyTopRoutes_SortsByMonth()
        {
            // Act
            var result = XmlProcessor.GetMonthlyTopRoutes(
                _fixture.PassengersFile,
                _fixture.CategoriesFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert
            var months = result.Elements("Month")
                .Select(m => new {
                    Year = int.Parse(m.Attribute("Year")?.Value ?? "0"),
                    Month = int.Parse(m.Attribute("Month")?.Value ?? "0")
                })
                .ToList();

            // ���������� ���������� ������� ������
            for (int i = 1; i < months.Count; i++)
            {
                Assert.True(months[i].Year > months[i - 1].Year ||
                          (months[i].Year == months[i - 1].Year && months[i].Month > months[i - 1].Month));
            }
        }
    }

    /// <summary>
    /// ����� � ������������� Test Fixture
    /// </summary>
    public class XmlProcessorSinglePaymentTests : IDisposable
    {
        private readonly SinglePaymentTestFixture _fixture;

        public XmlProcessorSinglePaymentTests()
        {
            _fixture = new SinglePaymentTestFixture();
        }

        [Fact]
        public void GetPassengerTripCounts_HandlesEmptyPayments()
        {
            // Arrange
            var emptyPaymentsFile = System.IO.Path.Combine(_fixture.TestDirectory, "empty.xml");
            new XDocument(new XElement("PaymentRecords")).Save(emptyPaymentsFile);

            var passengersFile = System.IO.Path.Combine(_fixture.TestDirectory, "passengers.xml");
            new XDocument(
                new XElement("Passengers",
                    new XElement("Passenger", new XAttribute("Id", 1),
                        new XElement("Surname", "��������"),
                        new XElement("CategoryId", 1))
                )).Save(passengersFile);

            // Act
            var result = XmlProcessor.GetPassengerTripCounts(passengersFile, emptyPaymentsFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PassengerTripCounts", result.Name.LocalName);
            Assert.False(result.HasElements); // ���� ��������, �� ���� �������
        }

        [Fact]
        public void GetPassengerTripCounts_HandlesMultiplePaymentFiles()
        {
            // Arrange
            var passengersFile = System.IO.Path.Combine(_fixture.TestDirectory, "passengers_test.xml");
            new XDocument(
                new XElement("Passengers",
                    new XElement("Passenger", new XAttribute("Id", 1),
                        new XElement("Surname", "��������"),
                        new XElement("CategoryId", 1))
                )).Save(passengersFile);

            var payment2File = System.IO.Path.Combine(_fixture.TestDirectory, "payment2.xml");
            new XDocument(
                new XElement("PaymentRecords",
                    new XElement("PaymentRecord",
                        new XElement("Date", "2024-04-02"),
                        new XElement("PassengerId", 1),
                        new XElement("RouteNumber", 5))
                )).Save(payment2File);

            // Act
            var result = XmlProcessor.GetPassengerTripCounts(
                passengersFile,
                _fixture.SinglePaymentFile,
                payment2File);

            // Assert
            var passenger = result.Element("Passenger");
            Assert.NotNull(passenger);

            var route = passenger.Element("Route");
            Assert.NotNull(route);
            Assert.Equal("5", route.Attribute("Number")?.Value);
            Assert.Equal("2", route.Attribute("TripCount")?.Value); // 2 ������ �� �������� 5
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }

    /// <summary>
    /// ����������� �����
    /// </summary>
    public class XmlProcessorIntegrationTests : IClassFixture<TestDataFixture>
    {
        private readonly TestDataFixture _fixture;

        public XmlProcessorIntegrationTests(TestDataFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void BothMethods_ProduceValidXml()
        {
            // Act
            var tripCounts = XmlProcessor.GetPassengerTripCounts(
                _fixture.PassengersFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            var monthlyTop = XmlProcessor.GetMonthlyTopRoutes(
                _fixture.PassengersFile,
                _fixture.CategoriesFile,
                _fixture.PaymentsFile1,
                _fixture.PaymentsFile2);

            // Assert - ���������� �� XML �������
            Assert.DoesNotContain("xmlns", tripCounts.ToString());
            Assert.DoesNotContain("xmlns", monthlyTop.ToString());

            // ���������� �� ����� �������� �� �����������
            var tempFile = System.IO.Path.GetTempFileName();
            try
            {
                tripCounts.Save(tempFile);
                var loaded = XDocument.Load(tempFile);
                Assert.NotNull(loaded);

                monthlyTop.Save(tempFile);
                loaded = XDocument.Load(tempFile);
                Assert.NotNull(loaded);
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }
        }
    }
}