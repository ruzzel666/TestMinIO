using Minio;
using Minio.DataModel.Args;

namespace TestMinIO.Services
{
    public class FileStorageService
    {
        private readonly IMinioClient _minioClient;

        public FileStorageService(IMinioClient minioClient)
        {
            _minioClient = minioClient;
        }

        public async Task UploadAsync(string fileName, string bucketName, Stream stream, string contentType)
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
        }

        public async Task<Stream> DownloadAsync(string fileName, string bucketName)
        {
            var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithCallbackStream((stream) =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(getObjectArgs);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<IEnumerable<object>> GetFilesListAsync(string bucketName)
        {
            var listArgs = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(true);

            var files = new List<object>();

            var items = _minioClient.ListObjectsEnumAsync(listArgs);

            await foreach (var item in items)
            {
                files.Add(new
                {
                    Name = item.Key,
                    Size = item.Size,
                    LastModified = item.LastModifiedDateTime
                });
            }

            return files;
        }

        public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiryInSeconds = 3600)
        {
            var presignedArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(expiryInSeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedArgs);
        }
    }
}
