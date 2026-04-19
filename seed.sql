USE HCARS_DB;
GO

INSERT INTO Specializations (Name, Description) VALUES
('Cardiology', 'Heart and cardiovascular system'),
('Dermatology', 'Skin, hair and nails'),
('Neurology', 'Brain and nervous system'),
('Pediatrics', 'Medical care for children'),
('Orthopedics', 'Bones, joints and muscles');
GO


INSERT INTO DoctorSpecializations (DoctorId, SpecializationId) VALUES
(1, 1), -- Doctor 1 -> Cardiology
(1, 3), -- Doctor 1 -> Neurology
(2, 2); -- Doctor 2 -> Dermatology
GO

INSERT INTO DoctorSchedules
(DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes)
VALUES
(1, 1, '08:00', '16:00', 30), -- Doctor 1 Monday
(1, 2, '08:00', '16:00', 30), -- Doctor 1 Tuesday
(1, 3, '08:00', '16:00', 30), -- Doctor 1 Wednesday
(2, 0, '09:00', '17:00', 30), -- Doctor 2 Sunday
(2, 1, '09:00', '17:00', 30), -- Doctor 2 Monday
(2, 4, '09:00', '17:00', 30); -- Doctor 2 Thursday
GO

INSERT INTO Appointments
(PatientId, DoctorId, StatusId, AppointmentDate,
StartTime, EndTime, Notes, CreatedAt)
VALUES
(1, 1, 2, DATEADD(day, 1, GETUTCDATE()),
'09:00', '09:30', 'Regular checkup', GETUTCDATE()),

(1, 1, 1, DATEADD(day, 3, GETUTCDATE()),
'10:00', '10:30', 'Follow up visit', GETUTCDATE()),

(2, 2, 2, DATEADD(day, 2, GETUTCDATE()),
'10:00', '10:30', 'Skin consultation', GETUTCDATE()),

(2, 1, 5, DATEADD(day, -7, GETUTCDATE()),
'11:00', '11:30', 'Neurological assessment', GETUTCDATE());
GO


INSERT INTO VisitRecords
(AppointmentId, DoctorNotes, Diagnosis, Treatment, CreatedAt)
VALUES
(4, 'Patient presented with mild headaches',
'Tension headache', 'Rest and hydration recommended',
GETUTCDATE());
GO

INSERT INTO Prescriptions
(VisitRecordId, MedicationName, Dosage,
Frequency, DurationDays, Instructions, CreatedAt)
VALUES
(1, 'Paracetamol', '500mg',
'Twice daily', 5, 'Take after meals', GETUTCDATE());
GO