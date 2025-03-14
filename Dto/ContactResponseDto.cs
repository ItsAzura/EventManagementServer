namespace EventManagementServer.Dto
{
    public class ContactResponseDto
    {
        public int Id { get; set; }
        public string ResponseMessage { get; set; } = String.Empty;
        public string AdminEmail { get; set; } = String.Empty;
    }
}
