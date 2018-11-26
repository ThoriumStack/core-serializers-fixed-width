using Thorium.Core.DataIntegration.Attributes;

namespace Tests.TestDataClasses
{
    public class AstronomyFixedWidthDiscriminator
    {
        [FixedWidthField(20)]
        public string FieldType { get; set; }
    }
}