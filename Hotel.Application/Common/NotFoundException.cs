using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.Common;

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
