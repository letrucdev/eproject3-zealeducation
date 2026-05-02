namespace ZealEducation.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(byte[] content, string objectKey, string contentType, CancellationToken cancellationToken);

    Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);
}
