using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.Common;

public class AppException : Exception
{
    public AppException(string message) : base(message)
    {
    }
}
