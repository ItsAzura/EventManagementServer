using System.ComponentModel.DataAnnotations;

namespace EventManagementServer.Dto
{
    public class CategoryDto
    {

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = String.Empty;

        public string CategoryDescription { get; set; } = String.Empty;
    }
}
