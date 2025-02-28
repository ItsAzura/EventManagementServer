namespace EventManagementServer.Dto
{
    public class LogoutRequestDto
    {
        public int UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}
