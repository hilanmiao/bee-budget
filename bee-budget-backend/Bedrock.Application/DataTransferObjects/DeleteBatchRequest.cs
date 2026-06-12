namespace Bedrock.Application.DataTransferObjects
{
    public class DeleteBatchRequest
    {
        public List<long> Ids { get; set; } = new();
    }
}