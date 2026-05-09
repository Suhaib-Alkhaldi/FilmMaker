using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ProductionCompanyProductionType : SharedEntity
    {
        public int ProductionCompanyProfileId { get; set; }

        [ForeignKey("ProductionCompanyProfileId")]
        public ProductionCompanyProfile ProductionCompanyProfile { get; set; } = null!;
        public int ProductionTypeId { get; set; }

        [ForeignKey("ProductionTypeId")]
        public LockupItem ProductionType { get; set; } = null!;
    }
}
