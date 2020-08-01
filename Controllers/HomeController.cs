using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Katz0rz.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class HomeController : ControllerBase
    {

        [HttpPost]
        [RequestSizeLimit(2_097_152)]
        public async Task<IActionResult> ImagePrediction([FromForm] AiPicture picture)
        {
            picture = FileValidation.ValidateImage(picture);

            //For Windows
            var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            
            // For Linux
            //var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
            var assetsRelativePath = Path.Combine(projectDirectory, "ClassificationModel/assets");
            var modelRelativePath = Path.Combine(projectDirectory, "ClassificationModel/model");

            var dataLocation = Path.Combine(assetsRelativePath, "labels.csv");
            var imagesFolder = Path.Combine(assetsRelativePath, "images/");
            var modelLocation = Path.Combine(modelRelativePath, "tensorflow_inception_graph.pb");
            var labelsLocation = Path.Combine(modelRelativePath, "imagenet_comp_graph_label_strings.txt");

            //var savedPicturePath = Path.Combine(projectDirectory, "uploadedPictures");
            var imageFolderPath = Directory.CreateDirectory("./uploadedPictures/");

            DeleteFiles(imageFolderPath.FullName);

            if (picture is null)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType);
            }

            try
            {
                var fileExtension = Path.GetExtension(picture.Picture.FileName);
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(imageFolderPath.FullName, fileName);

                using (var ms = new FileStream(filePath, FileMode.Create))
                {
                    await picture.Picture.CopyToAsync(ms);
                }

                var modelScorer = new ModelScorer(dataLocation, imagesFolder, modelLocation, labelsLocation);

                var prediction = (ImagePrediction)modelScorer.Score(new ImageData() { ImagePath = filePath, Label = "" });
                
                string imagePath = Path.Combine(imageFolderPath.Name, new FileInfo(filePath).Name);
                prediction.ImagePath = imagePath;
                    
                System.Diagnostics.Debug.WriteLine("======================================");
                System.Diagnostics.Debug.WriteLine(prediction.PredictedLabelValue + " with " + prediction.Score.Max()); 
                System.Diagnostics.Debug.WriteLine("======================================");

                //return RedirectToPage("/", prediction);
                return Ok(prediction);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                System.Diagnostics.Debug.WriteLine(e);
                System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                //throw;
                using FileStream fs = System.IO.File.Create(DateTime.Now + ".log");
                using var sr = new StreamWriter(fs);

                sr.Write(e.ToString());
                return BadRequest();
            }
            //return Redirect("/");
        }

        void DeleteFiles(string filePath)
        {
            foreach (string fileName in Directory.GetFiles(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"Deleting: {fileName}");
                System.IO.File.Delete(fileName);
            }
        }
       
    }
}
