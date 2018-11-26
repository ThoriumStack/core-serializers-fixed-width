using Thorium.Core.DataIntegration.Attributes;

namespace Tests.TestDataClasses
{
    public class Galaxy
    {
        [FixedWidthField(20)]
        public string RecordType { get; set; } = "GALAXY";
        [FixedWidthField(20)]
        public string Name { get; set; } = "Milky Way";
        [FixedWidthField(20)]
        public string Type { get; set; } = "Spiral";
        [FixedWidthField(20)]
        public string Address { get; set; } = "Local Group";

    }
}