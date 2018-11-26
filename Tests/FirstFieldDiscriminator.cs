using Thorium.Core.DataIntegration.Attributes;

namespace Tests
{
        public class FirstFieldDiscriminator<TDiscriminatorType>
        {
            [FixedWidthField(1)] 
            public TDiscriminatorType Value { get; set; }
        }
    }
