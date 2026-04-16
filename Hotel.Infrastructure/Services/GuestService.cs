using Hotel.Application.Common;
using Hotel.Application.DTOs.Guests;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class GuestService : IGuestService
{
    private readonly HotelDbContext _dbContext;

    public GuestService(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Guests
            .AsNoTracking()
            .Include(x => x.GuestIdentity)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new GuestDto
            {
                GuestId = x.GuestId,
                LastName = x.LastName,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                Phone = x.Phone,
                Email = x.Email,
                CreatedAt = x.CreatedAt,
                Identity = x.GuestIdentity == null ? null : new GuestIdentityDto
                {
                    GuestId = x.GuestIdentity.GuestId,
                    DocType = x.GuestIdentity.DocType,
                    DocNumber = x.GuestIdentity.DocNumber,
                    IssuedBy = x.GuestIdentity.IssuedBy,
                    IssuedDate = x.GuestIdentity.IssuedDate.HasValue
                        ? x.GuestIdentity.IssuedDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                    BirthDate = x.GuestIdentity.BirthDate.HasValue
                        ? x.GuestIdentity.BirthDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                    Citizenship = x.GuestIdentity.Citizenship,
                    Address = x.GuestIdentity.Address
                }
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<GuestDto> GetByIdAsync(int guestId, CancellationToken cancellationToken = default)
    {
        var guest = await _dbContext.Guests
            .AsNoTracking()
            .Include(x => x.GuestIdentity)
            .FirstOrDefaultAsync(x => x.GuestId == guestId, cancellationToken);

        if (guest is null)
            throw new NotFoundException("Гость не найден.");

        return MapGuest(guest);
    }

    public async Task<GuestDto> CreateAsync(CreateGuestDto request, CancellationToken cancellationToken = default)
    {
        ValidateGuest(request.LastName, request.FirstName);
        ValidateIdentity(request.DocType, request.DocNumber, request.IssuedBy, request.Citizenship, request.Address);

        var guest = new Guest
        {
            LastName = request.LastName.Trim(),
            FirstName = request.FirstName.Trim(),
            MiddleName = Normalize(request.MiddleName),
            Phone = Normalize(request.Phone),
            Email = Normalize(request.Email),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Guests.Add(guest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (HasIdentityData(
                request.DocType,
                request.DocNumber,
                request.IssuedBy,
                request.IssuedDate,
                request.BirthDate,
                request.Citizenship,
                request.Address))
        {
            var identity = new GuestIdentity
            {
                GuestId = guest.GuestId,
                DocType = request.DocType!.Trim(),
                DocNumber = request.DocNumber!.Trim(),
                IssuedBy = Normalize(request.IssuedBy),
                IssuedDate = request.IssuedDate.HasValue
                    ? DateOnly.FromDateTime(request.IssuedDate.Value)
                    : null,
                BirthDate = request.BirthDate.HasValue
                    ? DateOnly.FromDateTime(request.BirthDate.Value)
                    : null,
                Citizenship = Normalize(request.Citizenship),
                Address = Normalize(request.Address)
            };

            _dbContext.GuestIdentities.Add(identity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await GetByIdAsync(guest.GuestId, cancellationToken);
    }

    public async Task<GuestDto> UpdateAsync(int guestId, UpdateGuestDto request, CancellationToken cancellationToken = default)
    {
        ValidateGuest(request.LastName, request.FirstName);
        ValidateIdentity(request.DocType, request.DocNumber, request.IssuedBy, request.Citizenship, request.Address);

        var guest = await _dbContext.Guests
            .Include(x => x.GuestIdentity)
            .FirstOrDefaultAsync(x => x.GuestId == guestId, cancellationToken);

        if (guest is null)
            throw new NotFoundException("Гость не найден.");

        guest.LastName = request.LastName.Trim();
        guest.FirstName = request.FirstName.Trim();
        guest.MiddleName = Normalize(request.MiddleName);
        guest.Phone = Normalize(request.Phone);
        guest.Email = Normalize(request.Email);

        if (HasIdentityData(
                request.DocType,
                request.DocNumber,
                request.IssuedBy,
                request.IssuedDate,
                request.BirthDate,
                request.Citizenship,
                request.Address))
        {
            if (guest.GuestIdentity is null)
            {
                guest.GuestIdentity = new GuestIdentity
                {
                    GuestId = guest.GuestId
                };
            }

            guest.GuestIdentity.DocType = request.DocType!.Trim();
            guest.GuestIdentity.DocNumber = request.DocNumber!.Trim();
            guest.GuestIdentity.IssuedBy = Normalize(request.IssuedBy);
            guest.GuestIdentity.IssuedDate = request.IssuedDate.HasValue
                ? DateOnly.FromDateTime(request.IssuedDate.Value)
                : null;
            guest.GuestIdentity.BirthDate = request.BirthDate.HasValue
                ? DateOnly.FromDateTime(request.BirthDate.Value)
                : null;
            guest.GuestIdentity.Citizenship = Normalize(request.Citizenship);
            guest.GuestIdentity.Address = Normalize(request.Address);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(guestId, cancellationToken);
    }

    private static GuestDto MapGuest(Guest guest)
    {
        return new GuestDto
        {
            GuestId = guest.GuestId,
            LastName = guest.LastName,
            FirstName = guest.FirstName,
            MiddleName = guest.MiddleName,
            Phone = guest.Phone,
            Email = guest.Email,
            CreatedAt = guest.CreatedAt,
            Identity = guest.GuestIdentity == null ? null : new GuestIdentityDto
            {
                GuestId = guest.GuestIdentity.GuestId,
                DocType = guest.GuestIdentity.DocType,
                DocNumber = guest.GuestIdentity.DocNumber,
                IssuedBy = guest.GuestIdentity.IssuedBy,
                IssuedDate = guest.GuestIdentity.IssuedDate.HasValue
                    ? guest.GuestIdentity.IssuedDate.Value.ToDateTime(TimeOnly.MinValue)
                    : null,
                BirthDate = guest.GuestIdentity.BirthDate.HasValue
                    ? guest.GuestIdentity.BirthDate.Value.ToDateTime(TimeOnly.MinValue)
                    : null,
                Citizenship = guest.GuestIdentity.Citizenship,
                Address = guest.GuestIdentity.Address
            }
        };
    }

    private static void ValidateGuest(string lastName, string firstName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ValidationException("Фамилия обязательна.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ValidationException("Имя обязательно.");
    }

    private static void ValidateIdentity(
        string? docType,
        string? docNumber,
        string? issuedBy,
        string? citizenship,
        string? address)
    {
        if (!HasIdentityData(docType, docNumber, issuedBy, null, null, citizenship, address))
            return;

        if (string.IsNullOrWhiteSpace(docType))
            throw new ValidationException("Для документа гостя обязательно указать тип документа.");

        if (string.IsNullOrWhiteSpace(docNumber))
            throw new ValidationException("Для документа гостя обязательно указать номер документа.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool HasIdentityData(
        string? docType,
        string? docNumber,
        string? issuedBy,
        DateTime? issuedDate,
        DateTime? birthDate,
        string? citizenship,
        string? address)
    {
        return !string.IsNullOrWhiteSpace(docType)
               || !string.IsNullOrWhiteSpace(docNumber)
               || !string.IsNullOrWhiteSpace(issuedBy)
               || issuedDate.HasValue
               || birthDate.HasValue
               || !string.IsNullOrWhiteSpace(citizenship)
               || !string.IsNullOrWhiteSpace(address);
    }
}
