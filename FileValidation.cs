using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Katz0rz
{
    public static class FileValidation
    {
        private static readonly string[] permittedExtensions = { ".png", ".jpeg", ".jpg" };

        private static readonly Dictionary<string, List<byte[]>> _fileSignature =
            new Dictionary<string, List<byte[]>>
        {
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".png", new List<byte[]>
                {
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
        };


        public static AiPicture ValidateImage(AiPicture picture)
        {
            if (picture.Picture is null)
            {
                return null;
            }

            // Extension Validation
            var ext = Path.GetExtension(picture.Picture.FileName).ToLowerInvariant();

            System.Diagnostics.Debug.WriteLine("Extension: " + ext);

            if (!string.IsNullOrEmpty(ext) || permittedExtensions.Contains(ext) || FileSignatureValidation(ext, picture))
            {

                System.Diagnostics.Debug.WriteLine("Alles VALID! ALLAA");
                return picture;
            }
            // vllt vorher TotalVirus API Check
            return null;
        }

        private static bool FileSignatureValidation(string ext, AiPicture picture)
        {
            using (var reader = new BinaryReader(picture.Picture.OpenReadStream()))
            {
                var signatures = _fileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                System.Diagnostics.Debug.WriteLine("Signatures: " + signatures);
                System.Diagnostics.Debug.WriteLine("Header Bytes: " + headerBytes);

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
