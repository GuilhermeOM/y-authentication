using System.Text;
using Y.Authentication.Domain.Aggregates.Role;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.UnitTest.Fixtures;
internal class UserFixture
{
    public static User CreateValid()
    {
        var byteArrayMock = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());

        var userResult = User.Create(
            new PasswordHash(byteArrayMock, byteArrayMock),
            email: "dummy@dummy.com",
            name: "Dummy SurDummy",
            birthDate: DateOnly.MinValue,
            roleId: Guid.NewGuid());

        return userResult.IsSuccess
            ? userResult.Value
            : throw new InvalidOperationException("Could not create a valid user for the fixture.");
    }

    public static User CreateValidWithRoles()
    {
        var user = CreateValid();
        HydrateRoles(user);
        return user;
    }

    private static void HydrateRoles(User user)
    {
        foreach (var userRole in user.Roles)
        {
            var roleResult = Role.Create("Admin");
            if (roleResult.IsSuccess)
            {
                userRole.Role = roleResult.Value;
            }
        }
    }
}
