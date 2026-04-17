using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Constants
{
    public static class ReservationStatuses
    {
        public const string Created = "Created";
        public const string Confirmed = "Confirmed";
        public const string Cancelled = "Cancelled";
        public const string CheckedIn = "CheckedIn";
        public const string Completed = "Completed";
        public const string NoShow = "NoShow";
    }
}
