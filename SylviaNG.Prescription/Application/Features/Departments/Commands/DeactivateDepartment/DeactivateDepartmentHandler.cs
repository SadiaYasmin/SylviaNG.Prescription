using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Departments.Commands.DeactivateDepartment
{
    public class DeactivateDepartmentHandler : IRequestHandler<DeactivateDepartmentCommand, Unit>
    {
        private readonly IDepartmentService _service;
        public DeactivateDepartmentHandler(IDepartmentService service) => _service = service;

        public async Task<Unit> Handle(DeactivateDepartmentCommand command, CancellationToken cancellationToken)
        {
            await _service.DeactivateAsync(command.DepartmentId);
            return Unit.Value;
        }
    }
}
