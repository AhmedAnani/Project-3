using AutoMapper;
using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Application.Mappings;
using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Services;

public class UserService : IUserService
{
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepo,
            IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }


        public async Task<UserProfileDto?> GetCurrentUserAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);

            if (user == null)
                return null;

            return _mapper.MapToUserProfile(user);
        }


        public async Task<List<UserProfileDto>> GetAllUsersAsync(
            CancellationToken ct = default)
        {
            var users = await _userRepo.GetAllUsersAsync(ct);

            return _mapper.Map<List<UserProfileDto>>(users);
        }


        public async Task<List<UserProfileDto>> GetUsersByRoleAsync(
            UserRole role,
            CancellationToken ct = default)
        {
            var users = await _userRepo.GetUsersByRoleAsync(role, ct);

            return _mapper.Map<List<UserProfileDto>>(users);
        }


        public async Task PromoteToAdminAsync(
            Guid userId,
            Guid adminId,
            CancellationToken ct = default)
        {
            if (userId == adminId)
                throw new InvalidOperationException(
                    "Cannot modify your own role");


            var user = await _userRepo.GetByIdAsync(userId, ct);

            if (user == null)
                throw new KeyNotFoundException("User not found");


            if (user.Role == UserRole.Admin)
                throw new InvalidOperationException(
                    "User is already admin");


            user.Role = UserRole.Admin;

            await _userRepo.UpdateAsync(user, ct);
            await _userRepo.SaveChangesAsync(ct);
        }


        public async Task DemoteFromAdminAsync(
            Guid userId,
            Guid adminId,
            CancellationToken ct = default)
        {
            if (userId == adminId)
                throw new InvalidOperationException(
                    "Cannot modify your own role");


            var user = await _userRepo.GetByIdAsync(userId, ct);

            if (user == null)
                throw new KeyNotFoundException("User not found");


            if (user.Role == UserRole.User)
                throw new InvalidOperationException(
                    "User is already normal user");


            user.Role = UserRole.User;

            await _userRepo.UpdateAsync(user, ct);
            await _userRepo.SaveChangesAsync(ct);
        }


        public async Task DeleteUserAsync(
            Guid userId,
            Guid adminId,
            CancellationToken ct = default)
        {
            if (userId == adminId)
                throw new InvalidOperationException(
                    "Cannot delete your own account");


            var user = await _userRepo.GetByIdAsync(userId, ct);

            if (user == null)
                throw new KeyNotFoundException("User not found");


            user.IsDeleted = true;

            await _userRepo.UpdateAsync(user, ct);
            await _userRepo.SaveChangesAsync(ct);
        }
    }