using System;
using Xunit;
using Moq;
using CarRental.Domain.Entities;
using CarRental.Repository.Interfaces;
using CarRental.Service.Services;
using CarRental.Service.Interfaces;
using CarRental.Repository.Repositories; // Added for ClientRepository usage

namespace CarRental.Tests.Services
{
    public class ClientServiceTests
    {
        private readonly Mock<IClientRepository> _mockRepo;
        private readonly Mock<IEmailNotificationService> _mockEmailService;
        private readonly ClientService _clientService;

        public ClientServiceTests()
        {
            _mockRepo = new Mock<IClientRepository>();
            _mockEmailService = new Mock<IEmailNotificationService>();
            _clientService = new ClientService(_mockRepo.Object, _mockEmailService.Object);
        }

        // --- 1. Constructor Tests (2 tests) ---
        [Fact]
        public void Constructor_NullRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ClientService(null, _mockEmailService.Object));
        }

        [Fact]
        public void Constructor_NullEmailService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ClientService(_mockRepo.Object, null));
        }

        // --- 2. AddClient Validation Tests (10 tests) ---
        [Fact]
        public void AddClient_NullClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _clientService.AddClient(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddClient_InvalidFirstName_ThrowsArgumentException(string firstName)
        {
            var client = new Client { FirstName = firstName, LastName = "Doe", Email = "nb@test.com", LicenseNumber = "123456" };
            Assert.Throws<ArgumentException>(() => _clientService.AddClient(client));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddClient_InvalidLastName_ThrowsArgumentException(string lastName)
        {
            var client = new Client { FirstName = "John", LastName = lastName, Email = "nb@test.com", LicenseNumber = "123456" };
            Assert.Throws<ArgumentException>(() => _clientService.AddClient(client));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("plainaddress")]
        [InlineData("@missingusername.com")]
        [InlineData("username@.com")]
        public void AddClient_InvalidEmail_ThrowsArgumentException(string email)
        {
            var client = new Client { FirstName = "John", LastName = "Doe", Email = email, LicenseNumber = "123456" };
            Assert.Throws<ArgumentException>(() => _clientService.AddClient(client));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345")] // Too short
        [InlineData("1234567890123456")] // Too long (16 chars)
        [InlineData("Valid@License")] // Invalid char
        [InlineData("License space")] // Spaces
        public void AddClient_InvalidLicense_ThrowsArgumentException(string license)
        {
            var client = new Client { FirstName = "John", LastName = "Doe", Email = "nb@test.com", LicenseNumber = license };
            Assert.Throws<ArgumentException>(() => _clientService.AddClient(client));
        }

        // --- 3. AddClient Logic & Mocking Tests (4 tests) ---
        [Fact]
        public void AddClient_ValidData_CallsRepositoryMethod()
        {
            var client = new Client { FirstName = "John", LastName = "Doe", Email = "john@example.com", LicenseNumber = "AB123456" };
            
            _clientService.AddClient(client);

            _mockRepo.Verify(r => r.Add(client), Times.Once());
        }

        [Fact]
        public void AddClient_ValidData_CallsEmailServiceMethod()
        {
            var client = new Client { FirstName = "John", LastName = "Doe", Email = "john@example.com", LicenseNumber = "AB123456" };

            _clientService.AddClient(client);

            _mockEmailService.Verify(e => e.SendWelcomeEmail("john@example.com"), Times.Once());
        }

        [Fact]
        public void AddClient_ValidData_DoesNotThrow()
        {
            var client = new Client { FirstName = "John", LastName = "Doe", Email = "john@example.com", LicenseNumber = "AB123456" };
            var ex = Record.Exception(() => _clientService.AddClient(client));
            Assert.Null(ex);
        }
        
        [Fact]
        public void AddClient_RepoThrows_PropagatesException()
        {
            var client = new Client { FirstName = "John", LastName = "Doe", Email = "john@example.com", LicenseNumber = "AB123456" };
            _mockRepo.Setup(r => r.Add(It.IsAny<Client>())).Throws(new Exception("DB Error"));

            Assert.Throws<Exception>(() => _clientService.AddClient(client));
        }

        // --- 4. GetClientById Tests (2 tests) ---
        [Fact]
        public void GetClientById_CallsRepository()
        {
            _clientService.GetClientById(1);
            _mockRepo.Verify(r => r.GetById(1), Times.Once());
        }

        [Fact]
        public void GetClientById_ReturnsClientFromRepo()
        {
            var expectedClient = new Client { Id = 1, FirstName = "Test" };
            _mockRepo.Setup(r => r.GetById(1)).Returns(expectedClient);

            var result = _clientService.GetClientById(1);

            Assert.Same(expectedClient, result);
        }

        // --- 5. UpdateClient Tests (4 tests) ---
        [Fact]
        public void UpdateClient_NullClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _clientService.UpdateClient(null));
        }

        [Fact]
        public void UpdateClient_NonExistentClient_ThrowsInvalidOperationException()
        {
            var client = new Client { Id = 99, FirstName = "John", LastName = "Doe", Email = "john@test.com", LicenseNumber = "123456" };
            _mockRepo.Setup(r => r.GetById(99)).Returns((Client)null);

            Assert.Throws<InvalidOperationException>(() => _clientService.UpdateClient(client));
        }

        [Fact]
        public void UpdateClient_ValidClient_CallsRepositoryUpdate()
        {
            var client = new Client { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", LicenseNumber = "123456" };
            _mockRepo.Setup(r => r.GetById(1)).Returns(new Client()); // Found

            _clientService.UpdateClient(client);

            _mockRepo.Verify(r => r.Update(client), Times.Once());
        }

        [Fact]
        public void UpdateClient_ValidClient_ChecksExistenceFirst()
        {
             var client = new Client { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", LicenseNumber = "123456" };
             _mockRepo.Setup(r => r.GetById(1)).Returns(new Client());

             _clientService.UpdateClient(client);
             
             // Verify GetById was called before Update
             _mockRepo.Verify(r => r.GetById(1), Times.Once());
        }

        // --- 6. DeleteClient Tests (2 tests) ---
        [Fact]
        public void DeleteClient_NonExistentId_ThrowsInvalidOperationException()
        {
            _mockRepo.Setup(r => r.GetById(99)).Returns((Client)null);
            Assert.Throws<InvalidOperationException>(() => _clientService.DeleteClient(99));
        }

        [Fact]
        public void DeleteClient_ValidId_CallsRepositoryDelete()
        {
            _mockRepo.Setup(r => r.GetById(1)).Returns(new Client()); // Exists
            _clientService.DeleteClient(1);
            _mockRepo.Verify(r => r.Delete(1), Times.Once());
        }

        // --- 7. Additional Helper/Edge Case Tests (3 tests) ---
        
        [Fact]
        public void IsValidEmail_ReturnsFalse_ForNull()
        {
            Assert.False(_clientService.IsValidEmail(null));
        }

        [Fact]
        public void IsValidLicense_ReturnsFalse_ForNull()
        {
            Assert.False(_clientService.IsValidLicenseNumber(null));
        }

        [Fact]
        public void AddClient_WithExactBoundaryLengthLicense_Succeeds()
        {
            // 6 chars (min)
            var client = new Client { FirstName = "Min", LastName = "Len", Email = "m@t.com", LicenseNumber = "123456" };
            _clientService.AddClient(client);
            _mockRepo.Verify(r => r.Add(client), Times.Once());
        }
        
        // --- 8. Repository Integrated Tests (Moved from ClientRepositoryTests) ---
        [Fact]
        public void Repository_Add_AddsClientAndAutoIncrementsId()
        {
            var repo = new ClientRepository();
            var client = new Client { FirstName = "Test", LastName = "User" };

            repo.Add(client);

            var all = repo.GetAll();
            Assert.Single(all);
            Assert.Equal(1, client.Id);
        }

        [Fact]
        public void Repository_Add_ValidClient_RetrievableById()
        {
            var repo = new ClientRepository();
            var client = new Client { FirstName = "Popescu", LastName = "Ion" };
            repo.Add(client);

            var retrieved = repo.GetById(client.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Popescu", retrieved.FirstName);
        }

        [Fact]
        public void Repository_Update_UpdatesExistingClient()
        {
            var repo = new ClientRepository();
            var client = new Client { FirstName = "Old", LastName = "Name" };
            repo.Add(client);

            client.FirstName = "New";
            repo.Update(client);

            var retrieved = repo.GetById(client.Id);
            Assert.Equal("New", retrieved.FirstName);
        }

        [Fact]
        public void Repository_Delete_RemovesClient()
        {
            var repo = new ClientRepository();
            var client = new Client { FirstName = "To", LastName = "Delete" };
            repo.Add(client);

            repo.Delete(client.Id);

            var retrieved = repo.GetById(client.Id);
            Assert.Null(retrieved);
            Assert.Empty(repo.GetAll());
        }
    }
}
