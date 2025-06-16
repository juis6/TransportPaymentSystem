using System;

namespace TransportPaymentSystem
{
    public class Route
    {
        public int Number { get; set; }
        public string CurrentStop { get; set; } = string.Empty;
        public string FinalStop { get; set; } = string.Empty;
    }

    public class Passenger
    {
        public int Id { get; set; }
        public string Surname { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TripCost { get; set; }
    }

    public class PaymentRecord
    {
        public DateTime Date { get; set; }
        public int PassengerId { get; set; }
        public int RouteNumber { get; set; }
    }
}