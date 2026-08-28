using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Analytics;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetBusiestConsultationHours;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetDoctorLeaderboard;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetExecutiveSummary;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMedicineAnalytics;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyDoctorAnalytics;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyStaffAnalytics;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPatientAnalytics;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPrescriptionVolumeTrend;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Analytics &amp; Reporting Dashboard (Epic M, US-072–079). Hospital-wide views are
    /// Admin-only; the "my/*" endpoints are the Doctor/Staff-scoped personal-stats
    /// equivalents (US-077/078). Per-action <see cref="AuthorizeAttribute"/> (no class-level
    /// attribute) since the controller mixes Admin/Doctor/Staff roles, same reasoning as
    /// <see cref="PatientsController"/>'s doc comment.
    /// </summary>
    [ApiController]
    [Route("prescription/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("medicines")]
        public async Task<ActionResult<MedicineAnalyticsResponse>> GetMedicineAnalytics()
        {
            var result = await _mediator.Send(new GetMedicineAnalyticsQuery());
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("doctors/leaderboard")]
        public async Task<ActionResult<List<DoctorLeaderboardEntry>>> GetDoctorLeaderboard()
        {
            var result = await _mediator.Send(new GetDoctorLeaderboardQuery());
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("doctors/busiest-hours")]
        public async Task<ActionResult<BusiestConsultationHoursResponse>> GetBusiestConsultationHours()
        {
            var result = await _mediator.Send(new GetBusiestConsultationHoursQuery());
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("prescription-trend")]
        public async Task<ActionResult<PrescriptionVolumeTrendResponse>> GetPrescriptionVolumeTrend(
            [FromQuery] AnalyticsGranularity granularity = AnalyticsGranularity.Day,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _mediator.Send(new GetPrescriptionVolumeTrendQuery(granularity, from, to));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("patients")]
        public async Task<ActionResult<PatientAnalyticsResponse>> GetPatientAnalytics(
            [FromQuery] AnalyticsGranularity granularity = AnalyticsGranularity.Day)
        {
            var result = await _mediator.Send(new GetPatientAnalyticsQuery(granularity));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("executive-summary")]
        public async Task<ActionResult<ExecutiveSummaryResponse>> GetExecutiveSummary()
        {
            var result = await _mediator.Send(new GetExecutiveSummaryQuery());
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("my/doctor-stats")]
        public async Task<ActionResult<MyDoctorAnalyticsResponse>> GetMyDoctorAnalytics()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetMyDoctorAnalyticsQuery(keycloakId));
            return Ok(result);
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("my/staff-stats")]
        public async Task<ActionResult<MyStaffAnalyticsResponse>> GetMyStaffAnalytics()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetMyStaffAnalyticsQuery(keycloakId));
            return Ok(result);
        }
    }
}
