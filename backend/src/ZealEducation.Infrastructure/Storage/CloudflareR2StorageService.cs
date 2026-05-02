using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using ZealEducation.Application.Common.Interfaces;

namespace ZealEducation.Infrastructure.Storage;

public class CloudflareR2StorageService(IAmazonS3 s3Client, IOptions<R2Options> options) : IFileStorageService
{
    private readonly string _bucketName = options.Value.BucketName;

    public async Task<string> UploadAsync(byte[] content, string objectKey, string contentType, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(content);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await s3Client.PutObjectAsync(request, cancellationToken);

        return objectKey;
    }

    public async Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey
        };

        using var response = await s3Client.GetObjectAsync(request, cancellationToken);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey
        };

        await s3Client.DeleteObjectAsync(request, cancellationToken);
    }
}
