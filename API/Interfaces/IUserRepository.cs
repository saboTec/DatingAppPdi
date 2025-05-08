using System;
using API.DTO;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    void update(AppUser appUser);
    Task<bool> SaveAllAsync();
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserById(int id);
    Task<AppUser?> GetUserByUserNameAsync(string username);

    Task<IEnumerable<MemberDto>> GetMembersAsync();
    Task<MemberDto?> GetMemberAsync(string username);





    


}
