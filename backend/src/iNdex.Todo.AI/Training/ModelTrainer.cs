using iNdex.Todo.AI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;

namespace iNdex.Todo.AI.Training;

/// <summary>
/// Trains and persists all four ML.NET predictive models.
/// Call ModelTrainer.TrainAllAsync() from a background service on startup
/// if model files don't exist, then retrain weekly via a Hangfire/hosted service job.
/// </summary>
public sealed class ModelTrainer(ILogger<ModelTrainer> logger)
{
    private readonly MLContext _ml = new(seed: 42);

    // ── Feature 1: Priority Classification ───────────────────────────────────

    public ITransformer TrainPriorityModel(IEnumerable<PriorityTrainingData> data, string savePath)
    {
        logger.LogInformation("Training priority prediction model...");

        var dataView = _ml.Data.LoadFromEnumerable(data);

        var pipeline = _ml.Transforms.Text.FeaturizeText("TitleFeatures", nameof(PriorityTrainingData.Title))
            .Append(_ml.Transforms.Text.FeaturizeText("DescFeatures",  nameof(PriorityTrainingData.Description)))
            .Append(_ml.Transforms.Categorical.OneHotEncoding("CategoryEncoded", nameof(PriorityTrainingData.Category)))
            .Append(_ml.Transforms.Categorical.OneHotEncoding("TypeEncoded",     nameof(PriorityTrainingData.TicketType)))
            .Append(_ml.Transforms.Concatenate("Features",
                "TitleFeatures", "DescFeatures", "CategoryEncoded", "TypeEncoded"))
            .Append(_ml.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features",
                maximumNumberOfIterations: 100))
            .Append(_ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var cv = _ml.MulticlassClassification.CrossValidate(dataView, pipeline, numberOfFolds: 5);
        var avgAcc = cv.Average(r => r.Metrics.MacroAccuracy);
        logger.LogInformation("Priority model CV macro-accuracy: {Acc:P1}", avgAcc);

        var model = pipeline.Fit(dataView);
        _ml.Model.Save(model, dataView.Schema, savePath);
        logger.LogInformation("Priority model saved to {Path}", savePath);
        return model;
    }

    // ── Feature 2: Assignee Recommendation ───────────────────────────────────

    public ITransformer TrainAssigneeModel(IEnumerable<AssigneeFeatureData> data, string savePath)
    {
        logger.LogInformation("Training assignee recommendation model...");

        var dataView = _ml.Data.LoadFromEnumerable(data);

        var pipeline = _ml.Transforms.Categorical.OneHotEncoding("CategoryEnc", nameof(AssigneeFeatureData.TicketCategory))
            .Append(_ml.Transforms.Categorical.OneHotEncoding("TypeEnc",     nameof(AssigneeFeatureData.TicketType)))
            .Append(_ml.Transforms.Categorical.OneHotEncoding("PriorityEnc", nameof(AssigneeFeatureData.TicketPriority)))
            .Append(_ml.Transforms.Text.FeaturizeText("SkillFeatures", nameof(AssigneeFeatureData.UserSkills)))
            .Append(_ml.Transforms.Concatenate("Features",
                "CategoryEnc", "TypeEnc", "PriorityEnc", "SkillFeatures",
                nameof(AssigneeFeatureData.UserWorkload),
                nameof(AssigneeFeatureData.UserSuccessRate)))
            .Append(_ml.BinaryClassification.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 5));

        var model = pipeline.Fit(dataView);
        var metrics = _ml.BinaryClassification.Evaluate(model.Transform(dataView));
        logger.LogInformation("Assignee model AUC: {Auc:P1}, F1: {F1:P1}",
            metrics.AreaUnderRocCurve, metrics.F1Score);

        _ml.Model.Save(model, dataView.Schema, savePath);
        logger.LogInformation("Assignee model saved to {Path}", savePath);
        return model;
    }

    // ── Feature 3: Completion Time Regression ────────────────────────────────

    public ITransformer TrainCompletionTimeModel(IEnumerable<CompletionTimeData> data, string savePath)
    {
        logger.LogInformation("Training completion time model...");

        var dataView = _ml.Data.LoadFromEnumerable(data);

        var pipeline = _ml.Transforms.Categorical.OneHotEncoding("TypeEnc",     nameof(CompletionTimeData.TicketType))
            .Append(_ml.Transforms.Categorical.OneHotEncoding("PriorityEnc", nameof(CompletionTimeData.Priority)))
            .Append(_ml.Transforms.Categorical.OneHotEncoding("CategoryEnc", nameof(CompletionTimeData.Category)))
            .Append(_ml.Transforms.Concatenate("Features",
                "TypeEnc", "PriorityEnc", "CategoryEnc",
                nameof(CompletionTimeData.TitleLength),
                nameof(CompletionTimeData.DescLength),
                nameof(CompletionTimeData.UserAvgDays)))
            .Append(_ml.Regression.Trainers.FastForest(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfTrees: 100));

        var model = pipeline.Fit(dataView);
        var metrics = _ml.Regression.Evaluate(model.Transform(dataView));
        logger.LogInformation("Completion time model RMSE: {Rmse:F2} days, R²: {R2:F3}",
            metrics.RootMeanSquaredError, metrics.RSquared);

        _ml.Model.Save(model, dataView.Schema, savePath);
        logger.LogInformation("Completion time model saved to {Path}", savePath);
        return model;
    }

    // ── Feature 4: Deadline Risk Binary Classification ────────────────────────

    public ITransformer TrainDeadlineRiskModel(IEnumerable<DeadlineRiskData> data, string savePath)
    {
        logger.LogInformation("Training deadline risk model...");

        var dataView = _ml.Data.LoadFromEnumerable(data);

        var pipeline = _ml.Transforms.Categorical.OneHotEncoding("PriorityEnc", nameof(DeadlineRiskData.Priority))
            .Append(_ml.Transforms.Concatenate("Features",
                "PriorityEnc",
                nameof(DeadlineRiskData.DaysUntilDue),
                nameof(DeadlineRiskData.ProgressPercent),
                nameof(DeadlineRiskData.AssigneeWorkload),
                nameof(DeadlineRiskData.AssigneeOnTimeRate),
                nameof(DeadlineRiskData.EstimatedDays)))
            .Append(_ml.BinaryClassification.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100));

        var model = pipeline.Fit(dataView);
        var metrics = _ml.BinaryClassification.Evaluate(model.Transform(dataView));
        logger.LogInformation("Deadline risk model AUC: {Auc:P1}", metrics.AreaUnderRocCurve);

        _ml.Model.Save(model, dataView.Schema, savePath);
        logger.LogInformation("Deadline risk model saved to {Path}", savePath);
        return model;
    }
}
