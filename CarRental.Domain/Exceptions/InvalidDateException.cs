namespace CarRental.Domain.Exceptions;

public class InvalidDateException : Exception
{
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }

    public InvalidDateException(string message) : base(message)
    {
    }

    public InvalidDateException(string message, DateTime? startDate, DateTime? endDate) 
        : base(message)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public InvalidDateException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

