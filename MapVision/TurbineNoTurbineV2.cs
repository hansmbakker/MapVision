using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 9c60ffe7-d37d-442f-869b-41d2babef71f_788a1efa-4381-4cdc-8491-5f273a5f73de

namespace MapVision
{
    public sealed class TurbineNoTurbineV2ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class TurbineNoTurbineV2ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public TurbineNoTurbineV2ModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>()
            {
                { "no_turbine", float.NaN },
                { "turbine", float.NaN },
            };
        }
    }

    public sealed class TurbineNoTurbineV2Model
    {
        private LearningModelPreview learningModel;
        public static async Task<TurbineNoTurbineV2Model> CreateTurbineNoTurbineV2Model(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            TurbineNoTurbineV2Model model = new TurbineNoTurbineV2Model();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<TurbineNoTurbineV2ModelOutput> EvaluateAsync(TurbineNoTurbineV2ModelInput input) {
            TurbineNoTurbineV2ModelOutput output = new TurbineNoTurbineV2ModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
