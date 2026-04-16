using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Hotel.Domain.Entities;

public class RoomType
{
    public int RoomTypeId { get; set; }
    

    public string Name { get; set; } = null!;


    public int Capacity { get; set; }


    public decimal BasePrice { get; set; }


    public string? Description { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
