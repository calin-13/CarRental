using System;
using System.Collections.Generic;
using System.Linq;
using CarRental.Domain.Entities;
using CarRental.Repository.Interfaces;

namespace CarRental.Repository.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly List<Client> _clients = new List<Client>();

        public IEnumerable<Client> GetAll()
        {
            return _clients;
        }

        public Client GetById(int id)
        {
            return _clients.FirstOrDefault(c => c.Id == id);
        }

        public void Add(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            if (client.Id == 0)
            {
                client.Id = _clients.Any() ? _clients.Max(c => c.Id) + 1 : 1;
            }
            
            _clients.Add(client);
        }

        public void Update(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            var existingClient = GetById(client.Id);
            if (existingClient != null)
            {
                existingClient.FirstName = client.FirstName;
                existingClient.LastName = client.LastName;
                existingClient.Email = client.Email;
                existingClient.LicenseNumber = client.LicenseNumber;
            }
        }

        public void Delete(int id)
        {
            var client = GetById(id);
            if (client != null)
            {
                _clients.Remove(client);
            }
        }
    }
}
