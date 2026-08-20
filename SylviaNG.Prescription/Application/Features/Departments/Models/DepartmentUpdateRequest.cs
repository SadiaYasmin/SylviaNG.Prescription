namespace SylviaNG.Prescription.Application.Features.Departments.Models
{
    public class DepartmentUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
