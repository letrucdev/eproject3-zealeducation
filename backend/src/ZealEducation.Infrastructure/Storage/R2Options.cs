namespace ZealEducation.Infrastructure.Storage;

public class R2Options
{
    public const string SectionName = "R2";

    public string AccountId { get; set; } = default!;
    public string AccessKeyId { get; set; } = default!;
    public string SecretAccessKey { get; set; } = default!;
    public string BucketName { get; set; } = default!;
}
