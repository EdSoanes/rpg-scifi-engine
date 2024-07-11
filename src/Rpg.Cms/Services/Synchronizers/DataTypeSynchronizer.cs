using Rpg.Cms.Services.Factories;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services.Synchronizers
{
    public class DataTypeSynchronizer : IDataTypeSynchronizer
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IDataTypePresentationFactory _dataTypePresentationFactory;
        private readonly DataTypeModelFactory _dataTypeModelFactory;

        public DataTypeSynchronizer(
            IDataTypeService dataTypeService, 
            IDataTypePresentationFactory dataTypePresentationFactory, 
            DataTypeModelFactory dataTypeModelFactory)
        {
            _dataTypeService = dataTypeService;
            _dataTypePresentationFactory = dataTypePresentationFactory;
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

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.Boolean);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.CheckBoxList);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.DropDownListFlexible);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);
            res.AddRange(types);

            res = res
                .Where(x => x.Name?.StartsWith(session.System.Identifier) ?? false)
                .ToList();

            return res;
        }

        public async Task<List<IDataType>> Sync(SyncSession session)
        {
            var dataTypes = await GetDataTypesAsync(session);

            var res = dataTypes.ToList();
            var models = _dataTypeModelFactory.CreateModels(session, session.RootDataTypeFolder!);

            foreach (var model in models)
            {
                var existing = dataTypes.FirstOrDefault(x => x.Name == model.Name);
                var dataType = await CreateOrReplaceAsync(session, model, existing);
                res = res.Where(x => x.Name != dataType.Name).ToList();
                res.Add(dataType);
            }

            return res;
        }

        public async Task<List<IDataType>> ContainerPickerSync(SyncSession session)
        {
            var model = _dataTypeModelFactory.CreateContainerModel(session, session.RootDataTypeFolder!);
            var existing = session.DataTypes.FirstOrDefault(x => x.Name == model.Name);
            var dataType = await CreateOrReplaceAsync(session, model, existing);

            var res = session.DataTypes.Where(x => x.Name != dataType.Name).ToList();
            res.Add(dataType);

            return res;
        }

        private async Task<IDataType> CreateOrReplaceAsync(SyncSession session, CreateDataTypeRequestModel model, IDataType? existing)
        {
            if (existing == null)
            {
                var presAttempt = await _dataTypePresentationFactory.CreateAsync(model);
                if (!presAttempt.Success)
                    throw new InvalidOperationException($"Failed to create datatype presentation for {model.Name}");

                var dataTypeAttempt = await _dataTypeService.CreateAsync(presAttempt.Result, session.UserKey);
                if (!dataTypeAttempt.Success)
                    throw new InvalidOperationException($"Failed to create datatype for {model.Name}");

                return dataTypeAttempt.Result;
            }
            else
            {
                existing.ConfigurationData = model.Values
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Alias, x => x.Value!);

                var updated = await _dataTypeService.UpdateAsync(existing, session.UserKey);
                if (!updated.Success)
                    throw new InvalidOperationException($"Failed to update datatype presentation for {model.Name}");

                return updated.Result;
            }
        }
    }
}
