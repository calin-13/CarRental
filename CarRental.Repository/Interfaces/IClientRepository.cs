using System.Collections.Generic;
using CarRental.Domain.Entities;

namespace CarRental.Repository.Interfaces
{
    public interface IClientRepository
    {
        IEnumerable<Client> GetAll();
        Client GetById(int id);
        void Add(Client client);
        void Update(Client client);
        void Delete(int id);
    }
}
