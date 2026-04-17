using System;
using System.Collections.Generic;
using System.Linq;

namespace Contracts
{
    public record SurveyResponse(Guid SurveyId, string RespondentId, int Score, string Comment);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class SurveyResponseConsumer : IConsumer<Contracts.SurveyResponse>
{
    public List<int> Scores = new();

    public double AverageScore => Scores.Count == 0 ? 0.0 : Scores.Average();

    public void Consume(Contracts.SurveyResponse message)
    {
        Scores.Add(message.Score);
    }
}