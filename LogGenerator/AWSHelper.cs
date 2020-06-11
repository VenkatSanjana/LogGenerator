using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LogGenerator
{
    public class AWSHelper
    {
        /// <summary>
        /// Sends the response to the queue details supplied as parameters
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="responseQueueUrl"></param>
        /// <param name="responseRegion"></param>
        /// <returns></returns>
        public static bool SendResponseToSQSQueue(string accessKey, string secureAccessKey, string responseMessage, string responseQueueUrl, string responseRegion)
        {
            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = responseQueueUrl,
                    MessageBody = responseMessage
                };
                if (responseQueueUrl.ToLower().Contains(".fifo"))
                {
                    sendMessageRequest.MessageGroupId = "MessageGroupId";
                    using (SHA256 hash = SHA256Managed.Create())
                    {
                        sendMessageRequest.MessageDeduplicationId
                            = String.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(responseMessage)).Select(item => item.ToString("x2")));
                    }
                }

                AmazonSQSClient sqsRespClient = new AmazonSQSClient(accessKey, secureAccessKey, RegionEndpoint.GetBySystemName(responseRegion));
                sqsRespClient.SendMessage(sendMessageRequest);
                sqsRespClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Uploads the response to S3 bucket supplied as parameter
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="content"></param>
        /// <param name="bucketName"></param>
        /// <param name="bucketRegion"></param>
        /// <returns></returns>
        public static bool SendResponseToS3Bucket(string accessKey, string secureAccessKey, string keyName, string content, string bucketName, string bucketRegion)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest()
                {
                    ContentBody = content,
                    BucketName = bucketName,
                    Key = keyName
                };
                request.Metadata.Add("title", keyName);
                AmazonS3Client s3Client = new AmazonS3Client(accessKey,
                    secureAccessKey, RegionEndpoint.GetBySystemName(bucketRegion));
                PutObjectResponse response = s3Client.PutObject(request);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
