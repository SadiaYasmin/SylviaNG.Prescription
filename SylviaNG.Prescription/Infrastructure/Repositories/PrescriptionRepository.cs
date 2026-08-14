using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Infrastructure.Repositories
{
    public class PrescriptionRepository : Repository<PrescriptionRecord>, IPrescriptionRepository
    {
        public PrescriptionRepository(ApplicationDBContext dbContext) : base(dbContext) { }
    }
}
