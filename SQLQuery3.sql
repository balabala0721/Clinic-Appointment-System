CREATE VIEW vw_AppointmentReport AS
SELECT
    a.AppointmentId,
    a.AppointmentDate,
    a.StartTime,
    a.Status,
    p.FullName AS PatientName,
    d.FullName AS DoctorName,
    s.Name AS Specialisation
FROM Appointments a
JOIN Patients p ON p.PatientId = a.PatientId
JOIN Doctors d ON d.DoctorId = a.DoctorId
JOIN Specialisations s
    ON s.SpecialisationId = d.SpecialisationId;