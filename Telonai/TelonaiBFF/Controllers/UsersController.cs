﻿namespace TelonaiWebApi.Controllers;

using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using TelonaiWebApi.Entities;
using Amazon.Extensions.CognitoAuthentication;
using System.Text.RegularExpressions;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class UsersController : Controller
{    
    private readonly IInvitationService<InvitationModel, Invitation> _invitationService;
    private readonly IUserService _userService;
    private readonly IEmploymentService<EmploymentModel, Employment> _employmentService;
    private readonly IPersonService<PersonModel, Person> _personService;
    private readonly ITimecardUsaService _timecardService;
    private readonly IDayOffRequestService<DayOffRequestModel, DayOffRequest> _dayOffRequestService;

    public UsersController(IUserService userService, IPersonService<PersonModel, Person> personService, IInvitationService<InvitationModel, 
        Invitation> invitationService, IEmploymentService<EmploymentModel, Employment> employmentService, ITimecardUsaService timecardService
        ,IDayOffRequestService<DayOffRequestModel, DayOffRequest> dayOffRequestService)
    {
        _userService = userService;
        _personService = personService;
        _invitationService = invitationService;
        _employmentService = employmentService;
        _timecardService = timecardService;
        _dayOffRequestService = dayOffRequestService;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(BaseUser user)
    {
        Tuple <CognitoUser, SignInManagerResponse> result = null;
        LoginResult loginResult = new();

        if (ModelState.IsValid)
        {
            try
            {
                result = _userService.LoginAsync(user.Username, user.Password, user.RememberMe.Value).Result;
            }
            catch (HttpErrorResponseException ex)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");

                loginResult.Error = SignInManagerResponse.InvalidCredentials;
                await _userService.LogOutAsync();
                return Ok(loginResult);
            }
            if (result.Item2 ==  SignInManagerResponse.InvalidCredentials)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                
                loginResult.Error = SignInManagerResponse.InvalidCredentials;
                await  _userService.LogOutAsync();
                return Ok(loginResult);
            }
            else if (result.Item1==null)
            {
                loginResult.Error = result.Item2;
                return Ok(loginResult);
            }

            var email = result.Item1.Attributes["email"];
            var employments = _employmentService.GetByEmail(email).ToList();
            var personIds = employments.Select(e => e.PersonId).Distinct().ToList();
            var dayOffs = await GetPendingDayOffsAsync(employments);

            if (employments != null && employments.Count > 0)
            {
                loginResult.FullName = employments.First().Person;
                loginResult.OpenTimeCard = _timecardService.GetOpenTimeCard(personIds);
                loginResult.Employments = employments;
                loginResult.DayOffRequests = dayOffs;                
            }

            return Ok(loginResult);
        }

        loginResult.Error = SignInManagerResponse.UnknownError;
        return Ok(loginResult);
    }

    [HttpPost("changepassword")]
    public async Task<IActionResult> ChangePassword(UserChangePasswordModel user)
    {

        if (ModelState.IsValid && user != null)
        {
            var result = await _userService.ChangePasswordAsync(user.Username, user.OldPassword, user.NewPassword);
            if(result)
            {
                return Ok("Your password changed successfully");
            }
            else
            {
                return BadRequest();
            }
        }
        else
        {
            return BadRequest();
        }
    }


    [HttpPost("AccessDenied")]
    public IActionResult AccessDenied()
    {

        return Forbid();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            _userService.LogOutAsync();
        }
        catch (Exception ex)
        {
            throw;
        }

        return Ok();
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(User user)
    {
        if (ModelState.IsValid)
        {
            var invitation = _invitationService.GetAllByActivationCodeAndInviteeEmail2(user.ActivationCode, user.Email);

            var result = await _userService.SignUpAsync(user,UserRole.User, invitation.Job.CompanyId, invitation.JobId.Value);
            if (result.Succeeded)
            {

                var person = await CreateProfileFromUserAsync(user, invitation.Job.CompanyId);
                var emp = await CreateEmploymment(invitation.JobId.Value, person.Id, invitation.Job.CompanyId,(int)SignUpStatusTypeModel.UserProfileCreationStarted   );

                invitation.ExpirationDate = DateTime.UtcNow;
                await _invitationService.UpdateAsync(invitation);

                var loginResult = new LoginResult
                {  
                    Employments = new List<EmploymentModel> { emp },
                    FullName = $"{person.FirstName} {person.LastName}"
                };                

                return Ok(loginResult);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

        }
        return BadRequest();
    }

    [HttpPost("fp/{username}")]
    public async Task<IActionResult> FpRequest(string username)
    {
        if (ModelState.IsValid)
            await _userService.ForgotPasswordRequest(username);
        return new NoContentResult();
    }

    [HttpPost("fpr")]
    public async Task<IActionResult> FpRequest(PasswordChangeModel model)
    {
        if (ModelState.IsValid)
            await _userService.ForgotPasswordResponse(model.Username,model.Code,model.NewPassword);
        return new NoContentResult();
    }

    [HttpPost("confirmTfa")]
    public async Task<IActionResult> ConfirmTwoFactorCodeAsync(TwoFactoreModel user)
    {
        //var returnUrl = Url.Content("~/");
        if (ModelState.IsValid)
        {
            var result = await _userService.ConfirmTwoFactorCodeAsync(user);
            if (result.Succeeded)
            {
                return Ok(true);
            }
            ModelState.AddModelError(string.Empty, "Invalid 2FA code.");        
        }
        return BadRequest();
    }

    [HttpPost("confirmAccount/{username}/{code}")]
    public async Task<IActionResult> ConfirmAccountAsync(string username, string code)
    {

        if (ModelState.IsValid)
        {
            var result = await _userService.ConfirmAccount(username, code);
            return Ok(result);
        }

        return BadRequest();
    }
    
    [HttpPost("{username}")]
    public async Task<IActionResult> CheckUserNameAvailability(string username)
    {
        if (!string.IsNullOrEmpty(username))
        {
            if (await _userService.CheckUsernameAvailability(username))
            {
                return BadRequest("Invalid");
            }
            else
            {
                return Ok();
            }
        }
        else
        {
            return Ok("Please provide the username to check whether it is available or not");
        }
    }

    [HttpPost("email/{email}")]
    public async Task<IActionResult> ForgetUsername(string email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (Regex.IsMatch(email, pattern))
            {
                var forgetUsernameResult = await _userService.SendForgottenUsername(email);
                if (forgetUsernameResult)
                {
                    return Ok("Your username was delivered to your email address.Check your email, please.");
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                throw new ApplicationException("Please enter a valid email address.");
            }
        }
        else
        {
            throw new ApplicationException("Please enter email address.");
        }

    }


    private async Task<List<DayOffRequestModel>> GetPendingDayOffsAsync(List<EmploymentModel> employments)
    {
        var result= new List<DayOffRequestModel>();
        foreach (var item in employments.Where(e => e.IsPayrollAdmin))
        {
            result.AddRange(_dayOffRequestService.GetPendingRequestsByCompanyId(item.Job.CompanyId));
        }
        return result;
    }

    private async Task<PersonModel> CreateProfileFromUserAsync(User user, int companyId)
    {
        var p = new Person
        {
            FirstName = user.Firstname,
            LastName = user.Lastname,
            MiddleName = user?.Middlename,
            Email = user?.Email,
            MobilePhone = user?.MobilePhone,
            CompanyId = companyId,
            //BankAccountNumber = user?.BankAccountNumber,
            //RoutingNumber = user?.RoutingNumber,
            INineVerificationStatusId = (int)INineVerificationStatusModel.INineNotSubmitted
        };
        var exstingPerson=await _personService.GetByEmailAsync(user?.Email);
        if (exstingPerson != null) 
        {
            p.FirstName = exstingPerson?.FirstName;
            p.LastName = exstingPerson?.LastName;
            p.MiddleName = exstingPerson?.MiddleName;
        }
        
        return await _personService.CreateAsync(p);
    }

    private async Task<EmploymentModel> CreateEmploymment(int jobId,int personId,int companyId, int statusId)
    {
        var emp = new Employment
        {
            JobId = jobId,
            IsPayrollAdmin = false,
            PersonId = personId,
            SignUpStatusTypeId=statusId
        };
        return await _employmentService.CreateAsync(emp, companyId );       
    }
}