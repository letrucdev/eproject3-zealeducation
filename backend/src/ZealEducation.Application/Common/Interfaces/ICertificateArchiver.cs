namespace ZealEducation.Application.Common.Interfaces;

public interface ICertificateArchiver
{
    Task<byte[]> GenerateAndUploadAsync(Guid applicationId, CancellationToken cancellationToken, bool forceRegenerate = false);
}
