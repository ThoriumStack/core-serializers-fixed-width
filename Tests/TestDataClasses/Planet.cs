using Thorium.Core.DataIntegration.Attributes;

namespace Tests.TestDataClasses
{
    public class Planet
    {
        [FixedWidthField(20)]
        public string RecordType { get; set; } = "PLANET";
        [FixedWidthField(15)]
        public string Name { get; set; }
        [FixedWidthField(3)]
        public int OrderFromSun { get; set; }
        /// <summary>
        /// In millions of kilometers
        /// </summary>
        [FixedWidthField(20)]
        public double DistanceFromSun { get; set; }
    }
}