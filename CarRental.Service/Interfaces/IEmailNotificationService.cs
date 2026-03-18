namespace CarRental.Service.Interfaces
{
    public interface IEmailNotificationService
    {
        void SendWelcomeEmail(string email);
    }
}
