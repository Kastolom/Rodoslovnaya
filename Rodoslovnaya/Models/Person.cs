using System.ComponentModel.DataAnnotations;

namespace Rodoslovnaya
{
    public class Person
    {
        public int Id { get; set; }
        
        [Required]
        public string? Name { get; set; }
        
        public int? BirthYear { get; set; }
        public int? Parent1Id { get; set; }
        public int? Parent2Id { get; set; }
    }
}