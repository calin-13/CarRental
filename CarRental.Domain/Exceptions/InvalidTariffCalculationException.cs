namespace CarRental.Domain.Exceptions;

public class InvalidTariffCalculationException : Exception
{
    public InvalidTariffCalculationException(string message) : base(message)
    {
    }

    public InvalidTariffCalculationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
