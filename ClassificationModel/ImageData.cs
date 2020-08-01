using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Katz0rz
{
    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath { get; set; }

        [LoadColumn(1)]
        public string Label { get; set; }
    }
}
