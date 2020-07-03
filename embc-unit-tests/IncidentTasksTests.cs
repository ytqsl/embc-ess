using AutoFixture;
using AutoMapper;
using Gov.Jag.Embc.Public;
using Gov.Jag.Embc.Public.DataInterfaces;
using Gov.Jag.Embc.Public.ViewModels;
using Gov.Jag.Embc.Public.ViewModels.Search;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace embc_unit_tests
{
    public class IncidentTasksTests : TestBase
    {
        private IDataInterface di => Services.ServiceProvider.GetService<IDataInterface>();

        public IncidentTasksTests(ITestOutputHelper output, EmbcWebApplicationFactory<Startup> webApplicationFactory) : base(output, webApplicationFactory)
        {
        }

        [Fact]
        public async Task Create_IncidentTask_Saved()
        {
            var fixture = new Fixture();

            var task = fixture.Build<IncidentTask>()
                .Without(t => t.Id)
                .Without(t => t.Region)
                .With(t => t.Community, await GetRandomSeededCommunity())
                .Create();

            var taskId = await di.CreateIncidentTaskAsync(task);

            var result = await di.GetIncidentTaskAsync(taskId);

            Assert.NotNull(result);
            Assert.Equal(task.Active, result.Active);
            Assert.NotNull(result.Community);
            Assert.Equal(task.Community.Id, result.Community.Id);
            Assert.Null(result.Region);
            Assert.Equal(task.StartDate, result.StartDate);
            Assert.Equal(task.TaskNumber, result.TaskNumber);
            Assert.Equal(0, result.TotalAssociatedEvacuees);
        }

        [Fact]
        public void CanMapNullCorrectly()
        {
            Gov.Jag.Embc.Public.Models.Db.IncidentTask source = new Gov.Jag.Embc.Public.Models.Db.IncidentTask
            {
                Id = Guid.NewGuid(),
                Region = new Gov.Jag.Embc.Public.Models.Db.Region { Active = false, Name = "test" },
                Community = null
            };

            var mapper = Services.ServiceProvider.GetService<IMapper>();
            var executionPlan = mapper.ConfigurationProvider.BuildExecutionPlan(typeof(Gov.Jag.Embc.Public.Models.Db.IncidentTask), typeof(IncidentTask));
            var result = mapper.Map<IncidentTask>(source);

            Assert.Equal(source.Id.ToString(), result.Id);
            Assert.Equal(source.Region.Active, result.Region.Active);
            Assert.Equal(source.Region.Name, result.Region.Name);
            Assert.Null(result.Community);
        }

        [Fact]
        public void CanMapRegionCorrectly()
        {
            var source = new Gov.Jag.Embc.Public.Models.Db.Region { Active = true, Name = "test" };

            var result = Services.ServiceProvider.GetService<IMapper>().Map<Region>(source);

            Assert.Equal(source.Active, result.Active);
            Assert.Equal(source.Name, result.Name);
        }

        [Fact]
        public async Task CanSearchTasks()
        {
            var numberOfActiveTasks = 6;
            var numberOfInactiveTasks = 7;
            var fixture = new Fixture();

            var taskBuilder = fixture.Build<IncidentTask>()
               .Without(t => t.Id)
               .Without(t => t.Region)
               .Without(t => t.StartDate)
               .Without(t => t.TaskNumberStartDate)
               .Without(t => t.TaskNumberStartDate)
               .With(t => t.Community, await GetRandomSeededCommunity())
               .With(t => t.Active, true);

            //inactive tasks
            var startDate = DateTime.Parse("2020-01-01 13:00");
            var endDate = DateTime.Parse("2020-01-03 17:00");
            for (int i = 0; i < numberOfInactiveTasks; i++)
            {
                var task = taskBuilder.Create();
                task.StartDate = startDate;
                task.TaskNumberStartDate = startDate;
                task.TaskNumberEndDate = endDate;
                await di.CreateIncidentTaskAsync(task);
            }

            //active tasks
            startDate = DateTime.Now.AddDays(-1);
            endDate = startDate.AddDays(3).AddHours(5);
            for (int i = 0; i < numberOfActiveTasks; i++)
            {
                var task = taskBuilder.Create();
                task.StartDate = startDate;
                task.TaskNumberStartDate = startDate;
                task.TaskNumberEndDate = endDate;
                await di.CreateIncidentTaskAsync(task);
            }

            var allTasks = await di.GetIncidentTasksAsync(new IncidentTaskSearchQueryParameters { ActiveTasks = null });
            Assert.Equal(numberOfInactiveTasks + numberOfActiveTasks, allTasks.Items.Count());

            var activeTasks = await di.GetIncidentTasksAsync(new IncidentTaskSearchQueryParameters { ActiveTasks = true });
            Assert.Equal(numberOfActiveTasks, activeTasks.Items.Count());

            var inactiveTasks = await di.GetIncidentTasksAsync(new IncidentTaskSearchQueryParameters { ActiveTasks = false });
            Assert.Equal(numberOfInactiveTasks, inactiveTasks.Items.Count());
        }
    }
}