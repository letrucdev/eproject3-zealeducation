using ZealEducation.Infrastructure.Services;

namespace ZealEducation.Infrastructure.IntegrationTests.Services;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_produces_different_value_for_same_password_each_call()
    {
        var h1 = _hasher.Hash("Secret123!");
        var h2 = _hasher.Hash("Secret123!");

        h1.Should().NotBeNullOrEmpty();
        h2.Should().NotBeNullOrEmpty();
        h1.Should().NotBe(h2, because: "BCrypt uses a random salt per hash");
    }

    [Fact]
    public void Verify_returns_true_when_password_matches_hash()
    {
        var hash = _hasher.Hash("Secret123!");
        _hasher.Verify("Secret123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_returns_false_when_password_does_not_match()
    {
        var hash = _hasher.Hash("Secret123!");
        _hasher.Verify("WrongPwd!", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_returns_false_when_hash_is_malformed()
    {
        _hasher.Verify("anything", "not-a-real-bcrypt-hash").Should().BeFalse();
    }
}
