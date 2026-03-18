using System;
using System.Text.RegularExpressions;
using CarRental.Domain.Entities;
using CarRental.Repository.Interfaces;
using CarRental.Service.Interfaces;

namespace CarRental.Service.Services
{
    public class ClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IEmailNotificationService _emailNotificationService;

        private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        public ClientService(IClientRepository clientRepository, IEmailNotificationService emailNotificationService)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _emailNotificationService = emailNotificationService ?? throw new ArgumentNullException(nameof(emailNotificationService));
        }

        public void AddClient(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            ValidateClient(client);

            _clientRepository.Add(client);
            _emailNotificationService.SendWelcomeEmail(client.Email);
        }

        public Client GetClientById(int id)
        {
            return _clientRepository.GetById(id);
        }

        public void UpdateClient(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            ValidateClient(client);

            var existingClient = _clientRepository.GetById(client.Id);
            if (existingClient == null)
            {
                throw new InvalidOperationException($"Client with ID {client.Id} not found.");
            }

            _clientRepository.Update(client);
        }

        public void DeleteClient(int id)
        {
            var existingClient = _clientRepository.GetById(id);
            if (existingClient == null)
            {
                throw new InvalidOperationException($"Client with ID {id} not found.");
            }
            _clientRepository.Delete(id);
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, EmailPattern);
        }

        // Exposed for testing purposes primarily, or used internally
        private void ValidateClient(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.FirstName))
                throw new ArgumentException("First Name is required.");

            if (string.IsNullOrWhiteSpace(client.LastName))
                throw new ArgumentException("Last Name is required.");

            if (!IsValidEmail(client.Email))
                throw new ArgumentException("Invalid Email format.");

            if (!IsValidLicenseNumber(client.LicenseNumber))
                throw new ArgumentException("Invalid License Number. Must be between 6 and 15 alphanumeric characters.");
        }

        public bool IsValidLicenseNumber(string licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber)) return false;
            
            if (licenseNumber.Length < 6 || licenseNumber.Length > 15) return false;

            return Regex.IsMatch(licenseNumber, "^[a-zA-Z0-9]*$");
        }
    }
}
