using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Departments.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
        {
            _departmentRepository = departmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<long> CreateAsync(DepartmentCreateRequest request)
        {
            if (await _departmentRepository.ExistsByNameAsync(request.Name))
                throw new DuplicateException("Department", "Name", request.Name);

            var entity = request.ToEntity();
            await _departmentRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity.DepartmentId;
        }

        public async Task UpdateAsync(long id, DepartmentUpdateRequest request)
        {
            var entity = await _departmentRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Department", id);

            if (await _departmentRepository.ExistsByNameAsync(request.Name, id))
                throw new DuplicateException("Department", "Name", request.Name);

            entity.ApplyUpdate(request);
            _departmentRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateAsync(long id)
        {
            var entity = await _departmentRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Department", id);

            entity.IsActive = false;
            _departmentRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<DepartmentResponse>> GetAllAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            // Newest-first by DepartmentId, not Audit.CreatedAt — CreatedAt is never actually
            // populated anywhere in this codebase (no SaveChanges interceptor sets it), so it's
            // always null; PK-descending is the same reliable proxy every other list here uses
            // (Doctors, Staff, Patients, Consultations).
            return departments
                .OrderByDescending(d => d.DepartmentId)
                .Select(d => d.ToResponse())
                .ToList();
        }
    }
}
