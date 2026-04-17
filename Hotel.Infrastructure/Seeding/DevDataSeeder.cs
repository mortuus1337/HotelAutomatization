using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hotel.Infrastructure.Seeding;

public class DevDataSeeder
{
    private readonly HotelDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<DevDataSeeder> _logger;
    private readonly Random _random = new(20260417);

    public DevDataSeeder(
        HotelDbContext dbContext,
        IPasswordService passwordService,
        ILogger<DevDataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SyncPostgresSequencesAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
        await SeedMealPlansAsync(cancellationToken);
        await SeedRoomTypesAndRoomsAsync(cancellationToken);
        await SeedDeterministicManualTestDataAsync(cancellationToken);
    }

    private async Task SeedDeterministicManualTestDataAsync(CancellationToken cancellationToken)
    {
        const int rows = 20;

        var admins = await _dbContext.AppUsers
            .Where(x => x.RoleCode == Roles.Admin && x.IsActive)
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);

        var rooms = await _dbContext.Rooms
            .Include(x => x.RoomType)
            .Where(x => x.IsActive)
            .OrderBy(x => x.RoomNumber)
            .ToListAsync(cancellationToken);

        var mealPlans = await _dbContext.MealPlans
            .OrderBy(x => x.MealPlanId)
            .ToListAsync(cancellationToken);

