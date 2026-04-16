using Hotel.Domain.Entities;

namespace Hotel.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
}