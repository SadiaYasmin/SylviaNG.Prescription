using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Infrastructure.Repositories
{
    public class VerificationCodeRepository : Repository<VerificationCode>, IVerificationCodeRepository
    {
        public VerificationCodeRepository(ApplicationDBContext dbContext) : base(dbContext) { }

        public async Task<VerificationCode?> GetLatestActiveAsync(string email, VerificationPurposeEnum purpose)
        {
            return await _dbSet
                .Where(v => v.Email.ToLower() == email.ToLower() && v.Purpose == purpose && v.ConsumedAt == null)
                .OrderByDescending(v => v.IssuedAt)
                .FirstOrDefaultAsync();
        }
    }
}
