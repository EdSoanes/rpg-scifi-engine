using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigill.Common.OpenApi
{
    public class SigillDocumentProcessor : IDocumentProcessor
    {
        private readonly string _title;
        private readonly string _description;

        public SigillDocumentProcessor(string title, string description)
        {
            _title = title;
            _description = description;
        }

        public void Process(DocumentProcessorContext context)
        {
            context.Document.Info.Title = _title;
            context.Document.Info.Description = _description;
        }
    }
}
