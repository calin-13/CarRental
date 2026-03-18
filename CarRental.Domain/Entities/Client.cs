using System;

namespace CarRental.Domain.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }

        public Client() { }

        public Client(int id, string firstName, string lastName, string email, string licenseNumber)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            LicenseNumber = licenseNumber;
        }
    }
}
