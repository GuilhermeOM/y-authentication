using System.Text;
using Y.Authentication.Domain.Aggregates.User;

namespace Y.Authentication.UnitTest.Fixtures;
internal class UserFixture
{
    public static User CreateValid()
    {
        var byteArrayMock = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());

        var userResult = User.Create(
            email: "dummy@dummy.com",
            passwordHash: byteArrayMock,
            passwordSalt: byteArrayMock,
            name: "Dummy SurDummy",
            birthDate: DateOnly.MinValue,
            roleId: Guid.NewGuid());

        return userResult.IsSuccess
            ? userResult.Value
            : throw new InvalidOperationException("Could not create a valid user for the fixture.");
    }
}
