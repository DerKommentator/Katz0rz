using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace Katz0rz
{
    public class ImagePrediction : ImageData
    {
        public float[] Score { get; set; }
        public string PredictedLabelValue { get; set; }

        // Zum neu Testen
        public float Probability { get; set; }

        [ColumnName("softmax2_pre_activation")]
        public float[] PredictedLabels;
    }
}
