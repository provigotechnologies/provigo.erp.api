namespace InstituteService.DTOs
{
    public class InstituteDto
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public bool IsActive { get; set; }

    }

    public class InstituteResponseDto
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string LogoUrl { get; set; } = "";
        public bool IsActive { get; set; }
    }

    //public class InstituteUpdateDto
    //{
    //    public string Name { get; set; } = "";
    //    public string Phone { get; set; } = "";
    //    public string Email { get; set; } = "";
    //    public string Address { get; set; } = "";
    //    public string LogoUrl { get; set; } = "";
    //    public bool IsActive { get; set; }
    //}
}
