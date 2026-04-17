using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.Interfaces;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string storedValue, string password);
    bool IsHashed(string storedValue);
}
