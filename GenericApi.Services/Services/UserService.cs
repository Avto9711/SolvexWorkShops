﻿using AutoMapper;
using FluentValidation;
using GenericApi.Bl.Dto;
using GenericApi.Core.Settings;
using GenericApi.Model.Entities;
using GenericApi.Model.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GenericApi.Services.Services
{ 
    public interface IUserService : IBaseService<User, UserDto>  {
        Task<AuthenticateResponseDto> GetToken(AuthenticateRequestDto model);
    }
    public class UserService : BaseService<User, UserDto>, IUserService
    {
        private readonly JwtSettings _jwtSettings;
        public UserService(
            IUserRepository repository, 
            IMapper mapper, 
            IValidator<UserDto> validator, IOptions<JwtSettings> jwtSettings) : base(repository, mapper, validator)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthenticateResponseDto> GetToken(AuthenticateRequestDto model)
        {
            var user = await _repository.Query()
                .Where(x => x.UserName == model.UserName && x.Password == model.Password)
                .Select(x=> new { 
                    x.Id,
                    x.UserName,
                    x.Name,
                    x.LastName
                })
                .FirstOrDefaultAsync();

            if (user is null)
                return null;

            var response = new AuthenticateResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                LastName = user.LastName
            };

            response.Token = GenerateJwtToken(response);

            return response;
        }
        private string GenerateJwtToken(AuthenticateResponseDto user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var symmetricSecurityKey = new SymmetricSecurityKey(key);

            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var claimsIdentity = new ClaimsIdentity(new[] { 
                new Claim("id", user.Id.ToString()),
                new Claim("username",user.UserName)
            });

            var claims = new Dictionary<string, object>
            {
                { "name", user.Name },
                { "lastName", user.LastName },
            };

            var description  = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                SigningCredentials = signingCredentials
            };

            var handler = new JwtSecurityTokenHandler();

            var securityToken = handler.CreateToken(description);

            var token = handler.WriteToken(securityToken);

            return token;
        }
    }
}