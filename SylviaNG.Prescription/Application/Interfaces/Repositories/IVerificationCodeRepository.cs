using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Interfaces.Repositories
{
    public interface IVerificationCodeRepository : IRepository<VerificationCode>
    {
        /// <summary>Most recent, not-yet-consumed code for this email+purpose (used for the resend cooldown check and for verification).</summary>
        Task<VerificationCode?> GetLatestActiveAsync(string email, VerificationPurposeEnum purpose);
    }
}
