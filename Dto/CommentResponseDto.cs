namespace EventManagementServer.Dto
{
    public class CommentResponseDto
    {
        public int CommentID { get; set; }
        public int EventID { get; set; }
        public int UserID { get; set; }
        public string Content { get; set; }
        public DateTime CreateAt { get; set; }
        public SimpleEventDto Event { get; set; }
        public SimpleUserDto User { get; set; }
    }

    public class SimpleEventDto
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
    }

    public class SimpleUserDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}


