using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects
{
    public abstract class RpgEntity : RpgObject
    {
        [JsonProperty] public ActionTemplateDictionary ActionTemplates { get; private set; }

        public RpgEntity() : base() 
        {
            ActionTemplates = new ActionTemplateDictionary();
        }

        public RpgEntity(string name)
            : this()
        {
            Name = name;
        }

        #region Actions

        public bool CanInitiateAction(string actionTemplate)
            => CanInitiateAction(this, actionTemplate);

        public bool CanInitiateAction(ActionRef actionRef)
        {
            var owner = Graph.GetObject<RpgEntity>(actionRef.ActionTemplateOwnerId);
            return owner?.CanInitiateAction(actionRef.ActionTemplateName) ?? false;
        }

        public bool CanInitiateAction(RpgEntity owner, string actionTemplate)
            => owner.GetActionTemplate(actionTemplate)?.IsPerformable ?? false;

        public Activity InitiateAction(string actionTemplate)
            => InitiateAction(this, actionTemplate);

        public Activity InitiateAction(ActionRef actionRef)
        {
            var owner = Graph.GetObject<RpgEntity>(actionRef.ActionTemplateOwnerId);
            if (owner == null)
                throw new InvalidOperationException($"Cannot find action owner from actionref {actionRef.ActionTemplateOwnerId}.{actionRef.ActionTemplateName}");

            return InitiateAction(owner, actionRef.ActionTemplateName);
        }

        public Activity InitiateAction(RpgEntity owner, string actionTemplate)
        {
            var template = owner.GetActionTemplate(actionTemplate);
            if (template == null)
                throw new InvalidOperationException($"Cannot find action template {actionTemplate}");

            var activity = Graph.GetObjectsOwnedBy<Activity>(Id).FirstOrDefault();
            if (activity == null)
            {
                activity = new Activity(this, actionTemplate, 0);
                Graph.AddObject(activity);
            }

            var action = new Activities.Action(template, owner, this, activity);
            Graph.AddObject(action);

            return activity;
        }

        public ActionTemplate? GetActionTemplate(string actionTemplate)
            => ActionTemplates.ContainsKey(actionTemplate) ? ActionTemplates[actionTemplate] : null;

        #endregion Actions

        #region ModSets

        #endregion ModSets

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);

            var actionTemplates = ActionTemplateFactory.CreateFor(this);
            foreach (var actionTemplate in actionTemplates)
            {
                if (actionTemplate != null)
                {
                    actionTemplate.OnCreating(Graph!, this);
                    if (!ActionTemplates.ContainsKey(actionTemplate.Name))
                        ActionTemplates.Add(actionTemplate.Name, actionTemplate!);
                }
            }

        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity)
        {
            base.OnRestoring(graph, entity);

            foreach (var actionTemplates in ActionTemplates.Values)
                actionTemplates.OnRestoring(Graph);
        }
    }
}


