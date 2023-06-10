using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.TPDb;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.TPDb
{
    public class TPDbPerformerSettingsValidatorFixture : CoreTest
    {
        [TestCase("0")]
        [TestCase(null)]
        public void invalid_performerId_should_not_validate(int performerId)
        {
            var setting = new TPDbPerformerSettings
            {
                PerformerId = performerId,
            };

            setting.Validate().IsValid.Should().BeFalse();
            setting.Validate().Errors.Should().Contain(c => c.PropertyName == "PerformerId");
        }

        [TestCase("1")]
        [TestCase("82885")]
        [TestCase("164502")]
        public void valid_performerId_should_validate(int performerId)
        {
            var setting = new TPDbPerformerSettings
            {
                PerformerId = performerId,
            };

            setting.Validate().IsValid.Should().BeTrue();
            setting.Validate().Errors.Should().NotContain(c => c.PropertyName == "PerformerId");
        }
    }
}
