using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.Common;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
