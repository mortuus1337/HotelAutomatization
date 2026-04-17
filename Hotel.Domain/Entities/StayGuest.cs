using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Domain.Entities;

public class StayGuest
{

    public int StayId { get; set; }


    public int GuestId { get; set; }

    public bool IsMain { get; set; }

    public Stay Stay { get; set; } = null!;

    public Guest Guest { get; set; } = null!;
}