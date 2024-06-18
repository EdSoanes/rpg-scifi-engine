using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Tests.Models
{
    public class TestDataTypeRootFolder : IUmbracoEntity
    {
        public string? Name { get; set; } = "TestDataTypeFolder";
        public int CreatorId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ParentId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Level { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SortOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Trashed => throw new NotImplementedException();

        public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid Key { get; set; } = Guid.NewGuid();
        public DateTime CreateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime UpdateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? DeleteDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool HasIdentity => throw new NotImplementedException();

        public object DeepClone()
        {
            throw new NotImplementedException();
        }

        public void ResetIdentity()
        {
            throw new NotImplementedException();
        }

        public void SetParent(ITreeEntity? parent)
        {
            throw new NotImplementedException();
        }
    }
}
