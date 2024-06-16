using Rpg.Cms.Services.Factories;
using Rpg.ModObjects.Json;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services.Synchronizers
{
    public class DataTypeSynchronizer : IDataTypeSynchronizer
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IDataTypePresentationFactory _dataTypePresentationFactory;
        private readonly IDataTypeContainerService _dataTypeContainerService;
        private readonly DataTypeModelFactory _dataTypeModelFactory;

        public DataTypeSynchronizer(
            IDataTypeService dataTypeService, 
            IDataTypePresentationFactory dataTypePresentationFactory, 
            IDataTypeContainerService dataTypeContainerService,
            DataTypeModelFactory dataTypeModelFactory)
        {
            _dataTypeService = dataTypeService;
            _dataTypePresentationFactory = dataTypePresentationFactory;
            _dataTypeContainerService = dataTypeContainerService;
            _dataTypeModelFactory = dataTypeModelFactory;
        }

        public async Task<IEnumerable<IDataType>> GetDataTypesAsync(SyncSession session)
        {
            var res = new List<IDataType>();

            var types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.PlainInteger);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.Integer);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText);
            res.AddRange(types);

            res = res
                .Where(x => x.Name?.StartsWith(session.System.Identifier) ?? false)
                .ToList();

            return res;
        }

        public async Task<List<IDataType>> Sync(SyncSession session)
        {
            var res = await GetDataTypesAsync(session);
            foreach (var metaProp in session.System.Objects.SelectMany(x => x.Props))
            {
                var name = session.GetDataTypeName(metaProp.Prop);
                if (!res.Any(x => x.Name == name))
                {
                    var dataTypeModel = _dataTypeModelFactory.ToDataTypeModel(session, metaProp, session.RootDataTypeFolder!);
                    var presAttempt = await _dataTypePresentationFactory.CreateAsync(dataTypeModel);
                    if (!presAttempt.Success)
                        throw new InvalidOperationException($"Failed to create datatype presentation for {name}");

                    var dataTypeAttempt = await _dataTypeService.CreateAsync(presAttempt.Result, session.UserKey);
                    if (!dataTypeAttempt.Success)
                        throw new InvalidOperationException($"Failed to create datatype for {name}");

                    res.Add(dataTypeAttempt.Result);
                }
            }

            return res.ToList();
        }
    }
}
