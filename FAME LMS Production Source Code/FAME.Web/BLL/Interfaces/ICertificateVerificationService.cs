using First_Aid_Made_Easy.Models.Certificate;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICertificateVerificationService
    {
        /// <summary>
        /// Look up a certificate by its public ID for the verification page.
        /// Returns null if not found.
        /// </summary>
        CertificateVerifyVM Verify(string publicId, string ipAddress = null);

        /// <summary>
        /// Check rate limit for an IP address. Returns true if allowed.
        /// </summary>
        bool CheckRateLimit(string ipAddress);
    }
}
