using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Values;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services
{
    public class DataTypeSynchronizer : IDataTypeSynchronizer
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IDataTypePresentationFactory _dataTypePresentationFactory;
        private readonly IDataTypeContainerService _dataTypeContainerService;

        public DataTypeSynchronizer(IDataTypeService dataTypeService, IDataTypePresentationFactory dataTypePresentationFactory, IDataTypeContainerService dataTypeContainerService)
        {
            _dataTypeService = dataTypeService;
            _dataTypePresentationFactory = dataTypePresentationFactory;
            _dataTypeContainerService = dataTypeContainerService;
        }

        public string GetName(IMetaSystem system, MetaPropUIAttribute propUIAttribute)
            => $"{system.Identifier} {propUIAttribute.Name}".Replace("UIAttribute", "");

        public CreateDataTypeRequestModel CreateModel(IMetaSystem system, IUmbracoEntity parentFolder, MetaPropUIAttribute propUIAttribute)
        {
            var res = new CreateDataTypeRequestModel();

            res.Name = GetName(system, propUIAttribute);
            res.Parent = new ReferenceByIdModel(parentFolder.Key);
            res.EditorAlias = MapEditorToUmbEditor(propUIAttribute.Editor);
            res.EditorUiAlias = MapEditorToUmbUIEditor(propUIAttribute.Editor);
            res.Values = propUIAttribute.Editor switch
            {
                nameof(Int32) => CreateIntValues(propUIAttribute),
                "Html" => CreateHtmlValues(propUIAttribute),
                _ => Enumerable.Empty<DataTypePropertyPresentationModel>()
            };

            return res;
        }

        public async Task<List<IDataType>> Synchronize(RpgSyncSession session, IMetaSystem system)
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
                .Where(x => x.Name?.StartsWith(system.Identifier) ?? false)
                .ToList();

            foreach (var propUIAttribute in system.PropUIAttributes)
            {
                var name = GetName(system, propUIAttribute);
                if (!res.Any(x => x.Name == name))
                {
                    var dataTypeModel = CreateModel(system, session.RootDataTypeFolder!, propUIAttribute);
                    var presAttempt = await _dataTypePresentationFactory.CreateAsync(dataTypeModel);
                    if (!presAttempt.Success)
                        throw new InvalidOperationException($"Failed to create datatype presentation for {name}");

                    var dataTypeAttempt = await _dataTypeService.CreateAsync(presAttempt.Result, session.UserKey);
                    if (!dataTypeAttempt.Success)
                        throw new InvalidOperationException($"Failed to create datatype for {name}");

                    res.Add(dataTypeAttempt.Result);
                }
            }

            return res;
        }

        private IEnumerable<DataTypePropertyPresentationModel> CreateIntValues(MetaPropUIAttribute propUIAttribute)
        {
            var attr = propUIAttribute as IntegerUIAttribute;
            if (attr == null)
                return Enumerable.Empty<DataTypePropertyPresentationModel>();

            var vals = new List<DataTypePropertyPresentationModel>();
            if (attr.Min != int.MinValue)
                vals.Add(new DataTypePropertyPresentationModel { Alias = "min", Value = attr.Min });
            if (attr.Max != int.MaxValue)
                vals.Add(new DataTypePropertyPresentationModel { Alias = "max", Value = attr.Max });

            return vals;
        }

        private IEnumerable<DataTypePropertyPresentationModel> CreateHtmlValues(MetaPropUIAttribute propUIAttribute)
        {
            var attr = propUIAttribute as RichTextUIAttribute;
            if (attr == null)
                return Enumerable.Empty<DataTypePropertyPresentationModel>();

            var vals = new List<DataTypePropertyPresentationModel>
            {
                new DataTypePropertyPresentationModel { Alias = "mode", Value = "classic" },
                new DataTypePropertyPresentationModel { Alias = "toolbar", Value = new object[]
                {
                    "cut",
                    "copy",
                    "paste",
                    "bold",
                    "italic",
                    "alignleft",
                    "aligncenter",
                    "alignright",
                    "alignjustify",
                    "bullist",
                    "numlist",
                    "outdent",
                    "indent",
                    "sourcecode"
                }}
            };

            return vals;
        }

        private string MapEditorToUmbEditor(string editor)
        {
            return editor switch
            {
                nameof(Int32) => "Umbraco.Integer",
                nameof(Dice) => "Umbraco.TextBox",
                "Html" => "Umbraco.RichText",
                _ => "Umbraco.TextBox"
            };
        }

        private string MapEditorToUmbUIEditor(string editor)
        {
            return editor switch
            {
                nameof(Int32) => "Umb.PropertyEditorUi.Integer",
                nameof(Dice) => "Umb.PropertyEditorUi.TextBox",
                "Html" => "Umb.PropertyEditorUi.TinyMCE",
                _ => "Umb.PropertyEditorUi.TextBox"
            };
        }
    }
}
