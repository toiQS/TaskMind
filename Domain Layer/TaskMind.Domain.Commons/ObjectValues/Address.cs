using System;
using System.Collections.Generic;
using System.Text;

namespace TaskMind.Domain.Commons.ObjectValues
{
    public class Address
    {
        public string Street { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;

        public Address() { }

        public Address(string street, string city, string country)
        {
            Street = street;
            City = city;
            Country = country;
        }
    }
}
