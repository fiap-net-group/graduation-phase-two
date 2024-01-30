using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBlog.NewsManager.API.Application.Mapper;

namespace TechBlog.NewsManager.Tests.Application.Mapper
{
    public class StrategyMapperTests
    {
        [Fact]
        public void AutoMapper_CaseConfiguration_IsValid()
        {
            // Arrange
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<StrategyMapper>();
            });

            // Act & Assert
            configuration.AssertConfigurationIsValid();
        }
    }
}
