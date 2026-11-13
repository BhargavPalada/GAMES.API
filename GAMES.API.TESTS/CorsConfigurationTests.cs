using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GAMES.API.TESTS
{
    public class CorsConfigurationTests
    {
        [Fact]
        public void CorsPolicy_AllowAll_IsConfiguredWithPermissiveRules()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var serviceProvider = services.BuildServiceProvider();
            var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>().Value;

            // Act
            var policy = corsOptions.GetPolicy("AllowAll");

            // Assert
            Assert.NotNull(policy);
            Assert.True(policy.AllowAnyOrigin);
            Assert.True(policy.AllowAnyMethod);
            Assert.True(policy.AllowAnyHeader);
        }

        [Fact]
        public async Task CorsPolicyProvider_ReturnsAllowAllPolicy()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var serviceProvider = services.BuildServiceProvider();
            var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();

            // Act
            var httpContext = new DefaultHttpContext();
            var policy = await policyProvider.GetPolicyAsync(httpContext, "AllowAll");

            // Assert
            Assert.NotNull(policy);
            Assert.True(policy.AllowAnyOrigin);
            Assert.True(policy.AllowAnyMethod);
            Assert.True(policy.AllowAnyHeader);
        }
    }
}
