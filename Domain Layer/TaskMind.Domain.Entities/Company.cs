using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.ObjectValues;

namespace TaskMind.Domain.Entities
{
    internal class Company : EntityBase
    {
        public string Name { get; private set; }
        public Address Address { get; private set; } = new Address();
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; }
        public string TaxCode { get; private set; }
        public DateTime JoinDate { get; private set; }

        private Company(string name, Address address, string email, string phone, string taxCode, DateTime joinDate) : base()
        {
            Name = name;
            Address = address;
            Email = email;
            Phone = phone;
            TaxCode = taxCode;
            JoinDate = joinDate;
        }
        
    }
}
