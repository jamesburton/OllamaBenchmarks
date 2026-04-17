using System;
using System.Collections.Generic;
using System.Linq;

public record Appointment(int Id, string PatientName, string DoctorId, DateTime ScheduledAt, bool IsCancelled);

public class AppointmentScheduler
{
    private readonly List<Appointment> _appointments = new List<Appointment>();

    public void Schedule(Appointment appointment)
    {
        _appointments.Add(appointment);
    }

    public void Cancel(int appointmentId)
    {
        var index = _appointments.FindIndex(a => a.Id == appointmentId);
        if (index != -1)
        {
            // Use 'with' expression to create a new instance with IsCancelled set to true
            _appointments[index] = _appointments[index] with { IsCancelled = true };
        }
    }

    public List<Appointment> GetForDoctor(string doctorId)
    {
        return _appointments
            .Where(a => a.DoctorId == doctorId && !a.IsCancelled)
            .ToList();
    }

    public int UpcomingCount(DateTime now)
    {
        return _appointments.Count(a => !a.IsCancelled && a.ScheduledAt > now);
    }
}