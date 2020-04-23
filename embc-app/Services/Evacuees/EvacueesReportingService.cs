using Gov.Jag.Embc.Public.DataInterfaces;
using Gov.Jag.Embc.Public.Utils;
using Gov.Jag.Embc.Public.ViewModels.Search;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gov.Jag.Embc.Public.Services.Evacuees
{
    public class EvacueesReportingService
        : IRequestHandler<GenerateEvacueesReport, EvacueesReport>
    {
        private readonly IDataInterface dataInterface;

        public EvacueesReportingService(IDataInterface dataInterface)
        {
            this.dataInterface = dataInterface;
        }

        public async Task<EvacueesReport> Handle(GenerateEvacueesReport request, CancellationToken cancellationToken)
        {
            var evacueees = await dataInterface.GetEvacueesAsync(request.SearchCriteria);

            return new EvacueesReport
            {
                FileName = $"Evacuee_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                ContentType = "text/csv",
                Content = evacueees.ToCSV()
            };
        }
    }

    public class GenerateEvacueesReport : IRequest<EvacueesReport>
    {
        public string Format { get; set; }
        public EvacueeSearchQueryParameters SearchCriteria { get; set; }
    }

    public class EvacueesReport
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
    }
}
