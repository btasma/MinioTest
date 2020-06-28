using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Minio;

namespace MinioTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly MinioClient _blobClient;
        private readonly string endpoint = "minio1:9000";
        private string accessKey = "minio";
        private string secretKey = "minio123";
        private string bucketName = "test1234";

        public FileController()
        {
            _blobClient = new MinioClient(endpoint, accessKey, secretKey);
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            if (!await _blobClient.BucketExistsAsync(bucketName))
                await _blobClient.MakeBucketAsync(bucketName);

            return _blobClient.ListObjectsAsync(bucketName).Select(x => x.Key).ToEnumerable();
        }

        [HttpGet("{id}")]
        public async Task<FileStreamResult> Get(string id)
        {
            if (!await _blobClient.BucketExistsAsync(bucketName))
                await _blobClient.MakeBucketAsync(bucketName);

            var ms = new MemoryStream();
            var obj = await _blobClient.StatObjectAsync(bucketName, id);
            await _blobClient.GetObjectAsync(bucketName, id, s => s.CopyTo(ms));
            ms.Position = 0;
            return new FileStreamResult(ms, obj.ContentType);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<string> Post()
        {
            if (!await _blobClient.BucketExistsAsync(bucketName))
                await _blobClient.MakeBucketAsync(bucketName);

            var id = Guid.NewGuid().ToString();
            await _blobClient.PutObjectAsync(bucketName, id, Request.Body, Request.ContentLength.Value, Request.ContentType);
            return id;
        }
    }
}
