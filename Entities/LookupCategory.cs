namespace FilmMaker.Entities
{
    public class LookupCategory : SharedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<LookupItem> LookupItems { get; set; }
    }
}
