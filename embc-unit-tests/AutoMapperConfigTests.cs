using AutoMapper;
using Gov.Jag.Embc.Public;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace embc_unit_tests
{
    public class AutoMapperConfigTests : TestBase
    {
        public AutoMapperConfigTests(ITestOutputHelper output, EmbcWebApplicationFactory<Startup> webApplicationFactory) : base(output, webApplicationFactory)
        {
        }

        [Fact]
        public void AssertConfig()
        {
            Services.ServiceProvider.GetService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}