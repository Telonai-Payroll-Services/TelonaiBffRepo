namespace TelonaiWebApi.Services;

using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using System;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using System.Data;
using Amazon.CognitoIdentityProvider.Model;
using TelonaiWebApi.Entities;

public interface IUserService
{
    Task ChangePasswordAsync(string username, string oldPassword, string newPassword);
    Task<Tuple<CognitoUser, SignInManagerResponse>> LoginAsync(string username, string password, bool rememberMe);
    Task LogOutAsync();
    Task<IdentityResult> SignUpAsync(User user, UserRole userRole, int companyId, int jobId);
    Task<Microsoft.AspNetCore.Identity.SignInResult> ConfirmTwoFactorCodeAsync(TwoFactoreModel model);
    Task<string> ConfirmAccount(string username, string code);
    Task ForgotPasswordRequest(string username);
    Task ForgotPasswordResponse(string username, string code, string newPassword);
    Task<bool> CheckUsernameAvailability(string username);
    Task<bool> SendForgottenUsername(string email);
}

public class UserService : IUserService
{
    private readonly CognitoSignInManager<CognitoUser> _signInManager;
    private readonly CognitoUserManager<CognitoUser> _userManager;
    private readonly ILogger<UserService> _logger;
    private readonly CognitoUserPool _pool;
    private readonly IMailSender _mailSender;
    private readonly IPersonService<PersonModel,Person> _personService;

    public UserService(UserManager<CognitoUser> userManager, SignInManager<CognitoUser> signInManager,ILogger<UserService> logger, 
                      CognitoUserPool pool, IMailSender mailSender, IPersonService<PersonModel, Person> personService)
    {
        _userManager = userManager as CognitoUserManager<CognitoUser>;
        _logger = logger;
        _pool = pool;
        _signInManager = signInManager as CognitoSignInManager<CognitoUser>;
        _mailSender = mailSender;
        _personService = personService;
    }


    public async Task<Tuple<CognitoUser, SignInManagerResponse>> LoginAsync(string username, string password, bool rememberMe)
    {
        SignInResult result;
        try
        {
            result = await _signInManager.PasswordSignInAsync(username, password, rememberMe, lockoutOnFailure: false);
        }
        catch (UserNotConfirmedException ex)
        {
            return new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.UserIsNotConfirmed);
        }

