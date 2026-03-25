namespace Skorbord.API.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public int Age { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Team Team { get; set; } = null!;
    }
}
