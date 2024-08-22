﻿using financial_manager.Entities;
using financial_manager.Models;
using financial_manager.Repositories.Interfaces;
using financial_manager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace financial_manager.Repositories
{
    public class TokenRepositoty : ITokenRepository
    {
        private readonly FinancialManagerContext _financialManagerContext;
        public TokenRepositoty(
            FinancialManagerContext financialManagerContext,
            )
        {
            _financialManagerContext = financialManagerContext;
        }

        public async Task CreateTokenAsync(Token token)
        {
            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                throw new ArgumentNullException(nameof(token.RefreshToken));
            }

            _financialManagerContext.Tokens.Add(new TokenEntity
            {
                RefreshToken = token.RefreshToken,
                ExpirationDate = token.ExpirationDate,
                UserId = token.User!.Id,
                IsRevoked = token.IsRevoked
            });
            await _financialManagerContext.SaveChangesAsync();
        }

        public async Task<Token> GetTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentNullException("Invalid refresh token");
            }

            Token? authToken = await _financialManagerContext.Tokens
                .Include(at => at.User)
                .Where(at => at.RefreshToken == refreshToken)
                .Select(at => new Token
                {
                    Id = at.Id,
                    RefreshToken = at.RefreshToken,
                    ExpirationDate = at.ExpirationDate,
                    IsRevoked = at.IsRevoked,
                    User = new User
                    {
                        Id = at.User.Id,
                        FullName = at.User.FullName,
                        Email = at.User.Email,
                    }
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (authToken is null)
            {
                throw new NullReferenceException("Auth token does not exist");
            }

            return authToken;
        }
    }
}
