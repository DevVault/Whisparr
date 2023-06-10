using FluentValidation;

using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TPDb
{
    public class CustomSettingsValidator : AbstractValidator<TPDbPerformerSettings>
    {
        public CustomSettingsValidator()
        {
            RuleFor(c => c.PerformerId).GreaterThan(0);
        }
    }

    public class TPDbPerformerSettings : IImportListSettings
    {
        private static readonly CustomSettingsValidator Validator = new CustomSettingsValidator();

        public TPDbPerformerSettings()
        {
            BaseUrl = "https://api.whisparr.com/v2";
        }

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Performer Id", HelpText = "The TPDb Performer Id")]
        public int PerformerId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
