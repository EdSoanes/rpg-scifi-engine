using Newtonsoft.Json;
using Rpg.Sys.Actions;
using System.Collections;

namespace Rpg.Sys.Archetypes
{
    public class Container : Artifact, IList<Artifact>
    {
        [JsonProperty] private List<Artifact> _artifacts { get; set; } = new List<Artifact>();

        [JsonProperty] public int MaxEncumbrance { get; private set; } = int.MaxValue;
        [JsonProperty] public int MaxItems {  get; private set; } = int.MaxValue;
        [JsonProperty] public int CurrentEncumbrance { get; private set; }
        [JsonProperty] public int CurrentItems { get; private set; }

        [JsonProperty] public ActionCost AddCost { get; private set; } = new ActionCost();
        [JsonProperty] public ActionCost RemoveCost { get; private set; } = new ActionCost();

        public Container() { }
        public Container(ActionCost addCost, ActionCost removeCost)
        {
            AddCost = addCost;
            RemoveCost = removeCost;
        }

        #region IList

        public Artifact? Remove(Guid id)
        {
            var toRemove = _artifacts.SingleOrDefault(x => x.Id == id);
            if (toRemove != null)
            {
                _artifacts.Remove(toRemove);
            }

            return toRemove;
        }

        public Artifact this[int index] { get => _artifacts[index]; set => _artifacts[index] = value; }
        public int Count => _artifacts.Count;
        public bool IsReadOnly => false;
        public void Add(Artifact item) => _artifacts.Add(item);
        public void Clear() => _artifacts.Clear();
        public bool Contains(Artifact item) => _artifacts.Contains(item);
        public void CopyTo(Artifact[] array, int arrayIndex) => _artifacts.CopyTo(array, arrayIndex);
        public IEnumerator<Artifact> GetEnumerator() => _artifacts.GetEnumerator();
        public int IndexOf(Artifact item) => _artifacts.IndexOf(item);
        public void Insert(int index, Artifact item) => _artifacts.Insert(index, item);
        public bool Remove(Artifact item) => _artifacts.Remove(item);
        public void RemoveAt(int index) => _artifacts.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => _artifacts.GetEnumerator();

        #endregion IList
    }
}
