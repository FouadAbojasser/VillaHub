using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using VillaHub.Domain.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VillaHub.Web.ViewModels.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace VillaHub.Web.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        // Injecting Identity Helpers
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TwilioService _twilioService;

        public AccountController(
                        UserManager<ApplicationUser> userManager,
                        SignInManager<ApplicationUser> signInManager,
                        RoleManager<IdentityRole> roleManager,
                        IEmailSender emailSender, IUnitOfWork unitOfWork,
                        TwilioService twilioService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _twilioService = twilioService;
        }


        public async Task<IActionResult> RegisterAsync(string returnUrl = null!)
        {
            if (returnUrl == null)
                returnUrl = Url.Content("~/");

            //must be in dB initializer not in any action
            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_SuperAdmin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
            }

            RegisterVM registerVM = new()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),

                RedirectUrl = returnUrl,

                CountryList = SD.CountryList.Select(c => new SelectListItem
                {
                    Text = c.Text,
                    Value = c.Text 

                }).ToList()

            };

            return View(registerVM);
        }


        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            var countryPrefix = SD.CountryList
            .FirstOrDefault(c => c.Text == registerVM.Country)?.Value ?? "";

            ApplicationUser applicationUser = new ApplicationUser()
            {
                Name = registerVM.Name,
                UserName=registerVM.Email,
                Email = registerVM.Email,
                PhoneNumber = countryPrefix + registerVM.PhoneNumber,
                Country = registerVM.Country,
                CreatedAt= DateTime.UtcNow,
            };

            
            var result = await _userManager.CreateAsync(applicationUser, registerVM.Password);

            if (result.Succeeded)
            {
                if (!applicationUser.EmailConfirmed)
                {
                    //Generating User Token
                    string userToken = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                    //Generating Confirmation Link
                    var link = Url.Action("ConfirmEmail", "Account", new {applicationUser.Id, userToken}, Request.Scheme);

                    //Generating HTML Confirmation Message
                    string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "email-templates", "confirm.html");
                    string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
                    emailBody = emailBody.Replace("{{UserName}}", applicationUser.Name)
                                         .Replace("{{ConfirmationLink}}", link);

                    //Sending Confirmation Email
                    await _emailSender.SendEmailAsync(applicationUser.Email, "Confirmation Email", emailBody);

                    TempData["success"] = "User Registered Successfully!, Please Confirm Your Email";

                     
                    if (string.IsNullOrEmpty(registerVM.Role))
                    {
                        // Give the user Default Role = Customer
                        await _userManager.AddToRoleAsync(applicationUser, SD.Role_Customer);
                    }
                    else
                    { 
                        await _userManager.AddToRoleAsync(applicationUser, registerVM.Role); 
                    }
                        

                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                else
                {
                    TempData["error"] = "You Have Perviuosly Confirmed Your Email Address!!";
                }


            }
            else
            {
                foreach (var err in result.Errors)
                {

                    ModelState.AddModelError(string.Empty, err.Description);
                }
            }
            registerVM.RoleList = _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });
            registerVM.CountryList = SD.CountryList;
            return View(registerVM);

        }


        public IActionResult Login(string returnUrl = null!)
        {
            if(returnUrl == null)
            returnUrl = Url.Content("~/");

            LoginVM loginVM = new()
            {
                RedirectUrl = returnUrl
            };

            return View(loginVM);
        }


        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            var applicationUser = await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);

            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail);
            }

            if (applicationUser is not null)
            {
                //UserName or Email Found
                //Check Password
                var result = await _userManager.CheckPasswordAsync(applicationUser, loginVM.Password);

                if (result)
                {
                    // Correct Password
                    // Check if the Email is Confirmed ot not?
                    if (applicationUser.EmailConfirmed)
                    {
                        await _signInManager.SignInAsync(applicationUser, loginVM.RememberMe);

                        TempData["success"] = "Login Successfully";

                        var chk1 = await _userManager.IsInRoleAsync(applicationUser, SD.Role_Admin);
                        var chk2 = await _userManager.IsInRoleAsync(applicationUser, SD.Role_SuperAdmin);

                        if (chk1 || chk2)
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "" });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home", new { area = "" });
                        }
                    }
                    else
                    {
                        TempData["error"] = "Please Confirm Your Email First!";

                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }
                else
                {
                    //Wrong Password
                    ModelState.AddModelError("Password", "Invalid Password!");
                    return View(loginVM);
                }

            }

            ModelState.AddModelError("UserNameOrEmail", "Invalid User Name or Email!");

            return View(loginVM);
        }


        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();

            TempData["success"] = "Logout Successfully!";

            return RedirectToAction("Index", "Home", new {area = "" });
        }


        public IActionResult ResendConfirmEmail()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> ResendConfirmEmailAsync(ResendConfirmationEmailVM confirmEmailVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (confirmEmailVM.Email is not null)
            {
                ApplicationUser applicationUser = await _userManager.FindByEmailAsync(confirmEmailVM.Email);

                if (applicationUser != null)
                {
                    if (!applicationUser.EmailConfirmed)
                    {
                        //Generating User Token
                        string userToken = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                        //Generating Confirmation Link
                        var link = Url.Action("ConfirmEmail", "Account", new {applicationUser.Id, userToken}, Request.Scheme);

                        //Generating HTML Confirmation Message
                        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "email-templates", "confirm.html");
                        string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
                        emailBody = emailBody.Replace("{{UserName}}", applicationUser.Name)
                                             .Replace("{{ConfirmationLink}}", link);

                        //Sending Confirmation Email
                        await _emailSender.SendEmailAsync(confirmEmailVM.Email, "Resend Confirmation Email", emailBody);

                        TempData["success"] = "Confirmation Emial Sent Successfully!, Please Check Your Email";

                       return RedirectToAction("Index", "Home" , new { area = "" });
                    }
                    else
                    {
                        TempData["error"] = "You Email is Already Confirmed!";
                    }
                }
                else
                {
                    TempData["error"] = "User Not Found!";
                }
            }

            return RedirectToAction(nameof(Login));
        }


        public async Task<IActionResult> ConfirmEmailAsync(string Id, string UserToken)
        {
            var applicationUser = await _userManager.FindByIdAsync(Id);

            if (applicationUser is null)
            {
                //User Not Found
                return RedirectToAction("Index", "Home", new { area = "" });

            }

            if (applicationUser.EmailConfirmed)
            {
                TempData["info"] = "Your Email is Already Confirmed!";

                return RedirectToAction("Index", "Home", new { area = "" });

            }
            var result = await _userManager.ConfirmEmailAsync(applicationUser, UserToken);

            if (result.Succeeded)
            {
                TempData["success"] = "Email Confirmed Successfully!";

                return RedirectToAction("Index", "Home", new { area = "" });

            }
            else
            {
                TempData["error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return BadRequest();
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordVM resetPasswordRequestVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordRequestVM);
            }

            var applicationUser = await _userManager.FindByNameAsync(resetPasswordRequestVM.UserNameOrEmail);

            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByEmailAsync(resetPasswordRequestVM.UserNameOrEmail);
            }

            if (applicationUser is null)
            {
                ModelState.AddModelError(string.Empty, "Username or Email does not exist!");

                return View(resetPasswordRequestVM);
            }
            else
            {
                if (resetPasswordRequestVM.ResetMethod.Contains("OTP"))
                {
                    var userLastOTP = _unitOfWork.OTP.Get(e => e.ApplicationUserId == applicationUser.Id).LastOrDefault();

                    if (userLastOTP is null)
                    {
                        //User Has never User OTP
                        int GenOTP = new Random().Next(1000, 9999);
                        //Needed for ResetPassword
                        string token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                        //Add OTP to Database
                        await _unitOfWork.OTP.CreateAsync(new OTP()
                        {
                            OTP_Number = GenOTP,
                            ApplicationUserId = applicationUser.Id,
                            RequestDateTime = DateTime.UtcNow,
                            ExpairationDateTime = DateTime.UtcNow.AddMinutes(30),
                            UsedByUser = false

                        });
                        await _unitOfWork.OTP.CommitAsync();

                        if(resetPasswordRequestVM.ResetMethod.Contains("WhatsApp")){

                            var WhatsAppMessage = $"Your OTP is {GenOTP.ToString()}";

                            await _twilioService.SendWhatsAppMessage(applicationUser.PhoneNumber!, WhatsAppMessage);

                            TempData["success"] = "Password Reset has been requested successfully. Please check your WhatsApp for OTP.";
                        }
                        else
                        {
                            //Generating HTML OTP Message
                            string templatePathOTP = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "email-templates", "PasswordResetOTP.html");
                            string emailBodyOTP = await System.IO.File.ReadAllTextAsync(templatePathOTP);
                            emailBodyOTP = emailBodyOTP.Replace("{{UserName}}", applicationUser.Name)
                                                 .Replace("{{YourOTP}}", GenOTP.ToString());

                            //Sending Confirmation Email
                            await _emailSender.SendEmailAsync(applicationUser.Email!, "Reset Password Email", emailBodyOTP);

                            TempData["success"] = "Password Reset has been requested successfully!";
                        }
                        
                        TempData["_validationToken"] = Guid.NewGuid().ToString();

                        return RedirectToAction("NewPasswordOTP", "Account", new { area = "Identity", Token = token, ApplicationUserId = applicationUser.Id});
                    }

                    else if ((DateTime.UtcNow - userLastOTP.RequestDateTime).TotalMinutes > 30)
                    {
                        //User Has never User OTP
                        int GenOTP = new Random().Next(1000, 9999);
                        //Needed for ResetPassword
                        string token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                        //Add OTP to Database
                        await _unitOfWork.OTP.CreateAsync(new OTP()
                        {
                            OTP_Number = GenOTP,
                            ApplicationUserId = applicationUser.Id,
                            RequestDateTime = DateTime.UtcNow,
                            ExpairationDateTime = DateTime.UtcNow.AddMinutes(30),
                            UsedByUser = false

                        });
                        await _unitOfWork.OTP.CommitAsync();

                        //Sending WhatsApp OTP
                        if (resetPasswordRequestVM.ResetMethod.Contains("WhatsApp"))
                        {
                            var WhatsAppMessage = $"Your OTP is {GenOTP.ToString()}";
                            await _twilioService.SendWhatsAppMessage(applicationUser.PhoneNumber!, WhatsAppMessage);

                            TempData["success"] = "Password Reset has been requested successfully. Please check your WhatsApp for OTP.";
                        }
                        else
                        {
                            //Sending OTP via Email
                            //Generating HTML OTP Message
                            string templatePathOTP = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "email-templates", "PasswordResetOTP.html");
                            string emailBodyOTP = await System.IO.File.ReadAllTextAsync(templatePathOTP);
                            emailBodyOTP = emailBodyOTP.Replace("{{UserName}}", applicationUser.Name)
                                                 .Replace("{{YourOTP}}", GenOTP.ToString());

                            await _emailSender.SendEmailAsync(applicationUser.Email!, "Reset Password Email", emailBodyOTP);

                            TempData["success"] = "Password Reset has been requested successfully. Please check your Email for OTP.";

                        }

                        TempData["_validationToken"] = Guid.NewGuid().ToString();

                        return RedirectToAction("NewPasswordOTP", "Account", new { area = "Identity", Token = token, ApplicationUserId = applicationUser.Id});

                    }

                    else if ((DateTime.UtcNow - userLastOTP!.RequestDateTime).TotalMinutes < 30)
                    {

                        var remainingTime = TimeSpan.FromMinutes(30) - (DateTime.UtcNow - userLastOTP!.RequestDateTime);

                        ModelState.AddModelError(string.Empty, $"You can use OTP after {remainingTime.ToString("mm\\:ss")} mm:ss!");

                        return View(resetPasswordRequestVM);
                    }
                }

                else if (resetPasswordRequestVM.ResetMethod == "ConfirmationLink")
                {
                    //Using Token
                    string token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);

                    var ResetPasswordLink = Url.Action("NewPasswordLink", "Account", new {area = "Identity", Token = token, ApplicationUserId = applicationUser.Id}, Request.Scheme);


                    //Generating HTML Confirmation Message
                    string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "email-templates", "PasswordResetLink.html");
                    string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
                    emailBody = emailBody.Replace("{{UserName}}", applicationUser.Name)
                                         .Replace("{{ConfirmationLink}}", ResetPasswordLink);

                    //Sending Confirmation Email
                    await _emailSender.SendEmailAsync(applicationUser.Email!, "Reset Password Email", emailBody);

                    TempData["success"] = "Password Reset has been requested successfully!";

                    //used to prevent accessing Password reset page from the link directly
                    TempData["_validationToken"] = Guid.NewGuid().ToString();

                }

                return View(nameof(CheckYourEmail));

            }
        }


        public IActionResult CheckYourEmail()
        {
            return View();
        }


        public IActionResult NewPasswordLink(NewPasswordLinkVM newPasswordLinkVM)
        {
           
            if (TempData["_validationToken"] is not null)
            {
                return View(newPasswordLinkVM);
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> NewPasswordLinkAsync(NewPasswordLinkVM newPasswordLinkVM)
        {
            if (!ModelState.IsValid)
            {
                return View(newPasswordLinkVM);
            }

            var applicationUser = await _userManager.FindByIdAsync(newPasswordLinkVM.ApplicationUserId);

            if (applicationUser is not null)
            {
                var result = await _userManager.ResetPasswordAsync(applicationUser, newPasswordLinkVM.Token, newPasswordLinkVM.NewPassword);

                if (result.Succeeded)
                {
                    TempData["success"] = "Yor Password has been reset Successfully!";

                    return RedirectToAction("Index", "Home", new { area = "" });

                }
                else
                {
                    TempData["error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }

            return BadRequest();

        }


        public IActionResult NewPasswordOTP(NewPasswordOTPVM newPasswordOTPVM)
        {
            ModelState.Remove("OTP");
            if (TempData["_validationToken"] is not null)
            {
                return View(newPasswordOTPVM);
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> NewPasswordOTPAsync(NewPasswordOTPVM newPasswordOTPVM)
        {

            if (!ModelState.IsValid)
            {
                return View(newPasswordOTPVM);
            }

            var applicationUser = await _userManager.FindByIdAsync(newPasswordOTPVM.ApplicationUserId);

            if (applicationUser is not null)
            {
                var OTPinDB = _unitOfWork.OTP.Get(e => e.ApplicationUserId == newPasswordOTPVM.ApplicationUserId).LastOrDefault();

                if (OTPinDB != null && OTPinDB.OTP_Number == newPasswordOTPVM.OTP && DateTime.UtcNow < OTPinDB.ExpairationDateTime)
                {
                    var result = await _userManager.ResetPasswordAsync(applicationUser, newPasswordOTPVM.Token, newPasswordOTPVM.NewPassword);

                    if (result.Succeeded)
                    {
                        TempData["success"] = "Yor Password has been reset Successfully!";

                        OTPinDB.UsedByUser = true;

                        _unitOfWork.OTP.Update(OTPinDB);

                        await _unitOfWork.OTP.CommitAsync();

                        return RedirectToAction(nameof(Login), "Account", new {area = "Identity"});
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }


                }
                else if (OTPinDB != null && OTPinDB.OTP_Number == newPasswordOTPVM.OTP && DateTime.UtcNow > OTPinDB.ExpairationDateTime)
                {
                    ModelState.AddModelError(string.Empty, "OTP Expired!");

                    return View(newPasswordOTPVM);
                }
                else if (OTPinDB != null && OTPinDB.OTP_Number != newPasswordOTPVM.OTP)
                {
                    ModelState.AddModelError(string.Empty, "Invalid OTP!");

                    return View(newPasswordOTPVM);
                }

            }

            return View(newPasswordOTPVM);

        }




        //External Login Helper Methods
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null!)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null!, string remoteError = null!)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in if user already exists
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
            if (result.Succeeded)
            {
                TempData["success"] = $"Login Successful Using {info.LoginProvider}";
                return LocalRedirect(returnUrl ?? "/");
            }

            // User does not exist, ask for confirmation
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var model = new ExternalLoginConfirmationVM
            {
                Email = email!,
                CountryList = SD.CountryList.Select(c => new SelectListItem
                {
                    Text = c.Text,
                    Value = c.Text
                }).ToList()
            };

            return View("ExternalLoginConfirmation", model);
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationVM model, string returnUrl = null!)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                // Get the selected country's phone prefix
                var countryPrefix = SD.CountryList.FirstOrDefault(c => c.Text == model.Country)?.Value ?? "";

                var applicationUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Name = info.Principal.Identity!.Name!,
                    Email = model.Email,
                    Country = model.Country,
                    PhoneNumber = countryPrefix + model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(applicationUser);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(applicationUser, SD.Role_Customer);
                    result = await _userManager.AddLoginAsync(applicationUser, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(applicationUser, isPersistent: false);
                        TempData["success"] = $"User Account Created Using {info.LoginProvider}";
                        return LocalRedirect(returnUrl ?? "/");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Repopulate CountryList if something failed
            model.CountryList = SD.CountryList.Select(c => new SelectListItem
            {
                Text = c.Text,
                Value = c.Text
            }).ToList();

            return View(model);
        }



    }
}
