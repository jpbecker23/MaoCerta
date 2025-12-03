namespace MaoCerta.Web.ViewModels
{
    public class ProfessionalDetailViewModel
    {
        public int ProfessionalId { get; set; }
    }

    public class ServiceRequestViewModel
    {
        public int? ProfessionalId { get; set; }
        public string? ProfessionalName { get; set; }
    }

    public class ReviewFormViewModel
    {
        public int? ProfessionalId { get; set; }
        public int? ServiceRequestId { get; set; }
    }
}

