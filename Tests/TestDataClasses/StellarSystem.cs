using Thorium.Core.DataIntegration.Attributes;

namespace Tests.TestDataClasses
{
    public class StellarSystem
    {
        [FixedWidthField(20)]
        public string RecordType { get; set; } = "STELLARSYSTEM";
        [FixedWidthField(20)]
        public string StarType { get; set; }
        [FixedWidthField(10)]
        public bool IsBinarySystem { get; set; }
        [FixedWidthField(20)]
        public string Name { get; set; }
        [FixedWidthField(10)]
        public double Radius { get; set; }
    }
}