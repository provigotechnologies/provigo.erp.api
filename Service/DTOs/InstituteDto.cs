namespace InstituteService.DTOs
{
    public class InstituteDto
    {
        public int? Id { get; set; }   // null when creating new
        public string Name { get; set; }
        public string LogoPath { get; set; } = "";
        public string Address { get; set; } = "";
    }
}
