using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Infrastructure.Repositories
{
    public class TemplateRepository : Repository<PrescriptionTemplate>, ITemplateRepository
    {
        public TemplateRepository(ApplicationDBContext dbContext) : base(dbContext) { }
    }
}
