using System;
using System.Collections.Generic;
using System.Linq;

public record Reservation(string Id, string GuestName, int RoomNumber, DateTime CheckIn, DateTime CheckOut);

public class Hotel
{
    private readonly List<Reservation> _reservations = new();

    public void AddReservation(Reservation reservation)
    {
        if (reservation == null)
        {
            throw new ArgumentNullException(nameof(reservation));
        }

        if (reservation.CheckOut <= reservation.CheckIn)
        {
            throw new ArgumentException("CheckOut must be later than CheckIn.", nameof(reservation));
        }

        if (!IsRoomAvailable(reservation.RoomNumber, reservation.CheckIn, reservation.CheckOut))
        {
            throw new InvalidOperationException($"Room {reservation.RoomNumber} is not available for the specified dates.");
        }

        _reservations.Add(reservation);
    }

    public bool IsRoomAvailable(int roomNumber, DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            return false;
        }

        // A room is available if there are no overlapping reservations.
        // Overlap occurs if: ExistingStart < NewEnd AND ExistingEnd > NewStart
        return !_reservations.Any(r => 
            r.RoomNumber == roomNumber && 
            r.CheckIn < checkOut && 
            r.CheckOut > checkIn);
    }

    public List<Reservation> GetForGuest(string guestName)
    {
        return _reservations
            .Where(r => r.GuestName.Equals(guestName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Reservation> GetCurrentReservations(DateTime now)
    {
        // Active reservations are where 'now' is greater than or equal to CheckIn 
        // and less than CheckOut.
        return _reservations
            .Where(r => now >= r.CheckIn && now < r.CheckOut)
            .ToList();
    }
}