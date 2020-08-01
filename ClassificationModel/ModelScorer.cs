using Microsoft.ML;
using System;
using System.IO;
using System.Linq;

namespace Katz0rz
{
    public class ModelScorer
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly string labelsLocation;
        private readonly MLContext context;

        // dataLocation = D:\Microsoft_Visual_Studio\Projects\Katz0rz\AI_Model\labels.csv
        // imagesFolder = D:\Microsoft_Visual_Studio\Projects\Katz0rz\AI_Model\images\
        // modelLocation = D:\Microsoft_Visual_Studio\Projects\Katz0rz\AI_Model\model\tensorflow_inception_graph.pb
        // labelsLocation = D:\Microsoft_Visual_Studio\Projects\Katz0rz\AI_Model\model\imagenet_comp_graph_label_strings.txt

        public ModelScorer(string dataLocation, string imagesFolder, string modelLocation, string labelsLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            this.labelsLocation = labelsLocation;
            context = new MLContext();
        }

        PredictionEngine<ImageData, ImagePrediction> LoadModel(string dataLocation, string imagesFolder, string modelLocation)
        {
            // https://docs.microsoft.com/de-de/dotnet/machine-learning/tutorials/image-classification
            // https://www.youtube.com/watch?v=tAiKAtmPaXU

            var data = context.Data.LoadFromTextFile<ImageData>(dataLocation, separatorChar: ',');

            var pipeline = context.Transforms.Conversion.MapValueToKey("LabelKey", "Label") // TODO: warum muss das gemacht werden
                .Append(context.Transforms.LoadImages("input", imageFolder: imagesFolder, nameof(ImageData.ImagePath))) // TODO: warum muss das gemacht werden
                .Append(context.Transforms.ResizeImages("input", InceptionSettings.ImageWidth, InceptionSettings.ImageHeight, "input"))
                .Append(context.Transforms.ExtractPixels("input", interleavePixelColors: InceptionSettings.ChannelsList, offsetImage: InceptionSettings.Mean))
                .Append(context.Model.LoadTensorFlowModel(modelLocation)
                    .ScoreTensorFlowModel(new[] { "softmax2_pre_activation" }, new[] { "input" }, addBatchDimensionInput: true))
                .Append(context.MulticlassClassification.Trainers.LbfgsMaximumEntropy("LabelKey", "softmax2_pre_activation"))
                .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"));

            var model = pipeline.Fit(data);

            // For evaluation

            /*var imageData = File.ReadAllLines(@"D:\Microsoft_Visual_Studio\Projects\Katz0rz\AI_Model\labels.csv")
                .Select(l => l.Split(','))
                .Select(l => new ImageData { ImagePath = Path.Combine(Environment.CurrentDirectory, "images", l[0]) });

            var imageDataView = context.Data.LoadFromEnumerable(imageData);

            var predictions = model.Transform(imageDataView);

            var imagePredictions = context.Data.CreateEnumerable<ImagePrediction>(predictions, reuseRowObject: false, ignoreMissingColumns: true);

            var evalPredictions = model.Transform(data);

            var metrics = context.MulticlassClassification.Evaluate(evalPredictions, labelColumnName: "LabelKey", predictedLabelColumnName: "PredictedLabel");

            Console.WriteLine($"Log loss - {metrics.LogLoss}");*/

            var predictionFunc = context.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);

            return predictionFunc;
            /*var singlePrediction = predictionFunc.Predict(new ImageData
            {
                ImagePath = Path.Combine(@"D:\Microsoft_Visual_Studio\Projects\Katz0rz\AI_Model\images\", "pizza2.jpg")
            });

            Console.WriteLine($"Image {Path.GetFileName(singlePrediction.ImagePath)} was predicted as " +
                $"a {singlePrediction.PredictedLabelValue} with a score of {singlePrediction.Score.Max()}");
            
            Console.ReadLine();*/
        }

        public ImageData Score(ImageData data)
        {
            var model = LoadModel(dataLocation, imagesFolder, modelLocation);

            var prediction = PredictDataUsingModel(data, labelsLocation, model);


            return prediction;
        }

        protected ImageData PredictDataUsingModel(ImageData testData,
                                                                  string labelsLocation,
                                                                  PredictionEngine<ImageData, ImagePrediction> model)
        {

            var labels = ReadLabels(labelsLocation);

            var probs = model.Predict(testData).PredictedLabels;
            var imageData = model.Predict(new ImageData
            {
                ImagePath = testData.ImagePath,
                Label = testData.Label
            });

            (imageData.PredictedLabelValue, imageData.Probability) = GetBestLabel(labels, probs);
            return imageData;
        }




        public static (string, float) GetBestLabel(string[] labels, float[] probs)
        {
            var max = probs.Max();
            var index = probs.AsSpan().IndexOf(max);
            return (labels[index], max);
        }

        public static string[] ReadLabels(string labelsLocation)
        {
            return File.ReadAllLines(labelsLocation);
        }

    }
}
