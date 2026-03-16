using First_Aid_Made_Easy.Models.Certificate;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICertificateRenderService
    {
        /// <summary>
        /// Generate the certificate PDF from Fabric.js JSON + token values.
        /// Returns the file path to the generated PDF relative to the app root.
        /// </summary>
        string GeneratePdf(CertificateRenderContext context);

        /// <summary>
        /// Generate a thumbnail image of the certificate.
        /// Returns the file path to the generated thumbnail relative to the app root.
        /// </summary>
        string GenerateThumbnail(CertificateRenderContext context);

        /// <summary>
        /// Generate a QR code image as a Base64-encoded PNG string.
        /// </summary>
        string GenerateQrCodeBase64(string verificationUrl);

        /// <summary>
        /// Render the HTML for a certificate (used by Rotativa ViewAsPdf).
        /// </summary>
        string RenderCertificateHtml(CertificateRenderContext context);

        /// <summary>
        /// Generate a preview PDF with sample data (for the builder).
        /// </summary>
        byte[] GeneratePreviewPdf(string layoutJson, string orientation, string pageSize);
    }
}
