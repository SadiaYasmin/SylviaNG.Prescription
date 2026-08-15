namespace SylviaNG.Prescription.Application.Interfaces.Services
{
    /// <summary>
    /// US-083: real file storage for uploaded images (doctor photos/signatures, hospital
    /// logo/seal), replacing the inline base64 DB columns those entities used to carry.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Decodes a "data:image/...;base64,..." URI and persists it, returning a relative
        /// URL. A null/whitespace input returns null rather than throwing, matching the
        /// existing "null clears the image" convention (e.g. UpdateDoctorPhotoRequest).
        /// </summary>
        Task<string?> SaveImageAsync(string? base64DataUri, string category, CancellationToken cancellationToken = default);

        /// <summary>No-ops on null/empty/unrecognized URLs — safe to call unconditionally when replacing or removing an image.</summary>
        Task DeleteAsync(string? relativeUrl);
    }
}
