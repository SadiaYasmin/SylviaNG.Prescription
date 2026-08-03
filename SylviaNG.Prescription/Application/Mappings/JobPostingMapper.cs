using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    /// <summary>
    /// Manual mapping methods for JobPosting and JobApplication entities.
    /// Follow this pattern for all new feature mappings.
    /// </summary>
    public static class JobPostingMapper
    {
        // ── JobPosting Mappings ──────────────────────────────────────────

        public static JobPosting ToEntity(this JobPostingCreateRequest request)
        {
            return new JobPosting
            {
                SiteId = request.SiteId,
                DepartmentId = request.DepartmentId,
                DesignationId = request.DesignationId,
                Title = request.Title,
                Description = request.Description,
                Requirements = request.Requirements,
                NumberOfPositions = request.NumberOfPositions,
                EmploymentType = request.EmploymentType,
                MinSalary = request.MinSalary,
                MaxSalary = request.MaxSalary,
                PostingDate = request.PostingDate,
                ClosingDate = request.ClosingDate,
                IsActive = true
            };
        }

        public static void ApplyUpdate(this JobPosting entity, JobPostingUpdateRequest request)
        {
            if (request.DepartmentId.HasValue) entity.DepartmentId = request.DepartmentId;
            if (request.DesignationId.HasValue) entity.DesignationId = request.DesignationId;
            if (request.Title != null) entity.Title = request.Title;
            if (request.Description != null) entity.Description = request.Description;
            if (request.Requirements != null) entity.Requirements = request.Requirements;
            if (request.NumberOfPositions.HasValue) entity.NumberOfPositions = request.NumberOfPositions.Value;
            if (request.EmploymentType.HasValue) entity.EmploymentType = request.EmploymentType.Value;
            if (request.Status.HasValue) entity.Status = request.Status.Value;
            if (request.MinSalary.HasValue) entity.MinSalary = request.MinSalary;
            if (request.MaxSalary.HasValue) entity.MaxSalary = request.MaxSalary;
            if (request.PostingDate.HasValue) entity.PostingDate = request.PostingDate;
            if (request.ClosingDate.HasValue) entity.ClosingDate = request.ClosingDate;
            if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        }

        public static JobPostingResponse ToResponse(this JobPosting entity)
        {
            return new JobPostingResponse
            {
                JobPostingId = entity.JobPostingId,
                SiteId = entity.SiteId,
                DepartmentId = entity.DepartmentId,
                DesignationId = entity.DesignationId,
                Title = entity.Title,
                Description = entity.Description,
                Requirements = entity.Requirements,
                NumberOfPositions = entity.NumberOfPositions,
                EmploymentType = entity.EmploymentType,
                Status = entity.Status,
                MinSalary = entity.MinSalary,
                MaxSalary = entity.MaxSalary,
                PostingDate = entity.PostingDate,
                ClosingDate = entity.ClosingDate,
                IsActive = entity.IsActive,
                TotalApplications = entity.Applications?.Count ?? 0
            };
        }

        public static JobPostingLookupResponse ToLookupResponse(this JobPosting entity)
        {
            return new JobPostingLookupResponse
            {
                JobPostingId = entity.JobPostingId,
                Title = entity.Title
            };
        }

        // ── JobApplication Mappings ──────────────────────────────────────

        public static JobApplication ToEntity(this JobApplicationCreateRequest request)
        {
            return new JobApplication
            {
                JobPostingId = request.JobPostingId,
                CandidateName = request.CandidateName,
                CandidateEmail = request.CandidateEmail,
                CandidatePhone = request.CandidatePhone,
                ResumeUrl = request.ResumeUrl,
                CoverLetter = request.CoverLetter,
                IsActive = true
            };
        }

        public static void ApplyUpdate(this JobApplication entity, JobApplicationUpdateRequest request)
        {
            if (request.ApplicationStatus.HasValue) entity.ApplicationStatus = request.ApplicationStatus.Value;
            if (request.CandidatePhone != null) entity.CandidatePhone = request.CandidatePhone;
            if (request.ResumeUrl != null) entity.ResumeUrl = request.ResumeUrl;
            if (request.CoverLetter != null) entity.CoverLetter = request.CoverLetter;
            if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        }

        public static JobApplicationResponse ToResponse(this JobApplication entity)
        {
            return new JobApplicationResponse
            {
                JobApplicationId = entity.JobApplicationId,
                JobPostingId = entity.JobPostingId,
                JobPostingTitle = entity.JobPosting?.Title,
                CandidateName = entity.CandidateName,
                CandidateEmail = entity.CandidateEmail,
                CandidatePhone = entity.CandidatePhone,
                ResumeUrl = entity.ResumeUrl,
                ApplicationStatus = entity.ApplicationStatus,
                AppliedDate = entity.AppliedDate,
                IsActive = entity.IsActive
            };
        }
    }
}
