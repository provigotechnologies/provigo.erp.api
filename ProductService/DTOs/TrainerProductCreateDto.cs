namespace ProductService.DTOs
{
    public class TrainerProductCreateDto
    {
        public Guid BranchId { get; set; }
        public Guid TrainerId { get; set; }
        public List<int> ProductIds { get; set; }
    }
}
