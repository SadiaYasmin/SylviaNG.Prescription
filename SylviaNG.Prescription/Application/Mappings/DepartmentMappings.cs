using SylviaNG.Prescription.Application.Features.Departments.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class DepartmentMappings
    {
        public static Department ToEntity(this DepartmentCreateRequest request) => new()
        {
            Name = request.Name.Trim(),
            IsActive = true
        };

        public static void ApplyUpdate(this Department entity, DepartmentUpdateRequest request)
        {
            entity.Name = request.Name.Trim();
            entity.IsActive = request.IsActive;
        }

        public static DepartmentResponse ToResponse(this Department entity) => new()
        {
            DepartmentId = entity.DepartmentId,
            Name = entity.Name,
            IsActive = entity.IsActive
        };
    }
}
