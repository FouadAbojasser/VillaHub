using System.ComponentModel.DataAnnotations;

namespace VillaHub.Web.ViewModels.ContactUs
{
    public class ContactUsVM
    {
        //[Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        //[Display(Name = "Name", ResourceType = typeof(Resources.SharedResources))]
        //public string Name { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "InvalidEmail")]
        [Display(Name = "Email", ResourceType = typeof(Resources.SharedResources))]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [MaxLength(50, ErrorMessage = "Maximum length is 50 characters!")]
        [Display(Name = "Subject", ResourceType = typeof(Resources.SharedResources))]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Required")]
        [MaxLength(200, ErrorMessage = "Maximum length is 200 characters!")]
        [Display(Name = "Message", ResourceType = typeof(Resources.SharedResources))]
        public string Message { get; set; } = string.Empty;
    }
}
