using System;

namespace PROG7311_P2.Exceptions
{
    public class FarmCentralException : Exception
    {
        public FarmCentralException() : base() { }
        public FarmCentralException(string message) : base(message) { }
        public FarmCentralException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ProductNotFoundException : FarmCentralException
    {
        public ProductNotFoundException(int productId) : base($"Product with ID {productId} was not found.") { }
    }

    public class ProductValidationException : FarmCentralException
    {
        public ProductValidationException(string message) : base(message) { }
    }

    public class UnauthorizedAccessException : FarmCentralException
    {
        public UnauthorizedAccessException(string message) : base(message) { }
    }

    public class DatabaseException : FarmCentralException
    {
        public DatabaseException(string message, Exception innerException) : base(message, innerException) { }
    }
} 