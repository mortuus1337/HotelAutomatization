using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.Common;

public class ValidationException : AppException
{
    public ValidationException(string message) : base(message)
    {
    }
}
