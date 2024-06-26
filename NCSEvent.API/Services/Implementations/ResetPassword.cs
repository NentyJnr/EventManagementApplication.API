﻿//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.Extensions.Hosting;
//using Microsoft.IdentityModel.Tokens;
//using NCSEvent.API.Commons.Responses;
//using NCSEvent.API.Entities;

//namespace NCSEvent.API.Services.Implementations
//{
//    public class ResetPassword : IResetPassword
//    {
//        private readonly UserManager<Users> _userManager;
//        private readonly IEmailService _emailService;
//        private readonly IHttpContextAccessor _httpContext;
//        private readonly AppDbContext _context;
//        private readonly EmailHelper _emailHelper;
//        private readonly EmailServiceBinding _emailServiceBinding;
//        public readonly IHostEnvironment _hostEnvironment;
//        public ResetPassword(
//            IHttpContextAccessor httpContext,
//            UserManager<Users> userManager,
//            IEmailService emailService,
//            AppDbContext context,
//            EmailServiceBinding emailServiceBinding,
//            EmailHelper emailHelper,
//            IHostEnvironment hostEnvironment)
//        {
//            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
//            _emailService = emailService;
//            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
//            _context = context;
//            _emailServiceBinding = emailServiceBinding;
//            _emailHelper = emailHelper;
//            _hostEnvironment = hostEnvironment;
//        }

//        public async Task<ServerResponse<string>> GeneratePasswordResetOTP(string email)
//        {
//            var response = new ServerResponse<string>();
//            if (email == null)
//            {
//                response.IsSuccessful = false;
//                response.Data = null;

//                return response;
//            }
//            var user = await _userManager.FindByEmailAsync(email);

//            if (user == null)
//            {
//                response.IsSuccessful = false;
//                response.Data = null;

//                return response;
//            }

//            var otp = GenerateRandomOTP();

//            var otpRecord = new OTPs
//            {
//                OTP = Convert.ToString(otp),
//                Email = user.Email,
//                OTPType = "ResetPassword"
//            };
//            _context.OTPs.Add(otpRecord);
//            await _context.SaveChangesAsync();

//            string htmlPath = _hostEnvironment.ContentRootPath + Path.DirectorySeparatorChar + "EmailTemplates/SendPasswordOTPTemplate.html";
//            string htmlContent = Convert.ToString(Utilities.ReadHtmlFile(htmlPath));
//            var body = htmlContent.Replace("{USERNAME}", user.FullName).Replace("{OTP}", otp);
//            var emailPayLoad = new EmailServiceModel
//            {
//                from = _emailServiceBinding?.Sender ?? "",
//                messageBody = body, //$"Kindly find your password :{Password} and user name {request.Email} Link {ActivationLink}",
//                projectCode = _emailServiceBinding?.ProjectCode ?? "",
//                to = user.Email,
//                sentNow = true,
//                subject = "Reset Password",
//                recieverName = user.FirstName,
//                scheduleDate = DateTime.Now,
//                senderName = _emailServiceBinding?.SenderName ?? "N/A",



//            };
//            // var emailSuccess = _emailService.SendPasswordResetOTP(user.Email, user.FirstName, otp);

//            var emailSuccess = await _emailHelper.SendMail(emailPayLoad);

//            if (emailSuccess.statusCode == "200")
//            {
//                await _trans.CommitAsync();
//                response.IsSuccessful = true;
//                response.Data = null;
//                SetSuccess(response, "Successful", ResponseCodes.SUCCESS, _language);
//            }
//            else
//            {
//                response.IsSuccessful = false;
//                response.Data = null;
//                return SetError(response, ResponseCodes.REQUEST_NOT_SUCCESSFUL, _language);
//            }

//            return response;
//        }

//        public async Task<ServerResponse<bool>> ResetPasswordWithOTP(OTPDto model)
//        {
//            var response = new ServerResponse<bool>();

//            if (!model.IsValid(out ValidationResponse source, _messageProvider, _language))
//            {
//                response.IsSuccessful = false;
//                response.Data = false;
//                SetErrorValidation(response, source.Code, source.Message);

//                return response;
//            }

//            var user = await _userManager.FindByEmailAsync(model.Email);

//            if (user == null)
//            {
//                response.IsSuccessful = false;
//                response.Data = false;
//                SetError(response, ResponseCodes.RECORD_DOES_NOT_EXISTS, _language);
//                return response;
//            }

//            var otpRecord = await _context.OTPs.FirstOrDefaultAsync(o => o.Email == model.Email && o.OTP == model.OTP);

//            if (otpRecord == null)
//            {
//                response.IsSuccessful = false;
//                response.Data = false;
//                response.Error = new ErrorResponse
//                {
//                    ResponseCode = ResponseCodes.INVALID_OBJECT_MAPPING,
//                    ResponseDescription = "Invalid or expired OTP"
//                };
//                return response;
//            }

//            if (otpRecord.OTPType == "ResetPassword")
//            {

//                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

//                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

//                if (resetPasswordResult.Succeeded)
//                {
//                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
//                    await _userManager.UpdateAsync(user);
//                }
//                else
//                {
//                    response.IsSuccessful = false;
//                    response.Data = false;
//                    response.Error = new ErrorResponse
//                    {
//                        ResponseCode = ResponseCodes.INVALID_OBJECT_MAPPING,
//                        ResponseDescription = "Wrong OTP Type"
//                    };

//                    return response;
//                }

//                _context.OTPs.Remove(otpRecord);
//                await _context.SaveChangesAsync();
//            }
//            else
//            {
//                response.IsSuccessful = false;
//                response.Data = false;
//                response.Error = new ErrorResponse
//                {
//                    ResponseCode = ResponseCodes.INVALID_OBJECT_MAPPING,
//                    ResponseDescription = "Invalid OTP type"
//                };
//            }

//            var emailSuccess = _emailService.ResetPasswordEmail(user.Email, user.FirstName);

//            if (emailSuccess)
//            {
//                await _trans.CommitAsync();
//                response.IsSuccessful = true;
//                response.Data = true;
//                SetSuccess(response, true, ResponseCodes.SUCCESS, _language);
//            }
//            else
//            {
//                response.IsSuccessful = false;
//                response.Data = false;
//                SetError(response, ResponseCodes.REQUEST_NOT_SUCCESSFUL, _language);
//            }

//            return response;
//        }

//        private string GenerateRandomOTP()
//        {
//            Random random = new Random();
//            return random.Next(100000, 999999).ToString("D6");
//        }
//    }
//}
