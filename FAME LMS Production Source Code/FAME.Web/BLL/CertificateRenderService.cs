using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models.Certificate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace First_Aid_Made_Easy.BLL
{
    public class CertificateRenderService : ICertificateRenderService
    {
        private readonly ILogger _logger;

        public CertificateRenderService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate a certificate PDF from Fabric.js JSON + token context.
        /// Writes to ~/File-Repository/Certificates/PDFs/{PublicId}.pdf
        /// Returns the relative path.
        /// </summary>
        public string GeneratePdf(CertificateRenderContext context)
        {
            var html = RenderCertificateHtml(context);
            var fileName = $"{context.CertificateId}.pdf";
            var relDir = "File-Repository/Certificates/PDFs";
            var absDir = HostingEnvironment.MapPath($"~/{relDir}");
            Directory.CreateDirectory(absDir);
            var absPath = Path.Combine(absDir, fileName);

            // Use Rotativa's wkhtmltopdf binary directly
            var wkhtmlPath = HostingEnvironment.MapPath("~/Rotativa/wkhtmltopdf.exe");
            if (!File.Exists(wkhtmlPath))
            {
                _logger.Error("wkhtmltopdf.exe not found at {Path}", wkhtmlPath);
                throw new FileNotFoundException("wkhtmltopdf.exe not found", wkhtmlPath);
            }

            // Write HTML to a temp file
            var tempHtml = Path.Combine(Path.GetTempPath(), $"cert_{context.CertificateId}.html");
            File.WriteAllText(tempHtml, html, Encoding.UTF8);

            try
            {
                var orientation = context.Orientation?.ToLower() == "portrait" ? "Portrait" : "Landscape";
                var pageSize = context.PageSize?.ToUpper() == "LETTER" ? "Letter" : "A4";

                var args = $"--page-size {pageSize} --orientation {orientation} " +
                           "--disable-smart-shrinking --print-media-type " +
                           "--margin-top 0 --margin-bottom 0 --margin-left 0 --margin-right 0 " +
                           "--encoding UTF-8 " +
                           $"\"{tempHtml}\" \"{absPath}\"";

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = wkhtmlPath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit(30000); // 30s timeout

                if (process.ExitCode != 0)
                {
                    _logger.Error("wkhtmltopdf failed with exit code {Code}: {Error}", process.ExitCode, stderr);
                    throw new InvalidOperationException($"PDF generation failed: {stderr}");
                }

                _logger.Information("Certificate PDF generated: {Path}", absPath);
            }
            finally
            {
                try { File.Delete(tempHtml); } catch { }
            }

            return $"{relDir}/{fileName}";
        }

        /// <summary>
        /// Generate a thumbnail PNG for the certificate.
        /// Returns relative path.
        /// </summary>
        public string GenerateThumbnail(CertificateRenderContext context)
        {
            var html = RenderCertificateHtml(context);
            var fileName = $"{context.CertificateId}_thumb.png";
            var relDir = "File-Repository/Certificates/Thumbnails";
            var absDir = HostingEnvironment.MapPath($"~/{relDir}");
            Directory.CreateDirectory(absDir);
            var absPath = Path.Combine(absDir, fileName);

            var wkhtmlImagePath = HostingEnvironment.MapPath("~/Rotativa/wkhtmltoimage.exe");
            if (!File.Exists(wkhtmlImagePath))
            {
                // Thumbnail is optional — return null if wkhtmltoimage not available
                _logger.Warning("wkhtmltoimage.exe not found — skipping thumbnail generation");
                return null;
            }

            var tempHtml = Path.Combine(Path.GetTempPath(), $"cert_thumb_{context.CertificateId}.html");
            File.WriteAllText(tempHtml, html, Encoding.UTF8);

            try
            {
                var args = $"--width 800 --quality 80 \"{tempHtml}\" \"{absPath}\"";
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = wkhtmlImagePath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(15000);

                if (process.ExitCode != 0) return null;
            }
            finally
            {
                try { File.Delete(tempHtml); } catch { }
            }

            return $"{relDir}/{fileName}";
        }

        /// <summary>
        /// Generate a QR code as Base64-encoded PNG using QRCoder.
        /// </summary>
        public string GenerateQrCodeBase64(string verificationUrl)
        {
            try
            {
                // Using QRCoder NuGet package
                var qrGenerator = new QRCoder.QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(verificationUrl, QRCoder.QRCodeGenerator.ECCLevel.M);
                var qrCode = new QRCoder.PngByteQRCode(qrData);
                var pngBytes = qrCode.GetGraphic(8); // 8 pixels per module
                return Convert.ToBase64String(pngBytes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to generate QR code for {Url}", verificationUrl);
                return null;
            }
        }

        /// <summary>
        /// Convert Fabric.js canvas JSON + tokens into full HTML for PDF rendering.
        /// </summary>
        public string RenderCertificateHtml(CertificateRenderContext context)
        {
            // Determine page dimensions in points (CSS px)
            int pageWidth, pageHeight;
            if (context.PageSize?.ToUpper() == "LETTER")
            {
                pageWidth = 1056; pageHeight = 816; // 11in × 8.5in at 96dpi landscape
            }
            else
            {
                pageWidth = 1122; pageHeight = 793; // A4 landscape at 96dpi
            }

            if (context.Orientation?.ToLower() == "portrait")
            {
                var temp = pageWidth;
                pageWidth = pageHeight;
                pageHeight = temp;
            }

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"UTF-8\">");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: " + (context.PageSize?.ToUpper() == "LETTER" ? "letter" : "A4") + " " + (context.Orientation?.ToLower() ?? "landscape") + "; margin: 0; }");
            sb.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            sb.AppendLine($"body {{ width: {pageWidth}px; height: {pageHeight}px; overflow: hidden; font-family: 'Georgia', 'Times New Roman', serif; }}");
            sb.AppendLine($".canvas-container {{ position: relative; width: {pageWidth}px; height: {pageHeight}px; }}");
            sb.AppendLine(".canvas-obj { position: absolute; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine($"<div class=\"canvas-container\">");

            // Parse Fabric.js JSON
            if (!string.IsNullOrEmpty(context.LayoutJson) && context.LayoutJson != "{}")
            {
                try
                {
                    var canvas = JObject.Parse(context.LayoutJson);
                    var objects = canvas["objects"] as JArray;
                    var bgColor = canvas["background"]?.ToString();
                    var bgImage = canvas["backgroundImage"]?["src"]?.ToString();

                    if (!string.IsNullOrEmpty(bgColor))
                        sb.AppendLine($"<div style=\"position:absolute;top:0;left:0;width:100%;height:100%;background:{bgColor};\"></div>");
                    if (!string.IsNullOrEmpty(bgImage))
                        sb.AppendLine($"<img src=\"{bgImage}\" style=\"position:absolute;top:0;left:0;width:100%;height:100%;object-fit:cover;\" />");

                    if (objects != null)
                    {
                        foreach (var obj in objects)
                        {
                            RenderFabricObject(sb, obj, context);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to parse Fabric.js JSON for cert {CertId}", context.CertificateId);
                    // If JSON fails, render a simple fallback
                    RenderFallbackCertificate(sb, context, pageWidth, pageHeight);
                }
            }
            else
            {
                // No layout JSON — render a basic fallback
                RenderFallbackCertificate(sb, context, pageWidth, pageHeight);
            }

            // Inject QR code in bottom-right if present
            if (!string.IsNullOrEmpty(context.QrCodeBase64))
            {
                sb.AppendLine($"<img src=\"data:image/png;base64,{context.QrCodeBase64}\" " +
                    $"style=\"position:absolute;bottom:20px;right:20px;width:80px;height:80px;\" alt=\"QR\" />");
            }

            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        /// <summary>
        /// Generate a preview PDF with sample data (for builder preview button).
        /// </summary>
        public byte[] GeneratePreviewPdf(string layoutJson, string orientation, string pageSize)
        {
            var context = new CertificateRenderContext
            {
                LearnerName = "Jane Doe",
                CourseTitle = "First Aid & Emergency Response Certification",
                CertificateId = "PREVIEW-0000-0000",
                IssueDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddYears(2),
                Credits = 12.5m,
                VerificationUrl = "/Certificate/Verify/PREVIEW",
                QrCodeBase64 = GenerateQrCodeBase64("/Certificate/Verify/PREVIEW"),
                LayoutJson = layoutJson,
                Orientation = orientation,
                PageSize = pageSize
            };

            var html = RenderCertificateHtml(context);
            var tempHtml = Path.Combine(Path.GetTempPath(), $"cert_preview_{Guid.NewGuid()}.html");
            var tempPdf = Path.Combine(Path.GetTempPath(), $"cert_preview_{Guid.NewGuid()}.pdf");

            File.WriteAllText(tempHtml, html, Encoding.UTF8);

            try
            {
                var wkhtmlPath = HostingEnvironment.MapPath("~/Rotativa/wkhtmltopdf.exe");
                var orientVal = orientation?.ToLower() == "portrait" ? "Portrait" : "Landscape";
                var pageSizeVal = pageSize?.ToUpper() == "LETTER" ? "Letter" : "A4";

                var args = $"--page-size {pageSizeVal} --orientation {orientVal} " +
                           "--disable-smart-shrinking --print-media-type " +
                           "--margin-top 0 --margin-bottom 0 --margin-left 0 --margin-right 0 " +
                           $"\"{tempHtml}\" \"{tempPdf}\"";

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = wkhtmlPath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(30000);

                if (File.Exists(tempPdf))
                    return File.ReadAllBytes(tempPdf);

                return null;
            }
            finally
            {
                try { File.Delete(tempHtml); } catch { }
                try { File.Delete(tempPdf); } catch { }
            }
        }

        #region Private Helpers

        private void RenderFabricObject(StringBuilder sb, JToken obj, CertificateRenderContext ctx)
        {
            var type = obj["type"]?.ToString()?.ToLower();
            var left = obj["left"]?.Value<double>() ?? 0;
            var top = obj["top"]?.Value<double>() ?? 0;
            var scaleX = obj["scaleX"]?.Value<double>() ?? 1;
            var scaleY = obj["scaleY"]?.Value<double>() ?? 1;
            var angle = obj["angle"]?.Value<double>() ?? 0;
            var opacity = obj["opacity"]?.Value<double>() ?? 1;
            var visible = obj["visible"]?.Value<bool>() ?? true;

            if (!visible) return;

            var transform = $"left:{left}px;top:{top}px;opacity:{opacity};";
            if (angle != 0)
                transform += $"transform:rotate({angle}deg);";

            switch (type)
            {
                case "i-text":
                case "textbox":
                case "text":
                    RenderTextObject(sb, obj, ctx, transform, scaleX, scaleY);
                    break;
                case "image":
                    RenderImageObject(sb, obj, transform, scaleX, scaleY);
                    break;
                case "rect":
                    RenderRectObject(sb, obj, transform, scaleX, scaleY);
                    break;
                case "circle":
                    RenderCircleObject(sb, obj, transform, scaleX, scaleY);
                    break;
                case "line":
                    RenderLineObject(sb, obj, transform, scaleX, scaleY);
                    break;
                default:
                    // Unknown type — skip
                    break;
            }
        }

        private void RenderTextObject(StringBuilder sb, JToken obj, CertificateRenderContext ctx, string baseStyle, double scaleX, double scaleY)
        {
            var text = obj["text"]?.ToString() ?? "";
            var fontSize = (obj["fontSize"]?.Value<double>() ?? 24) * scaleY;
            var fontFamily = obj["fontFamily"]?.ToString() ?? "Georgia";
            var fill = obj["fill"]?.ToString() ?? "#000000";
            var fontWeight = obj["fontWeight"]?.ToString() ?? "normal";
            var fontStyle = obj["fontStyle"]?.ToString() ?? "normal";
            var textAlign = obj["textAlign"]?.ToString() ?? "left";
            var underline = obj["underline"]?.Value<bool>() ?? false;
            var width = obj["width"]?.Value<double>() ?? 0;

            // Token replacement
            var tokenType = obj["tokenType"]?.ToString();
            if (!string.IsNullOrEmpty(tokenType))
                text = ResolveToken(tokenType, ctx);
            else
                text = ReplaceTokensInText(text, ctx);

            var style = baseStyle +
                $"font-size:{fontSize:F0}px;" +
                $"font-family:'{fontFamily}',serif;" +
                $"color:{fill};" +
                $"font-weight:{fontWeight};" +
                $"font-style:{fontStyle};" +
                $"text-align:{textAlign};" +
                (underline ? "text-decoration:underline;" : "") +
                (width > 0 ? $"width:{width * scaleX:F0}px;" : "") +
                "white-space:pre-wrap;";

            sb.AppendLine($"<div class=\"canvas-obj\" style=\"{style}\">{HttpUtility.HtmlEncode(text)}</div>");
        }

        private void RenderImageObject(StringBuilder sb, JToken obj, string baseStyle, double scaleX, double scaleY)
        {
            var src = obj["src"]?.ToString() ?? "";
            var width = (obj["width"]?.Value<double>() ?? 100) * scaleX;
            var height = (obj["height"]?.Value<double>() ?? 100) * scaleY;

            // Resolve relative paths
            if (!src.StartsWith("data:") && !src.StartsWith("http"))
            {
                src = $"/Images/Certificates/Assets/{Path.GetFileName(src)}";
            }

            sb.AppendLine($"<img class=\"canvas-obj\" src=\"{src}\" style=\"{baseStyle}width:{width:F0}px;height:{height:F0}px;\" />");
        }

        private void RenderRectObject(StringBuilder sb, JToken obj, string baseStyle, double scaleX, double scaleY)
        {
            var width = (obj["width"]?.Value<double>() ?? 100) * scaleX;
            var height = (obj["height"]?.Value<double>() ?? 100) * scaleY;
            var fill = obj["fill"]?.ToString() ?? "transparent";
            var stroke = obj["stroke"]?.ToString();
            var strokeWidth = obj["strokeWidth"]?.Value<double>() ?? 0;
            var rx = obj["rx"]?.Value<double>() ?? 0;

            var style = baseStyle + $"width:{width:F0}px;height:{height:F0}px;background:{fill};";
            if (!string.IsNullOrEmpty(stroke) && strokeWidth > 0)
                style += $"border:{strokeWidth}px solid {stroke};";
            if (rx > 0)
                style += $"border-radius:{rx}px;";

            sb.AppendLine($"<div class=\"canvas-obj\" style=\"{style}\"></div>");
        }

        private void RenderCircleObject(StringBuilder sb, JToken obj, string baseStyle, double scaleX, double scaleY)
        {
            var radius = (obj["radius"]?.Value<double>() ?? 50);
            var width = radius * 2 * scaleX;
            var height = radius * 2 * scaleY;
            var fill = obj["fill"]?.ToString() ?? "transparent";
            var stroke = obj["stroke"]?.ToString();
            var strokeWidth = obj["strokeWidth"]?.Value<double>() ?? 0;

            var style = baseStyle + $"width:{width:F0}px;height:{height:F0}px;background:{fill};border-radius:50%;";
            if (!string.IsNullOrEmpty(stroke) && strokeWidth > 0)
                style += $"border:{strokeWidth}px solid {stroke};";

            sb.AppendLine($"<div class=\"canvas-obj\" style=\"{style}\"></div>");
        }

        private void RenderLineObject(StringBuilder sb, JToken obj, string baseStyle, double scaleX, double scaleY)
        {
            var stroke = obj["stroke"]?.ToString() ?? "#000";
            var strokeWidth = obj["strokeWidth"]?.Value<double>() ?? 1;
            var x1 = obj["x1"]?.Value<double>() ?? 0;
            var y1 = obj["y1"]?.Value<double>() ?? 0;
            var x2 = obj["x2"]?.Value<double>() ?? 100;
            var y2 = obj["y2"]?.Value<double>() ?? 0;

            var length = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) * scaleX;
            var angle = Math.Atan2(y2 - y1, x2 - x1) * 180 / Math.PI;

            var style = baseStyle + $"width:{length:F0}px;height:0;border-top:{strokeWidth}px solid {stroke};transform-origin:0 0;transform:rotate({angle:F1}deg);";
            sb.AppendLine($"<div class=\"canvas-obj\" style=\"{style}\"></div>");
        }

        private string ResolveToken(string tokenType, CertificateRenderContext ctx)
        {
            switch (tokenType?.ToUpper())
            {
                case "LEARNER_NAME": return ctx.LearnerName ?? "";
                case "COURSE_TITLE": return ctx.CourseTitle ?? "";
                case "CERT_ID": return ctx.CertificateId ?? "";
                case "ISSUE_DATE": return ctx.IssueDate.ToString("MMMM dd, yyyy");
                case "EXPIRY_DATE": return ctx.ExpiryDate?.ToString("MMMM dd, yyyy") ?? "";
                case "CREDITS": return ctx.Credits?.ToString("F1") ?? "";
                default: return $"{{{tokenType}}}";
            }
        }

        private string ReplaceTokensInText(string text, CertificateRenderContext ctx)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace("{LEARNER_NAME}", ctx.LearnerName ?? "")
                .Replace("{COURSE_TITLE}", ctx.CourseTitle ?? "")
                .Replace("{CERT_ID}", ctx.CertificateId ?? "")
                .Replace("{ISSUE_DATE}", ctx.IssueDate.ToString("MMMM dd, yyyy"))
                .Replace("{EXPIRY_DATE}", ctx.ExpiryDate?.ToString("MMMM dd, yyyy") ?? "")
                .Replace("{CREDITS}", ctx.Credits?.ToString("F1") ?? "");
        }

        private void RenderFallbackCertificate(StringBuilder sb, CertificateRenderContext ctx, int pageWidth, int pageHeight)
        {
            // Simple elegant fallback when no canvas JSON is defined
            sb.AppendLine($"<div style=\"position:absolute;top:0;left:0;width:{pageWidth}px;height:{pageHeight}px;background:#FFFDF7;border:3px solid #0F766E;\">");
            sb.AppendLine($"  <div style=\"position:absolute;top:8px;left:8px;right:8px;bottom:8px;border:1px solid #B8860B;\"></div>");
            sb.AppendLine($"  <div style=\"position:absolute;top:60px;left:0;width:100%;text-align:center;\">");
            sb.AppendLine($"    <div style=\"font-size:16px;color:#0F766E;letter-spacing:4px;text-transform:uppercase;\">First Aid Made Easy</div>");
            sb.AppendLine($"    <div style=\"font-size:36px;color:#1E293B;margin-top:30px;font-family:Georgia,serif;\">Certificate of Completion</div>");
            sb.AppendLine($"    <div style=\"font-size:14px;color:#666;margin-top:20px;\">This is to certify that</div>");
            sb.AppendLine($"    <div style=\"font-size:28px;color:#1E293B;margin-top:15px;font-weight:bold;\">{HttpUtility.HtmlEncode(ctx.LearnerName)}</div>");
            sb.AppendLine($"    <div style=\"font-size:14px;color:#666;margin-top:20px;\">has successfully completed the course</div>");
            sb.AppendLine($"    <div style=\"font-size:22px;color:#0F766E;margin-top:15px;font-style:italic;\">{HttpUtility.HtmlEncode(ctx.CourseTitle)}</div>");
            sb.AppendLine($"    <div style=\"font-size:14px;color:#666;margin-top:30px;\">Issued on {ctx.IssueDate:MMMM dd, yyyy}</div>");
            if (ctx.Credits.HasValue)
                sb.AppendLine($"    <div style=\"font-size:14px;color:#666;margin-top:8px;\">Credits: {ctx.Credits:F1}</div>");
            sb.AppendLine($"    <div style=\"font-size:11px;color:#999;margin-top:30px;\">Certificate ID: {ctx.CertificateId}</div>");
            sb.AppendLine($"  </div>");
            sb.AppendLine($"</div>");
        }

        #endregion
    }
}