        if (result.Succeeded)
        {
            _logger.LogInformation($"User logged in: {username}.");

            var user = await _userManager.FindByNameAsync(username);
            return new Tuple<CognitoUser, SignInManagerResponse>(user, SignInManagerResponse.LoginSucceeded);
        }
        else if (result.RequiresTwoFactor)
        {
            //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            return new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.RequiresTwoFactor);
        }
        else if (result.IsCognitoSignInResult())
        {
            if (result is CognitoSignInResult cognitoResult)
            {
                if (cognitoResult.RequiresPasswordChange)
                {
                    _logger.LogWarning("User password needs to be changed");
                    //return RedirectToPage("./ChangePassword");
                    return new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.RequiresPasswordChange);
                }
                else if (cognitoResult.RequiresPasswordReset)
                {
                    _logger.LogWarning("User password needs to be reset");
                    //return RedirectToPage("./ResetPassword");
                    return new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.RequiresPasswordReset);
                }
            }

        }
        return new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.InvalidCredentials);
    }

    public async Task<SignInResult> ConfirmTwoFactorCodeAsync(TwoFactoreModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync() ??
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.RespondToTwoFactorChallengeAsync(authenticatorCode, model.RememberMe.Value, model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.UserID);
            return result;
        }
        else
        {
            _logger.LogWarning("Invalid 2FA code entered for user with ID '{UserId}'.", user.UserID);
            return result;
        }
    }

    public async Task ForgotPasswordRequest(string username)
    {
        var user = await _userManager.FindByNameAsync(username) ?? throw new AppException("Invalid Username");        
        await user.ForgotPasswordAsync();             
    }
    public async Task ForgotPasswordResponse(string username,  string code, string newPassword)
    {
        var user = await _userManager.FindByNameAsync(username) ?? throw new AppException("Invalid Username");
        await user.ConfirmForgotPasswordAsync(code, newPassword);
    }
    public async Task ChangePasswordAsync(string username, string oldPassword, string newPassword)
    {

        var result = await _signInManager.PasswordSignInAsync(username, oldPassword, false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(username);
            await user.ChangePasswordAsync(oldPassword, newPassword);
        }
    }

    public async Task<IdentityResult> SignUpAsync(User inputUser, UserRole role,  int companyId, int jobId)
    {
        if (companyId == 0)
            throw new InvalidOperationException("Invalid company id.");

        var existingUser = await _userManager.FindByIdAsync(inputUser.Username);
        CognitoUser user = null;
        if (existingUser == null)
        {
            return await CreateUser(user, inputUser, companyId, jobId, role.ToString());
        }
        else if (existingUser.Attributes[CognitoAttribute.Email.AttributeName] != inputUser.Email)
        {
            throw new AppException($"Username {inputUser.Username} is taken by someone else. Please provide another username.");
        }

        //Here means I have already signed up through current company or another company
        var existingScope = existingUser.Attributes["custom:scope"];
        var currentCompanyScope = $"C{companyId}Role";
        var alreadySignedUpThroughThisCompany = existingScope.Contains(currentCompanyScope);

        user = _pool.GetUser(inputUser.Username);
        if (alreadySignedUpThroughThisCompany)
        {
            //I need to be an Admin to get here
            if (role == UserRole.User)
                throw new AppException("You are not authorized by your company to perform this operation.");

            currentCompanyScope += "Admin";
            if (existingScope.Contains(currentCompanyScope + "J0") && jobId != 0)
            {
                existingScope = existingScope.Replace(currentCompanyScope + "J0", $"{currentCompanyScope}J{jobId}");
                user.Attributes.Add("custom:scope", existingScope);
                return await _userManager.UpdateAsync(user);
            }
        }
        else //This means I am new at the current company
        {
            if (role == UserRole.User && jobId<1) //JobId==0 means employer is not creating the company profile
                throw new AppException("You are not authorized by your company to perform this operation.");

            var newScope = $"C{companyId}Role{role}J{jobId}," + currentCompanyScope;
            newScope = existingScope.Replace(currentCompanyScope, newScope);

            user.Attributes.Add("custom:scope", newScope);
            return await _userManager.UpdateAsync(user);
        }

        return new IdentityResult();
    }

    public async Task<string> ConfirmAccount(string username, string code)
    {
        // Retrieves a new user with the pool configuration set up
        CognitoUser user = _pool.GetUser(username);

        var result = await _userManager.ConfirmSignUpAsync(user, code, true);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Description.Contains("please request a code again")))
            {
                result = await _userManager.ResendSignupConfirmationCodeAsync(user);
                if (result.Succeeded)
                    return "NewCodeSent";
            }
            throw new AppException(result.Errors.Select(e => e.Description).Aggregate("", (current, next) => current + ", " + next));
        }
        return "Success";
    }

    public async Task LogOutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");       
    }

    public async Task<bool> SendForgottenUsername(string email)
    {
        //Fetch user information using registered email address.
        var user = await _userManager.FindByEmailAsync(email) ?? throw new AppException("Invalid Username");
        if(user != null)
        {
            //Get person detail information using email address.
            var person = await _personService.GetByEmailAsync(email);
            var fullname = $"{person.FirstName} {person.LastName}";
            string subject = "Forget username response";
            //Sending username via provided email address.
            await _mailSender.SendUsingAwsClientAsync(email, subject, CreateHtmlEmailBody(fullname,user.Username), CreateTextEmailBody(fullname, user.Username));
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> CheckUsernameAvailability(string username)
    {
        var userName = await _userManager.FindByNameAsync(username);        
        var isUsernameExist =  (userName != null) ? true : false;
        return isUsernameExist;
    }

    private async Task<IdentityResult> CreateUser(CognitoUser user, User inputUser, int companyId, int jobId, string role)
    {
        // Retrieves a new user with the pool configuration set up
        user = _pool.GetUser(inputUser.Username);

        user.Attributes.Add(CognitoAttribute.Email.AttributeName, inputUser.Email);
        user.Attributes.Add(CognitoAttribute.GivenName.AttributeName, inputUser.Firstname);
        user.Attributes.Add(CognitoAttribute.FamilyName.AttributeName, inputUser.Lastname);

        //Scope definition: C{companyId}Role{Admin or User}J{jobId} 
        var scope = $"C{companyId}Role{role}";
        scope += jobId > 0 ? "J" + jobId.ToString() : "J0";
        user.Attributes.Add("custom:scope", scope);

        if (!string.IsNullOrWhiteSpace(inputUser.MobilePhone))
        {
            user.Attributes.Add(CognitoAttribute.PhoneNumber.AttributeName, inputUser.MobilePhone);
        }

        // Registers the user in the pool
        var result = await _userManager.CreateAsync(user, inputUser.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        return result;
    }

    private static string CreateTextEmailBody(string fullName, string username)
    {
        return $"Dear {fullName} \r\n" +
              "We received a request to retrieve the username associated with this email address. \r\n" +
              $"Your username is: { username} \r\n" +
              $"If you did not request this information, please ignore this email.Your account remains secure, and no further action is required. \r\n" +
              $"If you have any questions or need further assistance, please contact our support team at service@telonai.com .\r\n" +
              "\r\n Thank you for choosing Telonai. \r\n" +
              "Telonai Payroll Services Inc.";
    }

    private static string CreateHtmlEmailBody(string fullName, string username)
    {
       return $"<h1>Dear {fullName}</h1>" +
              $"<br/>We received a request to retrieve the username associated with this email address." +
              $"<br/><p>Your username is: <b>{username}</b></p>" +
              $"<br/>If you did not request this information, please ignore this email.Your account remains secure, and no further action is required." +
              $"<br/>If you have any questions or need further assistance, feel free to contact our support team at service@telonai.com ." +
              $"<br/>Thank you for choosing Telonai., " +
              $"<br/>Telonai Payroll Services Inc.";
    }
}
