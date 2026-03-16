using System;
using System.Web;
using SixLabors.ImageSharp;
using System.Drawing.Imaging;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using System.Collections.Generic;
using System.Linq;
using static Google.Cloud.Vision.V1.FaceAnnotation.Types;
using System.Text.RegularExpressions;
using System.Drawing;
using SixLabors.ImageSharp.Processing;

namespace First_Aid_Made_Easy.BLL
{
    public class ImageProcessing
    {
        public string AbsPath { get; set; }
        public string _Path { get; set; }
        public ImageAnnotatorClient _Client { get; set; }

        public readonly string Key;
        public ImageProcessing(string Path)
        {
            var ser = HttpContext.Current.Server;
            Key = ser.MapPath("~/Content/fame-ocr-04d7ad4553c5.json");
            _Path = Path;
            AbsPath = ser.MapPath("~/Images/" + Path);
        }

        public bool Process()
        {
            using (var image = SixLabors.ImageSharp.Image.Load(AbsPath))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(800, 600),
                    Mode = ResizeMode.Max
                }));

                image.Mutate(x => x.GaussianSharpen());
                image.Mutate(x => x.Grayscale());
                image.Save(AbsPath);
            }
            return true;
        }


        public string Compress()
        {
            var Dir = Path.GetDirectoryName(AbsPath);
            var FileName = Guid.NewGuid() + Path.GetExtension(AbsPath);
            var NewPath = Path.Combine(Dir, FileName);
            using (var image = System.Drawing.Image.FromFile(AbsPath))
            {
                var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L); // Set the compression level to 50%
                image.Save(NewPath, jpegEncoder, encoderParams);
            }
            File.Delete(AbsPath);
            AbsPath = NewPath;
            return Path.Combine(Path.GetDirectoryName(_Path), FileName);
        }

        public ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            // Get the list of all image codecs
            var codecs = ImageCodecInfo.GetImageDecoders();

            // Find the codec that matches the specified format
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }
        public string VisionOCR()
        {
            try
            {
                GenrateClient();
                var imageBytes = File.ReadAllBytes(AbsPath);
                var image = Google.Cloud.Vision.V1.Image.FromBytes(imageBytes);
                var request = new AnnotateImageRequest
                {
                    Image = image,
                    Features = { new Feature { Type = Feature.Types.Type.TextDetection } }
                };

                var response = _Client.Annotate(request);
                return response.FullTextAnnotation.Text;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                JzLogger.WriteError(ex, "OCR Error " + Path.GetFileName(AbsPath));
            }

            return "";
        }
        public bool HasValidHumanFace()
        {
            GenrateClient();

            var imageBytes = File.ReadAllBytes(AbsPath);
            var image = Google.Cloud.Vision.V1.Image.FromBytes(imageBytes);
            IReadOnlyList<FaceAnnotation> response = _Client.DetectFaces(image);
            foreach (var face in response)
            {
                if (face.DetectionConfidence >= 0.5f &&
                    face.LandmarkingConfidence >= 0.5f &&
                    face.Landmarks.Any(lm => lm.Type == Landmark.Types.Type.NoseTip) &&
                    face.Landmarks.Any(lm => lm.Type == Landmark.Types.Type.LeftEye) &&
                    face.Landmarks.Any(lm => lm.Type == Landmark.Types.Type.RightEye))
                {
                    return true;
                }
            }
            return false;
        }
        public string GetProfileImage()
        {
            try
            {

                GenrateClient();
                var Dir = Path.GetDirectoryName(AbsPath);
                var FileName = Guid.NewGuid() + Path.GetExtension(AbsPath);
                var NewPath = Path.Combine(Dir, FileName);

                var imageBytes = File.ReadAllBytes(AbsPath);
                var image = Google.Cloud.Vision.V1.Image.FromBytes(imageBytes);
                IReadOnlyList<FaceAnnotation> response = _Client.DetectFaces(image);

                foreach (var annotation in response)
                {
                    var vertices = annotation.BoundingPoly.Vertices;
                    var x1 = vertices[0].X - 50;
                    var y1 = vertices[0].Y - 50;
                    var x2 = vertices[2].X + 100;
                    var y2 = vertices[2].Y + 100;
                    var width = x2 - x1;
                    var height = y2 - y1;
                    var rectangle = new System.Drawing.Rectangle(x1, y1, width, height);
                    var faceRegion = new Bitmap(width, height);
                    using (var g = Graphics.FromImage(faceRegion))
                    {
                        using (var stream = new MemoryStream(image.Content.ToByteArray()))
                        {
                            var bitmap = new Bitmap(stream);
                            g.DrawImage(bitmap, 0, 0, rectangle, GraphicsUnit.Pixel);
                        }
                    }

                    // Save the face region to a file
                    faceRegion.Save(NewPath);
                    return Path.Combine(Path.GetDirectoryName(_Path), FileName);
                }
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "Get Profile Image Error " + Path.GetFileName(AbsPath));
            }
            return "";
        }

        private void GenrateClient()
        {
            if (_Client == null)
            {
                var builder = new ImageAnnotatorClientBuilder
                {
                    CredentialsPath = Key,
                    //Endpoint = "eu-vision.googleapis.com"
                };
                _Client = builder.Build();
            }
        }


        public CNICParseResult ParseCNIC(string input)
        {
            var r = new CNICParseResult { };
            Match nameMatch = Regex.Match(input, @"Name\s+([^\n\r]+)");
            if (nameMatch.Success)
            {
                r.Name = nameMatch.Groups[1].Value.Trim();
            }

            // Extract the father name using a regular expression
            Match fatherNameMatch = Regex.Match(input, @"Father Name\s+([^\n\r]+)");
            if (fatherNameMatch.Success)
            {
                r.FatherName = fatherNameMatch.Groups[1].Value.Trim();
            }

            // Extract the CNIC number using a regular expression
            Match cnicMatch = Regex.Match(input, @"\d{5}-\d{7}-\d");
            if (cnicMatch.Success)
            {
                r.CNICNo = cnicMatch.Groups[0].Value.Trim();
            }

            // Extract the date of birth using a regular expression
            //Match dobMatch = Regex.Match(input, @"Date of Birth\s+([\d.]+)");
            //if (dobMatch.Success)
            //{
            //    DateTime dob = DateTime.ParseExact(dobMatch.Groups[1].Value.Trim(), "dd.MM.yyyy", null);
            //}

            r.isValid = r.Name != null && r.CNICNo != null && r.FatherName != null && r.CNICNo.Length == 15;

            if (!r.isValid)
                JzLogger.WriteWarning(null, "Parse Complete " + Path.GetFileName(AbsPath) + "  Parse Data = " + input);

            return r;
        }

        public bool DeleteFile()
        {
            if (File.Exists(AbsPath))
            {
                File.Delete(AbsPath);
                return true;
            }
            return false;
        }
    }

    public class CNICParseResult
    {
        public string Name { get; set; }
        public string FatherName { get; set; }
        public string CNICNo { get; set; }
        public bool isValid { get; set; }

        public string PicPath { get; set; }
    }

}