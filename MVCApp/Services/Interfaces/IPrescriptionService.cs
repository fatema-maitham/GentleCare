using MVCApp.ViewModels.Doctor;

namespace MVCApp.Services.Interfaces
{
    // Service contract for prescriptions created by the logged-in doctor.
    public interface IPrescriptionService
    {
        Task<List<DoctorPrescriptionViewModel>?> GetDoctorPrescriptionsAsync(string userId);
    }
}