        if (admins.Count == 0 || rooms.Count == 0 || mealPlans.Count == 0)
            return;

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE generated_document, stay_operation, stay_guest, stay, reservation_room, reservation, guest_identity, guest, work_shift RESTART IDENTITY CASCADE;",
            cancellationToken);

        var persons = new (string LastName, string FirstName, string? MiddleName)[]
        {
            ("Иванов", "Иван", "Алексеевич"),
            ("Петрова", "Елена", "Игоревна"),
            ("Смирнов", "Максим", "Павлович"),
            ("Кузнецова", "Ольга", "Сергеевна"),
            ("Соколов", "Кирилл", "Дмитриевич"),
            ("Волкова", "Наталья", "Владимировна"),
            ("Новиков", "Никита", "Андреевич"),
            ("Лебедева", "Алиса", "Михайловна"),
            ("Крылов", "Павел", "Егорович"),
            ("Громова", "Дарья", "Романовна"),
            ("Беляев", "Георгий", "Сергеевич"),
            ("Орлова", "Виктория", "Алексеевна"),
            ("Макаров", "Дмитрий", "Ильич"),
            ("Федорова", "Мария", "Викторовна"),
            ("Ларионов", "Артем", "Олегович"),
            ("Климова", "Полина", "Денисовна"),
            ("Миронов", "Роман", "Игоревич"),
            ("Ершова", "София", "Павловна"),
            ("Гаврилов", "Тимофей", "Андреевич"),
            ("Рябова", "Вероника", "Владимировна")
        };

        var guests = new List<Guest>(rows);
        for (var i = 1; i <= rows; i++)
        {
            var person = persons[i - 1];
            guests.Add(new Guest
            {
                LastName = person.LastName,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                Phone = $"+7991100{i:0000}",
                Email = $"guest{i}@hotel.local",
                CreatedAt = DateTimeOffset.Parse("2026-01-01T09:00:00+00:00").AddDays(i),
                GuestIdentity = new GuestIdentity
                {
                    DocType = "Паспорт РФ",
                    DocNumber = $"{4100 + i:0000} {120000 + i * 37:000000}",
                    IssuedBy = "ГУ МВД России по Новосибирской области",
                    IssuedDate = DateOnly.Parse("2021-01-15").AddDays(i),
                    BirthDate = DateOnly.Parse("1990-01-10").AddDays(i * 31),
                    Citizenship = i % 6 == 0 ? "KZ" : "RU",
                    Address = $"г. Новосибирск, ул. Тестовая, д. {i}"
                }
            });
        }

        _dbContext.Guests.AddRange(guests);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var reservationStatuses = new[]
        {
            ReservationStatuses.Created, ReservationStatuses.Confirmed, ReservationStatuses.CheckedIn, ReservationStatuses.Completed, ReservationStatuses.Cancelled
        };

        var sourceNames = new[]
        {
            "Стойка регистрации", "Сайт гостиницы", "Телефон", "OTA", "Корпоративный клиент"
        };

        var reservationBaseDate = DateOnly.Parse("2026-04-01");
        var reservations = new List<Reservation>(rows);
        for (var i = 1; i <= rows; i++)
        {
            var status = reservationStatuses[(i - 1) % reservationStatuses.Length];
            var plannedCheckin = reservationBaseDate.AddDays(i - 8);
            var plannedCheckout = plannedCheckin.AddDays(1 + (i % 4));
            var room = rooms[(i - 1) % rooms.Count];
            var nightly = room.RoomType.BasePrice;
            var nights = plannedCheckout.DayNumber - plannedCheckin.DayNumber;
            var total = nightly * nights;

            reservations.Add(new Reservation
            {
                CreatedAt = DateTimeOffset.Parse("2026-03-01T08:00:00+00:00").AddDays(i),
                CreatedByUserId = admins[(i - 1) % admins.Count].UserId,
                Status = status,
                Source = sourceNames[(i - 1) % sourceNames.Length],
                CustomerName = $"{guests[i - 1].LastName} {guests[i - 1].FirstName}",
                CustomerPhone = guests[i - 1].Phone,
                Comment = $"Тестовая бронь #{i}",
                PlannedCheckin = plannedCheckin,
                PlannedCheckout = plannedCheckout,
                Adults = room.RoomType.Capacity > 1 ? 2 : 1,
                Children = room.RoomType.Capacity > 2 && i % 3 == 0 ? 1 : 0,
                TotalPrice = total,
                Prepayment = Math.Round(total * 0.30m, 2),
                MealPlanId = mealPlans[(i - 1) % mealPlans.Count].MealPlanId
            });
        }

        _dbContext.Reservations.AddRange(reservations);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var reservationRooms = new List<ReservationRoom>(rows);
        for (var i = 1; i <= rows; i++)
        {
            var room = rooms[(i - 1) % rooms.Count];
            reservationRooms.Add(new ReservationRoom
            {
                ReservationId = reservations[i - 1].ReservationId,
                RoomId = room.RoomId,
                PricePerNight = room.RoomType.BasePrice
            });
        }

        _dbContext.ReservationRooms.AddRange(reservationRooms);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var stays = new List<Stay>(rows);
        for (var i = 1; i <= rows; i++)
        {
            var status = StayStatuses.Active;
            var reservation = reservations[i - 1];
            var plannedCheckin = reservation.PlannedCheckin;
            var plannedCheckout = reservation.PlannedCheckout;

            var actualCheckin = new DateTimeOffset(plannedCheckin.ToDateTime(new TimeOnly(14, 0), DateTimeKind.Utc));
            DateTimeOffset? actualCheckout = null;

            stays.Add(new Stay
            {
                ReservationId = reservation.ReservationId,
                RoomId = reservationRooms[i - 1].RoomId,
                Status = status,
                ActualCheckin = actualCheckin,
                ActualCheckout = actualCheckout,
                PlannedCheckin = plannedCheckin,
                PlannedCheckout = plannedCheckout,
                MealPlanId = reservation.MealPlanId,
                CreatedByUserId = admins[(i - 1) % admins.Count].UserId,
                Comment = $"Тестовое проживание #{i}"
            });
        }

        _dbContext.Stays.AddRange(stays);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var stayGuests = new List<StayGuest>(rows);
        for (var i = 1; i <= rows; i++)
        {
            stayGuests.Add(new StayGuest
            {
                StayId = stays[i - 1].StayId,
                GuestId = guests[i - 1].GuestId,
                IsMain = true
            });
        }

        _dbContext.StayGuests.AddRange(stayGuests);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var stayOperations = new List<StayOperation>(rows);
        for (var i = 1; i <= rows; i++)
        {
            var stay = stays[i - 1];
            var opType = stay.Status switch
            {
                _ => "CheckIn"
            };

            stayOperations.Add(new StayOperation
            {
                StayId = stay.StayId,
                UserId = admins[(i - 1) % admins.Count].UserId,
                OperationType = opType,
                OccurredAt = DateTimeOffset.Parse("2026-04-01T10:00:00+00:00").AddHours(i),
                Comment = $"Тестовая операция #{i}"
            });
        }

        _dbContext.StayOperations.AddRange(stayOperations);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var shifts = new List<WorkShift>(rows);
        for (var i = 1; i <= rows; i++)
        {
            var startedAt = DateTimeOffset.Parse("2026-03-10T06:00:00+00:00").AddDays(i);
            shifts.Add(new WorkShift
            {
                UserId = admins[(i - 1) % admins.Count].UserId,
                StartedAt = startedAt,
                EndedAt = startedAt.AddHours(8),
                Status = "Closed",
                Comment = $"Тестовая смена #{i}"
            });
        }

        _dbContext.WorkShifts.AddRange(shifts);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var documents = new List<GeneratedDocument>(rows);
        for (var i = 1; i <= rows; i++)
        {
            if (i % 3 == 0)
            {
                documents.Add(new GeneratedDocument
                {
                    DocumentType = "CheckoutAct",
                    FileName = $"checkout-act-{stays[i - 1].StayId}.pdf",
                    EntityType = "Stay",
                    EntityId = stays[i - 1].StayId,
                    GeneratedAt = DateTimeOffset.Parse("2026-04-01T11:00:00+00:00").AddMinutes(i * 5),
                    GeneratedByUserId = admins[(i - 1) % admins.Count].UserId
                });
            }
            else if (i % 2 == 0)
            {
                documents.Add(new GeneratedDocument
                {
                    DocumentType = "Invoice",
                    FileName = $"invoice-{reservations[i - 1].ReservationId}.pdf",
                    EntityType = "Reservation",
                    EntityId = reservations[i - 1].ReservationId,
                    GeneratedAt = DateTimeOffset.Parse("2026-04-01T11:00:00+00:00").AddMinutes(i * 5),
                    GeneratedByUserId = admins[(i - 1) % admins.Count].UserId
                });
            }
            else
            {
                documents.Add(new GeneratedDocument
                {
                    DocumentType = "ServiceContract",
                    FileName = $"service-contract-{reservations[i - 1].ReservationId}.pdf",
                    EntityType = "Reservation",
                    EntityId = reservations[i - 1].ReservationId,
                    GeneratedAt = DateTimeOffset.Parse("2026-04-01T11:00:00+00:00").AddMinutes(i * 5),
                    GeneratedByUserId = admins[(i - 1) % admins.Count].UserId
                });
            }
        }

        _dbContext.GeneratedDocuments.AddRange(documents);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }

    private async Task SyncPostgresSequencesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var sequenceSql = new[]
            {
                "SELECT setval(pg_get_serial_sequence('app_user','user_id'), COALESCE((SELECT MAX(user_id) FROM app_user), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('room_type','room_type_id'), COALESCE((SELECT MAX(room_type_id) FROM room_type), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('room','room_id'), COALESCE((SELECT MAX(room_id) FROM room), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('guest','guest_id'), COALESCE((SELECT MAX(guest_id) FROM guest), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('meal_plan','meal_plan_id'), COALESCE((SELECT MAX(meal_plan_id) FROM meal_plan), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('reservation','reservation_id'), COALESCE((SELECT MAX(reservation_id) FROM reservation), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('reservation_room','reservation_room_id'), COALESCE((SELECT MAX(reservation_room_id) FROM reservation_room), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('stay','stay_id'), COALESCE((SELECT MAX(stay_id) FROM stay), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('stay_operation','stay_operation_id'), COALESCE((SELECT MAX(stay_operation_id) FROM stay_operation), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('work_shift','work_shift_id'), COALESCE((SELECT MAX(work_shift_id) FROM work_shift), 0) + 1, false);",
                "SELECT setval(pg_get_serial_sequence('generated_document','generated_document_id'), COALESCE((SELECT MAX(generated_document_id) FROM generated_document), 0) + 1, false);"
            };

            foreach (var sql in sequenceSql)
            {
                await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not sync PostgreSQL sequences before seeding.");
        }
    }

    private async Task NormalizeLegacySeedDataAsync(CancellationToken cancellationToken)
    {
        var rooms = await _dbContext.Rooms
            .Where(x => x.Notes != null && (EF.Functions.ILike(x.Notes, "%seed%") || EF.Functions.ILike(x.Notes, "%?%")))
            .ToListAsync(cancellationToken);

        foreach (var room in rooms)
            room.Notes = "Номер доступен к продаже";

        var reservations = await _dbContext.Reservations
            .Where(x =>
                (x.CustomerName != null && (EF.Functions.ILike(x.CustomerName, "%seed%") || EF.Functions.ILike(x.CustomerName, "%?%"))) ||
                (x.Comment != null && (EF.Functions.ILike(x.Comment, "%seed%") || EF.Functions.ILike(x.Comment, "%?%"))))
            .OrderBy(x => x.ReservationId)
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            reservation.CustomerName = $"Гость #{reservation.ReservationId}";
            reservation.Comment = reservation.Status switch
            {
                ReservationStatuses.Created => "Новая бронь, ожидает подтверждения",
                ReservationStatuses.Confirmed => "Бронь подтверждена",
                ReservationStatuses.CheckedIn => "Гость заселен",
                ReservationStatuses.Completed => "Проживание завершено",
                ReservationStatuses.NoShow => "Гость не прибыл",
                ReservationStatuses.Cancelled => "Бронирование отменено по запросу",
                _ => "Рабочая бронь"
            };
        }

        var stays = await _dbContext.Stays
            .Where(x => x.Comment != null && (EF.Functions.ILike(x.Comment, "%seed%") || EF.Functions.ILike(x.Comment, "%?%")))
            .ToListAsync(cancellationToken);

        foreach (var stay in stays)
        {
            stay.Comment = stay.Status switch
            {
                StayStatuses.Active => "Идет проживание",
                StayStatuses.Planned => "Ожидается заезд",
                StayStatuses.CheckedOut => "Выезд оформлен",
                StayStatuses.Cancelled => "Заселение отменено",
                _ => "Проживание"
            };
        }

        var operations = await _dbContext.StayOperations
            .Where(x => x.Comment != null && (EF.Functions.ILike(x.Comment, "%seed%") || EF.Functions.ILike(x.Comment, "%?%")))
            .ToListAsync(cancellationToken);

        foreach (var operation in operations)
        {
            operation.Comment = operation.OperationType switch
            {
                "CheckIn" => "Регистрация заезда",
                "CheckOut" => "Оформление выезда",
                "RoomService" => "Обслуживание номера",
                "CancelStay" => "Отмена проживания",
                _ => "Операция по проживанию"
            };
        }

        var shifts = await _dbContext.WorkShifts
            .Where(x => x.Comment != null && (EF.Functions.ILike(x.Comment, "%seed%") || EF.Functions.ILike(x.Comment, "%?%")))
            .ToListAsync(cancellationToken);

        foreach (var shift in shifts)
            shift.Comment = shift.Status == "Open" ? "Активная смена" : "Плановая закрытая смена";

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.AppUsers.ToListAsync(cancellationToken);

        EnsureUser(users, "admin", "admin123", "Front Desk Admin", Roles.Admin);
        EnsureUser(users, "owner", "owner123", "Hotel Owner", Roles.Owner);
        EnsureUser(users, "admin2", "admin2123", "Night Shift Admin", Roles.Admin);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void EnsureUser(IReadOnlyCollection<AppUser> users, string login, string password, string fullName, string roleCode)
    {
        if (users.Any(x => x.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
            return;

        _dbContext.AppUsers.Add(new AppUser
        {
            Login = login,
            PasswordHash = _passwordService.Hash(password),
            FullName = fullName,
            RoleCode = roleCode,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private async Task SeedMealPlansAsync(CancellationToken cancellationToken)
    {
        var mealPlans = await _dbContext.MealPlans.ToListAsync(cancellationToken);

        EnsureMealPlan(mealPlans, "No meal", 0m);
        EnsureMealPlan(mealPlans, "Breakfast", 450m);
        EnsureMealPlan(mealPlans, "Half board", 900m);
        EnsureMealPlan(mealPlans, "Full board", 1500m);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void EnsureMealPlan(IReadOnlyCollection<MealPlan> mealPlans, string name, decimal price)
    {
        if (mealPlans.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return;

        _dbContext.MealPlans.Add(new MealPlan
        {
            Name = name,
            PricePerPersonPerDay = price
        });
    }

    private async Task<Dictionary<string, RoomType>> SeedRoomTypesAndRoomsAsync(CancellationToken cancellationToken)
    {
        var roomTypes = await _dbContext.RoomTypes
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        RoomType EnsureRoomType(string name, int capacity, decimal basePrice, string description)
        {
            if (roomTypes.TryGetValue(name, out var existing))
                return existing;

            var created = new RoomType
            {
                Name = name,
                Capacity = capacity,
                BasePrice = basePrice,
                Description = description
            };

            _dbContext.RoomTypes.Add(created);
            roomTypes[name] = created;
            return created;
        }

        EnsureRoomType("Single", 1, 2900m, "Single room");
        EnsureRoomType("Twin", 2, 3600m, "Twin beds");
        EnsureRoomType("Double", 2, 3900m, "Double bed");
        EnsureRoomType("Comfort", 2, 4600m, "Enhanced comfort");
        EnsureRoomType("Family", 4, 6200m, "Family room");
        EnsureRoomType("Lux", 3, 7800m, "Luxury room");

        await _dbContext.SaveChangesAsync(cancellationToken);

        roomTypes = await _dbContext.RoomTypes
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingRooms = await _dbContext.Rooms
            .Select(x => x.RoomNumber)
            .ToListAsync(cancellationToken);

        var existingNumbers = existingRooms.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var roomPlan = new[]
        {
            ("101", "Single", 1), ("102", "Single", 1), ("103", "Twin", 1), ("104", "Double", 1),
            ("105", "Comfort", 1), ("106", "Comfort", 1),
            ("201", "Single", 2), ("202", "Twin", 2), ("203", "Double", 2), ("204", "Comfort", 2),
            ("205", "Comfort", 2), ("206", "Family", 2),
            ("301", "Single", 3), ("302", "Twin", 3), ("303", "Double", 3), ("304", "Comfort", 3),
            ("305", "Family", 3), ("306", "Lux", 3),
            ("401", "Single", 4), ("402", "Double", 4), ("403", "Comfort", 4), ("404", "Lux", 4)
        };

        foreach (var (roomNumber, typeName, floor) in roomPlan)
        {
            if (existingNumbers.Contains(roomNumber))
                continue;

            _dbContext.Rooms.Add(new Room
            {
                RoomNumber = roomNumber,
                RoomTypeId = roomTypes[typeName].RoomTypeId,
                Floor = floor,
                IsActive = true,
                Notes = "Номер доступен к продаже"
            });

            existingNumbers.Add(roomNumber);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return roomTypes;
    }

    private async Task SeedGuestsAsync(CancellationToken cancellationToken)
    {
        const int targetGuests = 96;

        var existingGuests = await _dbContext.Guests
            .Include(x => x.GuestIdentity)
            .OrderBy(x => x.GuestId)
            .ToListAsync(cancellationToken);

        var maleFirstNames = new[]
        {
            "Алексей", "Иван", "Дмитрий", "Максим", "Егор", "Кирилл", "Артем", "Никита", "Роман", "Павел",
            "Сергей", "Антон", "Георгий", "Владимир", "Михаил", "Илья", "Денис", "Андрей", "Тимофей"
        };

        var femaleFirstNames = new[]
        {
            "Анна", "Елена", "Мария", "Ольга", "Ксения", "Наталья", "Полина", "Алиса", "Екатерина", "Дарья",
            "Виктория", "София", "Людмила", "Татьяна", "Ирина", "Вероника", "Лилия", "Нина", "Оксана"
        };

        var lastNamePairs = new (string Male, string Female)[]
        {
            ("Иванов", "Иванова"), ("Петров", "Петрова"), ("Смирнов", "Смирнова"), ("Кузнецов", "Кузнецова"),
            ("Соколов", "Соколова"), ("Волков", "Волкова"), ("Новиков", "Новикова"), ("Лебедев", "Лебедева"),
            ("Крылов", "Крылова"), ("Громов", "Громова"), ("Федоров", "Федорова"), ("Беляев", "Беляева"),
            ("Гаврилов", "Гаврилова"), ("Ларионов", "Ларионова"), ("Климов", "Климова"), ("Миронов", "Миронова"),
            ("Ершов", "Ершова"), ("Борисов", "Борисова"), ("Орлов", "Орлова"), ("Макаров", "Макарова"),
            ("Рябов", "Рябова"), ("Гордеев", "Гордеева"), ("Власов", "Власова"), ("Журавлев", "Журавлева"),
            ("Егоров", "Егорова"), ("Медведев", "Медведева"), ("Ковалев", "Ковалева"), ("Самойлов", "Самойлова")
        };

        var patronymicPairs = new (string Male, string Female)[]
        {
            ("Алексеевич", "Алексеевна"), ("Игоревич", "Игоревна"), ("Павлович", "Павловна"), ("Сергеевич", "Сергеевна"),
            ("Дмитриевич", "Дмитриевна"), ("Владимирович", "Владимировна"), ("Андреевич", "Андреевна"), ("Михайлович", "Михайловна"),
            ("Егорович", "Егоровна"), ("Кириллович", "Кирилловна"), ("Романович", "Романовна"), ("Ильич", "Ильинична"),
            ("Артемович", "Артемовна"), ("Олегович", "Олеговна"), ("Викторович", "Викторовна"), ("Георгиевич", "Георгиевна")
        };

        var authorities = new[]
        {
            "ГУ МВД России по Новосибирской области",
            "Отдел по вопросам миграции МВД России",
            "ОВМ УМВД России",
            "МФЦ Новосибирской области"
        };

        var streets = new[]
        {
            "Красный проспект", "Советская", "Гоголя", "Ленина", "Крылова", "Нарымская", "Державина", "Орджоникидзе", "Ядринцевская", "Мичурина"
        };

        static string BuildDocNumber(int serial)
            => $"{1000 + (serial % 9000):0000} {100000 + ((serial * 7919) % 900000):000000}";

        void ApplyGuestData(Guest guest, int serial)
        {
            var isFemale = serial % 2 == 0;
            var firstName = isFemale
                ? femaleFirstNames[(serial * 7 + 3) % femaleFirstNames.Length]
                : maleFirstNames[(serial * 7 + 3) % maleFirstNames.Length];

            var surname = lastNamePairs[(serial * 11 + 5) % lastNamePairs.Length];
            var patronymic = patronymicPairs[(serial * 13 + 7) % patronymicPairs.Length];

            guest.FirstName = firstName;
            guest.LastName = isFemale ? surname.Female : surname.Male;
            guest.MiddleName = serial % 9 == 0 ? null : (isFemale ? patronymic.Female : patronymic.Male);
            guest.Phone = $"+7991{3000000 + serial:0000000}";
            guest.Email = $"guest{serial}@hotel.local";

            var issuedDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-5).AddDays(-(serial % 365)));
            var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-19 - (serial % 45)).AddDays(serial % 280));
            var authority = authorities[serial % authorities.Length];
            var address = $"г. Новосибирск, ул. {streets[serial % streets.Length]}, д. {8 + serial}";

            guest.GuestIdentity ??= new GuestIdentity();
            guest.GuestIdentity.DocType = "Паспорт РФ";
            guest.GuestIdentity.DocNumber = BuildDocNumber(serial);
            guest.GuestIdentity.IssuedBy = authority;
            guest.GuestIdentity.IssuedDate = issuedDate;
            guest.GuestIdentity.BirthDate = birthDate;
            guest.GuestIdentity.Citizenship = serial % 8 == 0 ? "KZ" : "RU";
            guest.GuestIdentity.Address = address;
        }

        for (var i = 0; i < existingGuests.Count; i++)
            ApplyGuestData(existingGuests[i], i + 1);

        var toCreate = Math.Max(0, targetGuests - existingGuests.Count);
        for (var i = 1; i <= toCreate; i++)
        {
            var serial = existingGuests.Count + i;
            var guest = new Guest
            {
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-serial)
            };

            ApplyGuestData(guest, serial);
            _dbContext.Guests.Add(guest);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedReservationsAndStaysAsync(
        IReadOnlyDictionary<string, RoomType> roomTypeMap,
        CancellationToken cancellationToken)
    {
        const int targetReservations = 120;
        const int targetStays = 96;

        var adminUsers = await _dbContext.AppUsers
            .Where(x => x.RoleCode == Roles.Admin && x.IsActive)
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);

        if (adminUsers.Count == 0)
            return;

        var rooms = await _dbContext.Rooms
            .Include(x => x.RoomType)
            .Where(x => x.IsActive)
            .OrderBy(x => x.RoomNumber)
            .ToListAsync(cancellationToken);

        var mealPlans = await _dbContext.MealPlans.OrderBy(x => x.MealPlanId).ToListAsync(cancellationToken);
        var guests = await _dbContext.Guests.OrderBy(x => x.GuestId).ToListAsync(cancellationToken);

        if (rooms.Count == 0 || guests.Count == 0)
            return;

        var reservationCount = await _dbContext.Reservations.CountAsync(cancellationToken);
        if (reservationCount < targetReservations)
        {
            var reservationsToCreate = targetReservations - reservationCount;
            var newReservations = new List<Reservation>(reservationsToCreate);
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            for (var i = 1; i <= reservationsToCreate; i++)
            {
                var serial = reservationCount + i;
                var status = (serial % 6) switch
                {
                    0 => ReservationStatuses.Created,
                    1 => ReservationStatuses.Confirmed,
                    2 => ReservationStatuses.Cancelled,
                    3 => ReservationStatuses.Completed,
                    4 => ReservationStatuses.CheckedIn,
                    _ => ReservationStatuses.NoShow
                };

                var room = rooms[serial % rooms.Count];
                var nights = 1 + (serial % 5);
                var checkinShift = status switch
                {
                    ReservationStatuses.Created => 2 + (serial % 60),
                    ReservationStatuses.Confirmed => 1 + (serial % 35),
                    ReservationStatuses.CheckedIn => -1,
                    ReservationStatuses.Completed => -10 - (serial % 40),
                    ReservationStatuses.NoShow => -2 - (serial % 10),
                    _ => -5 - (serial % 30)
                };

                var checkin = today.AddDays(checkinShift);
                var checkout = checkin.AddDays(nights);
                var adults = 1 + (serial % Math.Max(1, room.RoomType.Capacity));
                var children = room.RoomType.Capacity > 2 ? serial % 2 : 0;
                var pricePerNight = room.RoomType.BasePrice;
                var totalPrice = pricePerNight * nights;
                var prepayment = status == ReservationStatuses.Cancelled ? totalPrice * 0.15m : totalPrice * 0.35m;
                var mealPlan = mealPlans.Count == 0 ? null : mealPlans[serial % mealPlans.Count];

                var reservation = new Reservation
                {
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-Math.Max(1, Math.Abs(checkinShift) + 3)),
                    CreatedByUserId = adminUsers[serial % adminUsers.Count].UserId,
                    Status = status,
                    Source = (serial % 5) switch
                    {
                        0 => "Reception",
                        1 => "Website",
                        2 => "Phone",
                        3 => "OTA",
                        _ => "РЎР°Р№С‚"
                    },
                    CustomerName = $"РљР»РёРµРЅС‚ #{serial}",
                    CustomerPhone = $"+7998{2000000 + serial:0000000}",
                    Comment = status switch
                    {
                        ReservationStatuses.Created => "РќРѕРІР°СЏ Р±СЂРѕРЅСЊ, РѕР¶РёРґР°РµС‚ РїРѕРґС‚РІРµСЂР¶РґРµРЅРёСЏ",
                        ReservationStatuses.Confirmed => "Р‘СЂРѕРЅСЊ РїРѕРґС‚РІРµСЂР¶РґРµРЅР°",
                        ReservationStatuses.CheckedIn => "Р“РѕСЃС‚СЊ Р·Р°СЃРµР»С‘РЅ РїРѕ Р±СЂРѕРЅРёСЂРѕРІР°РЅРёСЋ",
                        ReservationStatuses.Completed => "РџСЂРѕР¶РёРІР°РЅРёРµ Р·Р°РІРµСЂС€РµРЅРѕ, СЂР°СЃС‡С‘С‚ РІС‹РїРѕР»РЅРµРЅ",
                        ReservationStatuses.NoShow => "Р“РѕСЃС‚СЊ РЅРµ РїСЂРёР±С‹Р» РІ РґР°С‚Сѓ Р·Р°РµР·РґР°",
                        _ => "Р‘СЂРѕРЅСЊ РѕС‚РјРµРЅРµРЅР° РїРѕ Р·Р°РїСЂРѕСЃСѓ РєР»РёРµРЅС‚Р°"
                    },
                    PlannedCheckin = checkin,
                    PlannedCheckout = checkout,
                    Adults = adults,
                    Children = children,
                    TotalPrice = totalPrice,
                    Prepayment = Math.Round(prepayment, 2),
                    MealPlanId = mealPlan?.MealPlanId
                };

                newReservations.Add(reservation);
            }

            _dbContext.Reservations.AddRange(newReservations);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var reservationRooms = new List<ReservationRoom>(newReservations.Count);
            foreach (var reservation in newReservations)
            {
                var roomIndex = reservation.ReservationId % rooms.Count;
                var room = rooms[roomIndex];

                reservationRooms.Add(new ReservationRoom
                {
                    ReservationId = reservation.ReservationId,
                    RoomId = room.RoomId,
                    PricePerNight = room.RoomType.BasePrice + (reservation.ReservationId % 3) * 150m
                });

                if (roomTypeMap.TryGetValue("Family", out var familyType)
                    && reservation.Adults + reservation.Children >= 3
                    && reservation.ReservationId % 5 == 0)
                {
                    var extraRoom = rooms.FirstOrDefault(x => x.RoomTypeId == familyType.RoomTypeId) ?? room;
                    reservationRooms.Add(new ReservationRoom
                    {
                        ReservationId = reservation.ReservationId,
                        RoomId = extraRoom.RoomId,
                        PricePerNight = extraRoom.RoomType.BasePrice
                    });
                }
            }

            _dbContext.ReservationRooms.AddRange(reservationRooms);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var stayCount = await _dbContext.Stays.CountAsync(cancellationToken);
        if (stayCount >= targetStays)
            return;

        var reservations = await _dbContext.Reservations
            .Include(x => x.ReservationRooms)
            .OrderBy(x => x.ReservationId)
            .ToListAsync(cancellationToken);

        var staysToCreate = targetStays - stayCount;
        var todayForStays = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var newStays = new List<Stay>(staysToCreate);

        for (var i = 1; i <= staysToCreate; i++)
        {
            var serial = stayCount + i;
            var status = (serial % 5) switch
            {
                0 => StayStatuses.Active,
                1 => StayStatuses.CheckedOut,
                2 => StayStatuses.Planned,
                3 => StayStatuses.Cancelled,
                _ => StayStatuses.CheckedOut
            };

            var reservation = reservations[(serial * 3) % reservations.Count];
            var roomId = reservation.ReservationRooms.FirstOrDefault()?.RoomId
                         ?? rooms[(serial * 5) % rooms.Count].RoomId;

            var nights = 1 + (serial % 6);
            var plannedCheckin = status switch
            {
                StayStatuses.Active => todayForStays.AddDays(-(serial % 3)),
                StayStatuses.CheckedOut => todayForStays.AddDays(-5 - (serial % 20)),
                StayStatuses.Planned => todayForStays.AddDays(1 + (serial % 30)),
                StayStatuses.Cancelled => todayForStays.AddDays(serial % 15),
                _ => todayForStays
            };
            var plannedCheckout = plannedCheckin.AddDays(nights);

            DateTimeOffset? actualCheckin = null;
            DateTimeOffset? actualCheckout = null;
            if (status == StayStatuses.Active || status == StayStatuses.CheckedOut)
            {
                actualCheckin = new DateTimeOffset(
                    plannedCheckin.ToDateTime(new TimeOnly(14, 0), DateTimeKind.Utc));
            }

            if (status == StayStatuses.CheckedOut)
            {
                actualCheckout = new DateTimeOffset(
                    plannedCheckout.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc));
            }

            var mealPlan = mealPlans.Count == 0 ? null : mealPlans[(serial + 1) % mealPlans.Count];

            newStays.Add(new Stay
            {
                ReservationId = reservation.ReservationId,
                RoomId = roomId,
                Status = status,
                ActualCheckin = actualCheckin,
                ActualCheckout = actualCheckout,
                PlannedCheckin = plannedCheckin,
                PlannedCheckout = plannedCheckout,
                MealPlanId = mealPlan?.MealPlanId,
                CreatedByUserId = adminUsers[serial % adminUsers.Count].UserId,
                Comment = status switch
                {
                    StayStatuses.Active => "РРґС‘С‚ РїСЂРѕР¶РёРІР°РЅРёРµ, РѕРїР»Р°С‚Р° С‡Р°СЃС‚РёС‡РЅРѕ Р·Р°РєСЂС‹С‚Р°",
                    StayStatuses.Planned => "РћР¶РёРґР°РµС‚СЃСЏ Р·Р°РµР·Рґ",
                    StayStatuses.CheckedOut => "Р“РѕСЃС‚СЊ РІС‹РµС…Р°Р», РЅРѕРјРµСЂ РЅР°РїСЂР°РІР»РµРЅ РЅР° СѓР±РѕСЂРєСѓ",
                    _ => "Р—Р°СЃРµР»РµРЅРёРµ РѕС‚РјРµРЅРµРЅРѕ РґРѕ Р·Р°РµР·РґР°"
                }
            });
        }

        _dbContext.Stays.AddRange(newStays);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var stayGuests = new List<StayGuest>(newStays.Count * 2);
        var stayOperations = new List<StayOperation>(newStays.Count * 3);

        foreach (var stay in newStays)
        {
            var guestA = guests[(stay.StayId * 7) % guests.Count];
            var guestB = guests[(stay.StayId * 11 + 3) % guests.Count];

            stayGuests.Add(new StayGuest
            {
                StayId = stay.StayId,
                GuestId = guestA.GuestId,
                IsMain = true
            });

            if (guestB.GuestId != guestA.GuestId && stay.StayId % 3 == 0)
            {
                stayGuests.Add(new StayGuest
                {
                    StayId = stay.StayId,
                    GuestId = guestB.GuestId,
                    IsMain = false
                });
            }

            var userId = stay.CreatedByUserId ?? adminUsers[0].UserId;
            if (stay.Status is StayStatuses.Active or StayStatuses.CheckedOut)
            {
                stayOperations.Add(new StayOperation
                {
                    StayId = stay.StayId,
                    UserId = userId,
                    OperationType = "CheckIn",
                    OccurredAt = stay.ActualCheckin ?? DateTimeOffset.UtcNow.AddHours(-3),
                    Comment = "Р РµРіРёСЃС‚СЂР°С†РёСЏ Р·Р°РµР·РґР° Рё РІС‹РґР°С‡Р° РєР»СЋС‡Р°"
                });
            }

            if (stay.Status == StayStatuses.CheckedOut)
            {
                stayOperations.Add(new StayOperation
                {
                    StayId = stay.StayId,
                    UserId = userId,
                    OperationType = "RoomService",
                    OccurredAt = (stay.ActualCheckin ?? DateTimeOffset.UtcNow).AddHours(8),
                    Comment = "РџСЂРѕРјРµР¶СѓС‚РѕС‡РЅР°СЏ СѓР±РѕСЂРєР° Рё РїРѕРїРѕР»РЅРµРЅРёРµ РїСЂРёРЅР°РґР»РµР¶РЅРѕСЃС‚РµР№"
                });

                stayOperations.Add(new StayOperation
                {
                    StayId = stay.StayId,
                    UserId = userId,
                    OperationType = "CheckOut",
                    OccurredAt = stay.ActualCheckout ?? DateTimeOffset.UtcNow,
                    Comment = "РћС„РѕСЂРјР»РµРЅРёРµ РІС‹РµР·РґР° Рё Р·Р°РєСЂС‹С‚РёРµ СЃС‡С‘С‚Р°"
                });
            }

            if (stay.Status == StayStatuses.Cancelled)
            {
                stayOperations.Add(new StayOperation
                {
                    StayId = stay.StayId,
                    UserId = userId,
                    OperationType = "CancelStay",
                    OccurredAt = DateTimeOffset.UtcNow.AddDays(-1),
                    Comment = "РћС‚РјРµРЅР° РїСЂРѕР¶РёРІР°РЅРёСЏ РїРѕ Р·Р°РїСЂРѕСЃСѓ РєР»РёРµРЅС‚Р°"
                });
            }
        }

        _dbContext.StayGuests.AddRange(stayGuests);
        _dbContext.StayOperations.AddRange(stayOperations);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await EnsureExistingStaysAreRichAsync(guests, adminUsers, cancellationToken);
    }

    private async Task EnsureExistingStaysAreRichAsync(
        IReadOnlyList<Guest> guests,
        IReadOnlyList<AppUser> admins,
        CancellationToken cancellationToken)
    {
        var stays = await _dbContext.Stays
            .Include(x => x.StayGuests)
            .Include(x => x.Operations)
            .OrderBy(x => x.StayId)
            .ToListAsync(cancellationToken);

        foreach (var stay in stays)
        {
            var existingGuestIds = stay.StayGuests.Select(x => x.GuestId).ToHashSet();

            if (existingGuestIds.Count == 0)
            {
                var mainGuest = guests[stay.StayId % guests.Count];
                _dbContext.StayGuests.Add(new StayGuest
                {
                    StayId = stay.StayId,
                    GuestId = mainGuest.GuestId,
                    IsMain = true
                });
                existingGuestIds.Add(mainGuest.GuestId);
            }

            if (stay.Status != StayStatuses.Cancelled && existingGuestIds.Count == 1 && stay.StayId % 2 == 0)
            {
                var secondaryGuest = guests[(stay.StayId * 7) % guests.Count];
                if (!existingGuestIds.Contains(secondaryGuest.GuestId))
                {
                    _dbContext.StayGuests.Add(new StayGuest
                    {
                        StayId = stay.StayId,
                        GuestId = secondaryGuest.GuestId,
                        IsMain = false
                    });
                }
            }

            var existingOperationTypes = stay.Operations.Select(x => x.OperationType).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var userId = stay.CreatedByUserId ?? admins[stay.StayId % admins.Count].UserId;

            if (stay.Status is StayStatuses.Active or StayStatuses.CheckedOut)
            {
                if (!existingOperationTypes.Contains("CheckIn"))
                {
                    _dbContext.StayOperations.Add(new StayOperation
                    {
                        StayId = stay.StayId,
                        UserId = userId,
                        OperationType = "CheckIn",
                        OccurredAt = stay.ActualCheckin ?? DateTimeOffset.UtcNow.AddHours(-12),
                        Comment = "Р РµРіРёСЃС‚СЂР°С†РёСЏ РіРѕСЃС‚СЏ РЅР° СЃС‚РѕР№РєРµ"
                    });
                }

                if (!existingOperationTypes.Contains("RoomService"))
                {
                    _dbContext.StayOperations.Add(new StayOperation
                    {
                        StayId = stay.StayId,
                        UserId = userId,
                        OperationType = "RoomService",
                        OccurredAt = (stay.ActualCheckin ?? DateTimeOffset.UtcNow).AddHours(10),
                        Comment = "Р’С‹РїРѕР»РЅРµРЅР° СѓР±РѕСЂРєР° Рё РѕР±СЃР»СѓР¶РёРІР°РЅРёРµ РЅРѕРјРµСЂР°"
                    });
                }
            }

            if (stay.Status == StayStatuses.CheckedOut && !existingOperationTypes.Contains("CheckOut"))
            {
                _dbContext.StayOperations.Add(new StayOperation
                {
                    StayId = stay.StayId,
                    UserId = userId,
                    OperationType = "CheckOut",
                    OccurredAt = stay.ActualCheckout ?? DateTimeOffset.UtcNow,
                    Comment = "РћС„РѕСЂРјР»РµРЅ РІС‹РµР·Рґ Рё Р·Р°РєСЂС‹С‚С‹ СЂР°СЃС‡С‘С‚С‹"
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedShiftsAsync(CancellationToken cancellationToken)
    {
        const int targetClosedShifts = 20;

        var admins = await _dbContext.AppUsers
            .Where(x => x.RoleCode == Roles.Admin && x.IsActive)
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);

        if (admins.Count == 0)
            return;

        var staleOpenShifts = await _dbContext.WorkShifts
            .Where(x => x.Status == "Open" && x.StartedAt < DateTimeOffset.UtcNow.AddHours(-16))
            .ToListAsync(cancellationToken);

        if (staleOpenShifts.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var open in staleOpenShifts)
            {
                open.Status = "Closed";
                open.EndedAt = now;
                open.Comment = string.IsNullOrWhiteSpace(open.Comment)
                    ? "РђРІС‚РѕРјР°С‚РёС‡РµСЃРєРё Р·Р°РєСЂС‹С‚Р° Р·Р°РІРёСЃС€Р°СЏ СЃРјРµРЅР°"
                    : $"{open.Comment} | РђРІС‚РѕРјР°С‚РёС‡РµСЃРєРё Р·Р°РєСЂС‹С‚Р° Р·Р°РІРёСЃС€Р°СЏ СЃРјРµРЅР°";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var closedCount = await _dbContext.WorkShifts
            .CountAsync(x => x.Status == "Closed", cancellationToken);

        if (closedCount >= targetClosedShifts)
            return;

        var toCreate = targetClosedShifts - closedCount;

        for (var i = 1; i <= toCreate; i++)
        {
            var serial = closedCount + i;
            var admin = admins[serial % admins.Count];
            var startedAt = DateTimeOffset.UtcNow.AddDays(-serial).AddHours(-(8 + (serial % 3)));
            var endedAt = startedAt.AddHours(8 + (serial % 2));

            _dbContext.WorkShifts.Add(new WorkShift
            {
                UserId = admin.UserId,
                StartedAt = startedAt,
                EndedAt = endedAt,
                Status = "Closed",
                Comment = "РџР»Р°РЅРѕРІР°СЏ Р·Р°РєСЂС‹С‚Р°СЏ СЃРјРµРЅР°"
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDocumentHistoryAsync(CancellationToken cancellationToken)
    {
        try
        {
            const int targetDocuments = 60;

            var currentCount = await _dbContext.GeneratedDocuments.CountAsync(cancellationToken);
            if (currentCount >= targetDocuments)
                return;

            var admins = await _dbContext.AppUsers
                .Where(x => x.RoleCode == Roles.Admin)
                .OrderBy(x => x.UserId)
                .ToListAsync(cancellationToken);

            var reservations = await _dbContext.Reservations
                .OrderByDescending(x => x.ReservationId)
                .Take(80)
                .ToListAsync(cancellationToken);

            var stays = await _dbContext.Stays
                .OrderByDescending(x => x.StayId)
                .Take(80)
                .ToListAsync(cancellationToken);

            if (reservations.Count == 0 || stays.Count == 0)
                return;

            var toCreate = targetDocuments - currentCount;

            for (var i = 1; i <= toCreate; i++)
            {
                var serial = currentCount + i;
                var admin = admins.Count == 0 ? null : admins[serial % admins.Count];

                if (serial % 3 == 0)
                {
                    var stay = stays[serial % stays.Count];
                    _dbContext.GeneratedDocuments.Add(new GeneratedDocument
                    {
                        DocumentType = "CheckoutAct",
                        FileName = $"checkout-act-{stay.StayId}-{serial}.pdf",
                        EntityType = "Stay",
                        EntityId = stay.StayId,
                        GeneratedAt = DateTimeOffset.UtcNow.AddHours(-serial),
                        GeneratedByUserId = admin?.UserId
                    });
                }
                else if (serial % 2 == 0)
                {
                    var reservation = reservations[serial % reservations.Count];
                    _dbContext.GeneratedDocuments.Add(new GeneratedDocument
                    {
                        DocumentType = "Invoice",
                        FileName = $"invoice-{reservation.ReservationId}-{serial}.pdf",
                        EntityType = "Reservation",
                        EntityId = reservation.ReservationId,
                        GeneratedAt = DateTimeOffset.UtcNow.AddHours(-serial),
                        GeneratedByUserId = admin?.UserId
                    });
                }
                else
                {
                    var reservation = reservations[serial % reservations.Count];
                    _dbContext.GeneratedDocuments.Add(new GeneratedDocument
                    {
                        DocumentType = "ServiceContract",
                        FileName = $"service-contract-{reservation.ReservationId}-{serial}.pdf",
                        EntityType = "Reservation",
                        EntityId = reservation.ReservationId,
                        GeneratedAt = DateTimeOffset.UtcNow.AddHours(-serial),
                        GeneratedByUserId = admin?.UserId
                    });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Skipping generated_document seed (table may be absent before migration).");
        }
    }
}
