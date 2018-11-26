using System;
using Thorium.Core.DataIntegration.Attributes;

namespace Tests.TestDataClasses
{
    public class ChemicalElement
    {
        [FixedWidthField(20)]
        public string Name { get; set; }
        [FixedWidthField(5)]
        public string Symbol { get; set; }
        [FixedWidthField(5)]
        public int AtomicNumber { get; set; }
        [FixedWidthField(20)]
        [DateTimeFormat("yyyyMMdd")]
        public DateTime DiscoveryDate { get; set; }

    }
}