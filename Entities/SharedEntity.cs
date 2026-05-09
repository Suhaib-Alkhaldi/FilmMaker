namespace FilmMaker.Entities
{
    public class SharedEntity
    {
        public int Id { get; set; }
        public string? CreatedBy { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
