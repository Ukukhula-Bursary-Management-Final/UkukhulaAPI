﻿using BusinessLogic.Models;
using BusinessLogic.Models.Response;
using DataAccess;
using DataAccess.Entity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using System.Transactions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLogic
{
    public class UserBLL
    {
        private UserDAL _userDAL;
        private IConfiguration _configuration;

        public UserBLL(UserDAL userDAL, IConfiguration configuration)
        {
            _configuration = configuration;
            _userDAL = userDAL;
            _configuration = configuration;
        }

        public async Task<UserManagerResponse> LoginUserAsync(Login model)
        {

            User user =_userDAL.getUserByEmail(model.Email);

            if (user == null) { 
             UserManagerResponse response = new UserManagerResponse
             {
                 Message = $"No user found with email:{model.Email}",
                 isSuccess = false,
                 
             };

                return response;

            }

            if(user.Status=="INACTIVE"){
                UserManagerResponse response = new UserManagerResponse
             {
                 Message = $"The account with email:{model.Email} has been deactivated",
                 isSuccess = false,
                 
             };
                return response;
            }

            string RoleName = _userDAL.getRoleNameByUserID(user.ID);

         


            Claim[] claims = new[]
            {
                new Claim("Email", model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Role, RoleName)
            };
            

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            int UniversityID = 0;

            if (RoleName == Roles.UniversityAdmin)
            {
                UniversityID = _userDAL.getUniverstityIdForUser(user.ID);
            }
            return new UserManagerResponse
            {
                Message = tokenString,
                isSuccess = true,
                ExpireDate = token.ValidTo,
                Id = user.ID,
                Role = RoleName,
                UniversityID = UniversityID,
                
            };
        }

        public UserManagerResponse ProcessRegistration(Register model,int UniversityID)
        {
            if (model == null)
            {
                return new UserManagerResponse
                {
                    Message = "Unable to register user, no information provided",
                    isSuccess = false
                };
              
            }

                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))//Need to add option to identify async functions
                {
                    ContactDetails contacts = new ContactDetails
                    {
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                    };
                    int contactId = _userDAL.InsertContactsAndGetPrimaryKey(contacts);
                    if(contactId==0){
                    return new UserManagerResponse
                    {
                        Message = "Error inserting into contacts table",
                        isSuccess = false
                    };
                    }
                    User user = new User
                    {
                        ContactID = contactId,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                    };
                    int userId = _userDAL.InsertUserAndGetPrimaryKey(user);

                    _userDAL.InsertToUserRole(userId, model.Role);

                    //should insert into university user when neccesary
                    if (model.Role =="University Admin"){
                    _userDAL.insertIntoUniversityUser(UniversityID,userId);
                    }else{
                        //insert to
                    }


                    scope.Complete();
                    return new UserManagerResponse
                    {
                        Message = $"User created. Username: {model.Email}",
                        isSuccess = true
                    };
                }
            }
            

        public async Task<User> getUser(string email)
        {
            return _userDAL.getUserByEmail(email);
        }
    }
}