namespace ZealEducation.Application.Common.Helpers;

public static class ExamGradeCalculator
{
    public static string Calculate(decimal score, int maxScore, int passScore)
    {
        if (maxScore <= 0) return string.Empty;
        if (score < passScore) return "F";

        var pct = (double)score / maxScore * 100.0;
        if (pct >= 90) return "A";
        if (pct >= 80) return "B";
        if (pct >= 70) return "C";
        return "D";
    }
}
