using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;

namespace Rpg.Cms.Tests.Models
{
    internal class TestDataType : IDataType
    {
        public IDataEditor? Editor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string EditorAlias { get; set; }

        public string? EditorUiAlias { get; set; }
        public ValueStorageType DatabaseType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDictionary<string, object> ConfigurationData { get; set; }

        public object? ConfigurationObject => throw new NotImplementedException();

        public string? Name { get; set; }
        public int CreatorId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ParentId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Level { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SortOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Trashed => throw new NotImplementedException();

        public int Id { get; set; }
        public Guid Key { get; set; }
        public DateTime CreateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime UpdateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? DeleteDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool HasIdentity => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;


        private TestDataType(int id, CreateDataTypeRequestModel model)
        {
            Id = id;
            Key = model.Id!.Value;
            EditorAlias = model.EditorAlias;
            EditorUiAlias = model.EditorUiAlias;
            Name = model.Name;
            ConfigurationData = new Dictionary<string, object>();

            foreach (var val in model.Values.Where(x => x.Value != null))
                ConfigurationData.Add(val.Alias, val.Value!);
        }

        public static List<IDataType> Convert(CreateDataTypeRequestModel[] models)
        {
            var dataTypes = new List<IDataType>();

            for (int i = 0; i < models.Count(); i++)
            {
                var model = models[i];
                dataTypes.Add(new TestDataType(i, model));
            }

            return dataTypes;
        }

        public object DeepClone()
        {
            throw new NotImplementedException();
        }

        public void DisableChangeTracking()
        {
            throw new NotImplementedException();
        }

        public void EnableChangeTracking()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetWereDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public bool IsDirty()
        {
            throw new NotImplementedException();
        }

        public bool IsPropertyDirty(string propName)
        {
            throw new NotImplementedException();
        }

        public void ResetDirtyProperties(bool rememberDirty)
        {
            throw new NotImplementedException();
        }

        public void ResetDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public void ResetIdentity()
        {
            throw new NotImplementedException();
        }

        public void ResetWereDirtyProperties()
        {
            throw new NotImplementedException();
        }

        public void SetParent(ITreeEntity? parent)
        {
            throw new NotImplementedException();
        }

        public bool WasDirty()
        {
            throw new NotImplementedException();
        }

        public bool WasPropertyDirty(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}
