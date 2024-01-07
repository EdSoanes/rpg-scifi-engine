using Newtonsoft.Json;
using Rpg.Sys.Actions;
using System.Collections;

namespace Rpg.Sys.Archetypes
{
    public class Container : Artifact, IEnumerable<Artifact>
    {
        [JsonProperty] private List<Artifact> _artifacts { get; set; } = new List<Artifact>();

        [JsonProperty] public int MaxEncumbrance { get; protected set; } = int.MaxValue;
        [JsonProperty] public int MaxItems {  get; protected set; } = int.MaxValue;
        [JsonProperty] public int CurrentEncumbrance { get => Encumbrance(); }
        [JsonProperty] public int CurrentItems { get => Count(); }

        [JsonProperty] public ActionCost AddCost { get; private set; } = new ActionCost();
        [JsonProperty] public ActionCost RemoveCost { get; private set; } = new ActionCost();

        [JsonConstructor] private Container() { }

        public Container(string containerName, ActionCost addCost, ActionCost removeCost)
        {
            Name = containerName;
            AddCost = addCost;
            RemoveCost = removeCost;
        }

        public Container(string containerName, int maxEncumbrance, int maxItems, ActionCost addCost, ActionCost removeCost)
        {
            Name = containerName;
            MaxEncumbrance = maxEncumbrance; 
            MaxItems = maxItems;
            AddCost = addCost;
            RemoveCost = removeCost;
        }

        #region IList

        private void OnContainerEncumbranceChanged() 
            => OnContainerEncumbranceChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentEncumbrance)));

        private void OnContainerEncumbranceChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var container = sender as Container;
            var prop = e.PropertyName;

            if (container != null && prop == nameof(CurrentEncumbrance))
                NotifyPropertyChanged(nameof(CurrentEncumbrance));
        }

        private void OnContainerItemsChanged()
            => OnContainerItemsChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentItems)));

        private void OnContainerItemsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var container = sender as Container;
            var prop = e.PropertyName;

            if (container != null && prop == nameof(CurrentItems))
                NotifyPropertyChanged(nameof(CurrentItems));
        }

        public void Add(Artifact artifact)
        {
            _artifacts.Add(artifact);

            if (artifact is Container)
            {
                artifact.PropertyChanged += OnContainerEncumbranceChanged;
                artifact.PropertyChanged += OnContainerItemsChanged;
            }

            OnContainerEncumbranceChanged();
            OnContainerItemsChanged();
        }

        public Artifact? Remove(Guid id)
        {
            var artifact = _artifacts.SingleOrDefault(x => x.Id == id);
            if (artifact != null)
            {
                _artifacts.Remove(artifact);
                if (artifact is Container)
                {
                    artifact.PropertyChanged -= OnContainerEncumbranceChanged;
                    artifact.PropertyChanged -= OnContainerItemsChanged;
                }

                OnContainerEncumbranceChanged();
                OnContainerItemsChanged();
            }

            return artifact;
        }

        public Artifact[] GetAll() 
            => _artifacts.ToArray();

        public T? Get<T>(Guid id) where T : Artifact
            => GetByFilter(out var containers, artifact => artifact.Id == id)?.SingleOrDefault() as T;

        public T? Get<T>(Guid id, out Container? container) where T : Artifact
        {
            var res = GetByFilter(out var containers, artifact => artifact.Id == id)?.SingleOrDefault() as T;
            container = res != null
                ? containers.Single()
                : null;

            return res;
        }

        public T[] Get<T>() where T : Artifact 
            => GetByFilter(out var containers, artifact => artifact is T).Cast<T>().ToArray();

        public T[] Get<T>(out Container[] containers) where T : Artifact
            => GetByFilter(out containers, artifact => artifact is T).Cast<T>().ToArray();

        public Artifact[] Get(string isA)
            => GetByFilter(out var containers, (artifact) => artifact.IsA(isA));

        public Artifact[] Get(string isA, out Container[] containers)
            => GetByFilter(out containers, (artifact) => artifact.IsA(isA));

        public Artifact[] GetByFilter(out Container[] containers, Func<Artifact, bool> filter)
        {
            var res = new List<Artifact>();
            var cRes = new List<Container>();

            foreach (var artifact in _artifacts)
            {
                if (filter(artifact))
                {
                    res.Add(artifact);
                    if (!cRes.Any(x => x.Id == Id))
                        cRes.Add(this);
                }

                if (artifact is Container container)
                {
                    var subRes = container.GetByFilter(out var cSubRes, filter);
                    res.AddRange(subRes);
                    cRes.AddRange(cSubRes);
                }
            }

            containers = cRes.ToArray();
            return res.ToArray();
        }

        public int Count() => Get<Container>().Sum(x => x.Count()) + _artifacts.Count();
        public int Encumbrance() => Get<Container>().Sum(x => x.Encumbrance()) + _artifacts.Sum(x => x.Presence.Weight);
        public IEnumerator<Artifact> GetEnumerator() => _artifacts.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _artifacts.GetEnumerator();

        #endregion IList
    }
}
