using System;
using System.Collections.Generic;
using System.IO;
using Thorium.Core.DataIntegration;
using Thorium.Core.DataIntegration.Transports;
using Thorium.Core.Serializers.FixedWidthSerializer;
using Tests.TestDataClasses;
using Xunit;

namespace Tests
{
    public class SerializerTests
    {
       
        [Fact]
        public void TestFixedWidthFormatter()
        {
            var elems = new List<ChemicalElement>()
            {
                new ChemicalElement { Name = "Hydrogen", AtomicNumber = 1, DiscoveryDate = new DateTime(1766,5,16), Symbol = "H"},
                new ChemicalElement { Name = "Phosporous", AtomicNumber = 15, DiscoveryDate = new DateTime(1669,7,17), Symbol = "P"},
                new ChemicalElement { Name = "Cobalt", AtomicNumber = 27, DiscoveryDate = new DateTime(1732,10,11), Symbol = "Co"}
            };

            
            var serializer = new FixedWidthSerializer();

            var outputBuilder = new OutputBuilder()
                .SetSerializer(new FixedWidthSerializer());
            
            File.Delete("chemistry.txt");
            
            var transport = new LocalFileTransport()
            {
                FilePath = @"chemistry.txt"
            };
            
            var integ = new Integrator();
            
            integ.SendData(outputBuilder, transport);
            
            
            return;
        }
        
        [Fact]
        public void BuilderTestFixedWidth()
        {
            var star = new StellarSystem()
            {
                IsBinarySystem = false,
                Name = "Sol",
                StarType = "Yellow Dwarf",
                Radius = 695.700
            };
            var planets = new List<Planet>()
            {
                new Planet() { Name="Mercury", DistanceFromSun = 57.91, OrderFromSun = 1 },
                new Planet() { Name="Venus", DistanceFromSun = 108.2, OrderFromSun = 2 },
                new Planet() { Name="Earth", DistanceFromSun = 149.6, OrderFromSun = 3 },
                new Planet() { Name="Mars", DistanceFromSun = 227.9, OrderFromSun = 4 },
                new Planet() { Name="Jupiter", DistanceFromSun = 778.5, OrderFromSun = 5 },
                new Planet() { Name="Saturn", DistanceFromSun = 1429, OrderFromSun = 6 },
                new Planet() { Name="Uranus", DistanceFromSun = 2877, OrderFromSun = 7 },
                new Planet() { Name="Neptune", DistanceFromSun = 4498, OrderFromSun = 8 },
            };
            var serializer = new FixedWidthSerializer();
            
            File.Delete("estplanetsfixedwithmulti.txt");
            var transport = new LocalFileTransport { FilePath = $"testplanetsfixedwithmulti.txt" };
            var build = new OutputBuilder()
                .SetSerializer(serializer)
                .AddData(star)
                .AddListData(planets)
                // .SetSerializer(new DataIntegrationJsonSerializer())
                .AddData(new Galaxy() { Name = "Alpha Centauri" });

            var integ = new Integrator();
            
            integ.SendData(build, transport);
            
            //  var result = a.SendAsyncData(build, transport);
            return ;
        }

        [Fact]
        public void BuilderTestFixedWidthDelimiter()
        {
            var star = new StellarSystem()
            {
                IsBinarySystem = false,
                Name = "Sol",
                StarType = "Yellow Dwarf",
                Radius = 695.700
            };
            var planets = new List<Planet>()
            {
                new Planet() { Name="Mercury", DistanceFromSun = 57.91, OrderFromSun = 1 },
                new Planet() { Name="Venus", DistanceFromSun = 108.2, OrderFromSun = 2 },
                new Planet() { Name="Earth", DistanceFromSun = 149.6, OrderFromSun = 3 },
                new Planet() { Name="Mars", DistanceFromSun = 227.9, OrderFromSun = 4 },
                new Planet() { Name="Jupiter", DistanceFromSun = 778.5, OrderFromSun = 5 },
                new Planet() { Name="Saturn", DistanceFromSun = 1429, OrderFromSun = 6 },
                new Planet() { Name="Uranus", DistanceFromSun = 2877, OrderFromSun = 7 },
                new Planet() { Name="Neptune", DistanceFromSun = 4498, OrderFromSun = 8 },
            };
            var serializer = new FixedWidthSerializer()
            {
                Delimiter = ","
            };

            File.Delete("estplanetsfixedwithmulti.txt");
            var transport = new LocalFileTransport { FilePath = $"testplanetsfixedwithmulti.txt" };
            var build = new OutputBuilder()
                .SetSerializer(serializer)
                .AddData(star)
                .AddListData(planets)
                // .SetSerializer(new DataIntegrationJsonSerializer())
                .AddData(new Galaxy() { Name = "Alpha Centauri" });

            var integ = new Integrator();

            integ.SendData(build, transport);

            //  var result = a.SendAsyncData(build, transport);
            return;
        }

       
    }
}