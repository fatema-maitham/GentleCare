USE HCARS_DB;
GO

INSERT INTO Specializations (Name, Description) VALUES
('Cardiology', 'Heart and cardiovascular system'),
('Dermatology', 'Skin, hair and nails'),
('Neurology', 'Brain and nervous system'),
('Pediatrics', 'Medical care for children'),
('Orthopedics', 'Bones, joints and muscles');
GO

-- Test passwords are: Test@1234

INSERT INTO AspNetUsers 
(Id, FullName, UserName, NormalizedUserName, Email, NormalizedEmail,
EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
IsActive, CreatedAt, ProfilePicture)
VALUES
('cm-001', 'Dr. Fatima Al-Mansoori', 'manager@hcars.com', 
'MANAGER@HCARS.COM', 'manager@hcars.com', 'MANAGER@HCARS.COM',
1, 'AQAAAAIAAYagAAAAEH+test+hash+here', 
NEWID(), NEWID(), 1, GETUTCDATE(), NULL),


('doc-001', 'Dr. Ahmed Al-Rashidi', 'doctor1@hcars.com',
'DOCTOR1@HCARS.COM', 'doctor1@hcars.com', 'DOCTOR1@HCARS.COM',
1, 'AQAAAAIAAYagAAAAEH+test+hash+here',
NEWID(), NEWID(), 1, GETUTCDATE(), NULL),


('doc-002', 'Dr. Sara Al-Khalifa', 'doctor2@hcars.com',
'DOCTOR2@HCARS.COM', 'doctor2@hcars.com', 'DOCTOR2@HCARS.COM',
1, 'AQAAAAIAAYagAAAAEH+test+hash+here',
NEWID(), NEWID(), 1, GETUTCDATE(), NULL),


('rec-001', 'Mariam Al-Dosari', 'receptionist@hcars.com',
'RECEPTIONIST@HCARS.COM', 'receptionist@hcars.com', 
'RECEPTIONIST@HCARS.COM',
1, 'AQAAAAIAAYagAAAAEH+test+hash+here',
NEWID(), NEWID(), 1, GETUTCDATE(), NULL),


('pat-001', 'Ali Hassan', 'patient1@hcars.com',
'PATIENT1@HCARS.COM', 'patient1@hcars.com', 'PATIENT1@HCARS.COM',
1, 'AQAAAAIAAYagAAAAEH+test+hash+here',
NEWID(), NEWID(), 1, GETUTCDATE(), NULL),


('pat-002', 'Noor Al-Zayani', 'patient2@hcars.com',
'PATIENT2@HCARS.COM', 'patient2@hcars.com', 'PATIENT2@HCARS.COM',
1, 'AQAAAAIAAYagAAAAEH+test+hash+here',
NEWID(), NEWID(), 1, GETUTCDATE(), NULL);
GO


INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES
('role-cm', 'ClinicManager', 'CLINICMANAGER', NEWID()),
('role-doc', 'Doctor', 'DOCTOR', NEWID()),
('role-rec', 'Receptionist', 'RECEPTIONIST', NEWID()),
('role-pat', 'Patient', 'PATIENT', NEWID());
GO

INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES
('cm-001', 'role-cm'),
('doc-001', 'role-doc'),
('doc-002', 'role-doc'),
('rec-001', 'role-rec'),
('pat-001', 'role-pat'),
('pat-002', 'role-pat');
GO


INSERT INTO Doctors (UserId, LicenseNumber, Bio) VALUES
('doc-001', 'LIC-2024-001', 
    'Cardiologist with 10 years experience'),
('doc-002', 'LIC-2024-002', 
    'Dermatologist specializing in skin conditions');
GO

INSERT INTO DoctorSpecializations (DoctorId, SpecializationId) VALUES
(1, 1), 
(1, 3), 
(2, 2);
GO

INSERT INTO DoctorSchedules 
(DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes)
VALUES
(1, 1, '08:00', '16:00', 30),
(1, 2, '08:00', '16:00', 30),
(1, 3, '08:00', '16:00', 30), 
(2, 0, '09:00', '17:00', 30), 
(2, 1, '09:00', '17:00', 30),
(2, 4, '09:00', '17:00', 30); 
GO


INSERT INTO Patients 
(UserId, CPRNumber, ReferenceNumber, DateOfBirth, 
BloodType, Address, EmergencyContactName, EmergencyContactPhone)
VALUES
('pat-001', '900112345', 'REF-2024-001', '1990-01-12',
'A+', 'Manama, Bahrain', 'Hassan Ali', '+97333112233'),
('pat-002', '950256789', 'REF-2024-002', '1995-02-20',
'B+', 'Riffa, Bahrain', 'Khalid Al-Zayani', '+97333445566');
GO

INSERT INTO Appointments
(PatientId, DoctorId, SpecializationId, AppointmentDate,
StartTime, EndTime, Status, Notes, CreatedAt)
VALUES
(1, 1, 1, DATEADD(day, 1, GETUTCDATE()),
'09:00', '09:30', 'Confirmed', 
'Regular checkup', GETUTCDATE()),

(1, 1, 1, DATEADD(day, 3, GETUTCDATE()),
'10:00', '10:30', 'Requested',
'Follow up visit', GETUTCDATE()),

(2, 2, 2, DATEADD(day, 2, GETUTCDATE()),
'10:00', '10:30', 'Confirmed',
'Skin consultation', GETUTCDATE()),

(2, 1, 3, DATEADD(day, -7, GETUTCDATE()),
'11:00', '11:30', 'Completed',
'Neurological assessment', GETUTCDATE());
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
Frequency, DurationDays, Instructions)
VALUES
(1, 'Paracetamol', '500mg', 
'Twice daily', 5, 'Take after meals');
GO