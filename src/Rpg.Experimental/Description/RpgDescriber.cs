using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Time;
using System.Reflection;
using Clock = Rpg.Experimental.Time.Temporal;

namespace Rpg.Experimental.Description
{
    /// <summary>
    /// Describes why things are as they are: the tree of mods behind a value, what a state or mod set
    /// does, and what an action has done or is waiting for.
    ///
    /// Describing only reads. It never changes the graph, never triggers a time event and never rolls.
    /// </summary>
    public static class RpgDescriber
    {
        public const string SkippedCostReason = "Not charged: turns were not being tracked";
        public const string DroppedEffectReason = "Dropped: too minor to start counting turns for";

        #region Property

        /// <summary>
        /// Describe a property of an object. The path can lead through child objects (e.g. "Head.Health").
        /// Depth is how many levels of sources to follow: 1 describes the property's own mods only.
        /// </summary>
        public static RpgPropertyDescription? DescribeProperty(RpgGraph graph, RpgObject? obj, string path, int depth = int.MaxValue)
        {
            var (propObj, prop) = graph.PropertyRefs.GetObjectForPath(obj, path);
            if (propObj == null || prop == null)
                return null;

            return DescribeProperty(graph, propObj.Id, prop, depth, new HashSet<string>());
        }

        private static RpgPropertyDescription? DescribeProperty(RpgGraph graph, string objectId, string prop, int depth, HashSet<string> describing)
        {
            var propData = graph.GetPropertyData<RpgPropertyDataModdable>(objectId, prop);
            if (propData == null)
                return null;

            var obj = graph.GetObject(objectId);
            var expression = propData.GetExpression(graph);

            var description = new RpgPropertyDescription
            {
                ObjectId = objectId,
                ObjectName = ObjectName(graph, obj, objectId),
                Archetype = obj?.Archetype,
                Prop = prop,
                DisplayName = DisplayName(graph, objectId, prop),
                Value = propData.GetValue<Dice>(graph),
                Expression = expression,
                BaseValue = ModCalculator.BaseValue(graph, propData.Mods),
                Roll = propData.Roll,
                IsRollPending = propData.IsRollPending(graph),
                IsRollOutOfDate = propData.Roll != null
                    && expression != null
                    && !expression.Value.IsConstant
                    && !propData.Roll.AppliesTo(expression)
            };

            var key = $"{objectId}.{prop}";
            if (describing.Contains(key))
            {
                description.IsLoop = true;
                return description;
            }

            if (depth <= 0)
            {
                description.IsTruncated = propData.Mods.Any();
                return description;
            }

            describing.Add(key);

            foreach (var mod in propData.Mods)
                description.Mods.Add(DescribeMod(graph, propData, mod, depth - 1, describing));

            describing.Remove(key);

            return description;
        }

        private static RpgModDescription DescribeMod(RpgGraph graph, RpgPropertyDataModdable propData, Mod mod, int depth, HashSet<string> describing)
        {
            var contribution = ModCalculator.Value(graph, mod);
            var owner = FindOwner(graph, mod);

            var description = new RpgModDescription
            {
                ModId = mod.Id,
                Name = mod.Name,
                Kind = mod.GetType().Name,
                TargetObjectId = propData.ObjectId,
                TargetProp = propData.Prop,
                Contribution = contribution,
                Lifespan = LifespanText(mod),
                IsManual = mod.IsManual,
                Origin = DescribeOrigin(graph, propData, mod, owner)
            };

            SetStatus(graph, description, propData, mod, owner, contribution);

            var sourceRef = mod.Source?.PropRef;
            if (sourceRef != null)
            {
                description.SourceObjectId = sourceRef.ObjectId;
                description.SourceProp = sourceRef.Path;
                description.SourceProperty = DescribeProperty(graph, sourceRef.ObjectId, sourceRef.Path, depth, describing);
            }
            else
            {
                description.FixedValue = mod.Source?.Value;
            }

            var calc = mod.Source?.Calc;
            if (calc != null)
            {
                description.Calculation = new RpgCalculationDescription
                {
                    Name = calc.FullName,
                    Description = CalculationText(graph, calc),
                    Input = ModCalculator.SourceValue(graph, mod),
                    Output = contribution
                };
            }

            return description;
        }

        private static void SetStatus(RpgGraph graph, RpgModDescription description, RpgPropertyDataModdable propData, Mod mod, RpgLifecycleObject? owner, Dice? contribution)
        {
            var counted = ModFilters.ActiveNoThreshold(propData.Mods).Any(x => x.Id == mod.Id);
            if (counted)
            {
                description.Counts = true;
                description.Status = RpgModStatus.Counts;

                var sourceRef = mod.Source?.PropRef;
                var sourcePending = sourceRef != null
                    && (graph.GetPropertyData<RpgPropertyDataModdable>(sourceRef.ObjectId, sourceRef.Path)?.IsRollPending(graph) ?? false);

                if (contribution == null && sourcePending)
                {
                    description.Status = RpgModStatus.WaitingForRoll;
                    description.WhyNot = "Waiting for a roll";
                }

                return;
            }

            description.Counts = false;

            switch (mod.Expiry)
            {
                case LifecycleExpiry.Expired:
                case LifecycleExpiry.Destroyed:
                    description.Status = RpgModStatus.Expired;
                    var ended = mod.Expired ?? mod.End;
                    description.WhyNot = ended.Type == TimePointType.Turn || ended.Type == TimePointType.Event
                        ? $"Ended at {PointText(ended)}"
                        : "Ended";
                    break;

                case LifecycleExpiry.Pending:
                case LifecycleExpiry.Unset:
                    description.Status = RpgModStatus.NotStarted;
                    description.WhyNot = mod.IsLifespanRelative
                        ? "Not started"
                        : $"Starts at {PointText(mod.Start)}";
                    break;

                case LifecycleExpiry.Suspended:
                    if (mod.IsUserEnabled == false)
                    {
                        description.Status = RpgModStatus.SwitchedOff;
                        description.WhyNot = "Switched off by hand";
                    }
                    else
                    {
                        description.Status = RpgModStatus.NotApplied;
                        description.WhyNot = owner switch
                        {
                            RpgState state => $"{state.Name} is off",
                            RpgModSet set when graph.GetObject(set.OwnerId) is RpgActivityAction => "The action is not completed",
                            RpgModSet set => $"{set.Name ?? "The mod set"} is not applied",
                            _ => "Not applied"
                        };
                    }
                    break;

                default:
                    if (mod.Type == ModType.Threshold)
                    {
                        description.Status = RpgModStatus.Limit;
                        description.WhyNot = "A limit. Limits are not applied yet";
                    }
                    else if (propData.Mods.Any(ModFilters.IsOverride) && !ModFilters.IsOverride(mod))
                    {
                        description.Status = RpgModStatus.Overridden;
                        description.WhyNot = "Set aside by an override";
                    }
                    else
                    {
                        description.Status = RpgModStatus.Replaced;
                        description.WhyNot = "Replaced by a newer version";
                    }
                    break;
            }
        }

        #endregion Property

        #region Origin

        /// <summary>
        /// The state or mod set a mod belongs to. Mods added to a set without an owner are found by looking
        /// through the sets.
        /// </summary>
        private static RpgLifecycleObject? FindOwner(RpgGraph graph, Mod mod)
        {
            var owner = graph.GetLifecycleObject(mod.OwnerId);
            if (owner is RpgModSet || owner is RpgActivityAction)
                return owner;

            return graph.Objects.Values
                .OfType<RpgModSet>()
                .FirstOrDefault(x => x.Mods.Any(m => m.Id == mod.Id));
        }

        private static RpgOriginDescription DescribeOrigin(RpgGraph graph, RpgPropertyDataModdable propData, Mod mod, RpgLifecycleObject? owner)
        {
            if (mod.IsManual)
                return new RpgOriginDescription { Kind = RpgOriginKind.Manual, Text = "Changed by hand" };

            if (owner is RpgState state)
            {
                var stateOwner = graph.GetObject(state.OwnerId);
                return new RpgOriginDescription
                {
                    Kind = RpgOriginKind.State,
                    Name = state.Name,
                    ObjectId = state.OwnerId,
                    ObjectName = ObjectName(graph, stateOwner, state.OwnerId),
                    Text = $"State {state.Name} on {ObjectName(graph, stateOwner, state.OwnerId)}"
                };
            }

            if (owner is RpgModSet set)
            {
                if (graph.GetObject(set.OwnerId) is RpgActivityAction setAction)
                    return ActionOrigin(graph, setAction, set.Name == ActionMethodNames.Cost ? RpgOriginKind.ActionCost : RpgOriginKind.ActionEffect, set.Name);

                var setOwner = graph.GetObject(set.OwnerId);
                return new RpgOriginDescription
                {
                    Kind = RpgOriginKind.ModSet,
                    Name = set.Name,
                    ObjectId = set.OwnerId,
                    ObjectName = ObjectName(graph, setOwner, set.OwnerId),
                    Text = $"{set.Name ?? "Mod set"} on {ObjectName(graph, setOwner, set.OwnerId)}"
                };
            }

            if (owner is RpgActivityAction ownerAction)
                return ActionOrigin(graph, ownerAction, RpgOriginKind.ActionEffect, ActionMethodNames.Outcome);

            var sourceRef = mod.Source?.PropRef;
            if (sourceRef != null)
            {
                var sourceObj = graph.GetObject(sourceRef.ObjectId);
                return new RpgOriginDescription
                {
                    Kind = RpgOriginKind.Derived,
                    ObjectId = sourceRef.ObjectId,
                    ObjectName = ObjectName(graph, sourceObj, sourceRef.ObjectId),
                    Text = $"From {ObjectName(graph, sourceObj, sourceRef.ObjectId)}.{DisplayName(graph, sourceRef.ObjectId, sourceRef.Path)}"
                };
            }

            if (mod.Type == ModType.Initial)
                return new RpgOriginDescription { Kind = RpgOriginKind.StartingValue, Text = "Starting value" };

            if (graph.GetObject(propData.ObjectId) is RpgActivityAction targetAction)
                return ActionOrigin(graph, targetAction, RpgOriginKind.ActionInput, null);

            if (propData.IsVirtual && propData.NeedsNumber)
                return new RpgOriginDescription { Kind = RpgOriginKind.RollRequest, Text = "Roll asked for by the rules" };

            return new RpgOriginDescription { Kind = RpgOriginKind.Rules, Text = "Set by the rules" };
        }

        private static RpgOriginDescription ActionOrigin(RpgGraph graph, RpgActivityAction activityAction, RpgOriginKind kind, string? step)
        {
            var actionName = activityAction.GetAction()?.Name ?? "action";
            var actionOwner = graph.GetObject(activityAction.ActionOwnerId);

            return new RpgOriginDescription
            {
                Kind = kind,
                Name = actionName,
                Step = step,
                ObjectId = activityAction.ActionOwnerId,
                ObjectName = ObjectName(graph, actionOwner, activityAction.ActionOwnerId),
                Text = kind switch
                {
                    RpgOriginKind.ActionCost => $"Cost of {actionName}",
                    RpgOriginKind.ActionEffect => $"Effect of {actionName}",
                    _ => $"Set during {actionName}"
                }
            };
        }

        #endregion Origin

        #region Mod sets and states

        public static RpgModSetDescription DescribeModSet(RpgGraph graph, RpgModSet modSet)
        {
            var owner = graph.GetObject(modSet.OwnerId);
            var description = new RpgModSetDescription
            {
                Id = modSet.Id,
                Name = modSet.Name,
                OwnerId = modSet.OwnerId,
                OwnerName = owner != null ? ObjectName(graph, owner, modSet.OwnerId!) : null,
                IsApplied = modSet.IsApplied,
                Lifespan = LifespanText(modSet)
            };

            //What the set changes is worked out from its mods as they stand. Nothing is applied to find out.
            foreach (var group in modSet.Mods.Where(x => x.Target != null).GroupBy(x => (x.Target!.ObjectId, x.Target.Path)))
            {
                var propData = graph.GetPropertyData<RpgPropertyDataModdable>(group.Key.ObjectId, group.Key.Path);
                var change = new RpgChangeDescription
                {
                    ObjectId = group.Key.ObjectId,
                    ObjectName = ObjectName(graph, graph.GetObject(group.Key.ObjectId), group.Key.ObjectId),
                    Prop = group.Key.Path,
                    DisplayName = DisplayName(graph, group.Key.ObjectId, group.Key.Path),
                    Change = Dice.Zero
                };

                foreach (var mod in group)
                {
                    var value = ModCalculator.Value(graph, mod);
                    if (value != null)
                        change.Change += value.Value;

                    if (propData != null)
                        change.Mods.Add(DescribeMod(graph, propData, mod, 0, new HashSet<string>()));
                }

                description.Changes.Add(change);
            }

            if (modSet is RpgState state)
                DescribeStateReason(graph, state, owner, description);

            return description;
        }

        public static RpgModSetDescription? DescribeState(RpgGraph graph, RpgObject obj, string stateName)
        {
            var state = graph.GetObjectState(obj.Id, stateName);
            return state != null
                ? DescribeModSet(graph, state)
                : null;
        }

        private static void DescribeStateReason(RpgGraph graph, RpgState state, RpgObject? owner, RpgModSetDescription description)
        {
            description.IsState = true;
            description.IsOn = state.IsOn;
            description.NeedsTurnTracking = state.NeedsTurnTracking;

            var stateName = state.Name ?? state.GetType().Name;
            var activationData = graph.GetPropertyData<RpgPropertyDataModdable>(state.OwnerId, RpgState.StatePropName(stateName));
            if (activationData != null)
                foreach (var mod in activationData.Mods.Where(x => x.Type != ModType.Initial))
                    description.Activations.Add(DescribeMod(graph, activationData, mod, 0, new HashSet<string>()));

            var activated = description.Activations.Any(x => x.Counts);

            if (owner == null || owner.Expiry != LifecycleExpiry.Active)
            {
                description.Reason = RpgStateReason.OwnerNotActive;
                description.ReasonText = "What it belongs to is not active";
            }
            else if (!state.IsApplied)
            {
                description.Reason = RpgStateReason.NotApplied;
                description.ReasonText = "It is not applied";
            }
            else if (state.IsUserEnabled == true)
            {
                description.Reason = RpgStateReason.SwitchedOnByHand;
                description.ReasonText = "Switched on by hand";
            }
            else if (state.IsUserEnabled == false)
            {
                description.Reason = RpgStateReason.SwitchedOffByHand;
                description.ReasonText = "Switched off by hand";
            }
            else if (state.IsOn && activated)
            {
                description.Reason = RpgStateReason.Activated;
                description.ReasonText = "Switched on by " + string.Join(" and ", description.Activations
                    .Where(x => x.Counts)
                    .Select(x => $"{LowerFirst(x.Origin.Text)} ({x.Lifespan})"));
            }
            else if (state.IsOn)
            {
                description.Reason = RpgStateReason.ConditionMet;
                description.ReasonText = "Its condition is met";
            }
            else
            {
                description.Reason = RpgStateReason.ConditionNotMet;
                description.ReasonText = "Its condition is not met";
            }
        }

        #endregion Mod sets and states

        #region Action

        public static RpgActionDescription DescribeAction(RpgGraph graph, RpgActivityAction activityAction, int depth = int.MaxValue)
        {
            var action = activityAction.GetAction();
            var actionOwner = graph.GetObject(activityAction.ActionOwnerId);

            var description = new RpgActionDescription
            {
                ActivityActionId = activityAction.Id,
                ActionName = action?.Name ?? activityAction.ActionId,
                OwnerId = activityAction.ActionOwnerId,
                OwnerName = ObjectName(graph, actionOwner, activityAction.ActionOwnerId),
                IsPerformable = action?.IsPerformable ?? false,
                IsComplete = activityAction.IsComplete,
                PendingRolls = activityAction.GetPendingRolls(graph).ToList(),
                FollowOnActions = activityAction.OutcomeActions.ToList()
            };

            description.Steps.Add(DescribeStep(graph, activityAction, ActionMethodNames.Cost, activityAction.CostMethod, depth));
            description.Steps.Add(DescribeStep(graph, activityAction, ActionMethodNames.Perform, activityAction.PerformMethod, depth));
            description.Steps.Add(DescribeStep(graph, activityAction, ActionMethodNames.Outcome, activityAction.OutcomeMethod, depth));

            foreach (var propData in graph.GetPropertyData<RpgPropertyDataModdable>(activityAction.Id))
            {
                var expression = propData.GetExpression(graph);
                if (propData.Roll != null && propData.Roll.AppliesTo(expression))
                    description.Rolls.Add(new RpgRollDescription
                    {
                        Prop = propData.Prop,
                        Expression = expression!.Value,
                        Roll = propData.Roll
                    });
            }

            description.Costs = activityAction.CostSet.Mods.Select(x => DescribeEffect(graph, x, null)).ToList();
            description.SkippedCosts = activityAction.SkippedCosts.Select(x => DescribeEffect(graph, x, SkippedCostReason)).ToList();
            description.DroppedEffects = activityAction.DroppedEffects.Select(x => DescribeEffect(graph, x, DroppedEffectReason)).ToList();

            var dropped = activityAction.DroppedEffects.Select(x => x.Id).ToHashSet();
            description.Effects = activityAction.Result.Mods
                .Where(x => !dropped.Contains(x.Id))
                .Select(x => DescribeEffect(graph, x, null))
                .ToList();

            return description;
        }

        private static RpgStepDescription DescribeStep(RpgGraph graph, RpgActivityAction activityAction, string name, RpgActionMethod method, int depth)
        {
            var step = new RpgStepDescription
            {
                Name = name,
                IsDone = method.IsDone
            };

            foreach (var arg in method.Args.Where(x => !RpgArg.IsGraphArg(x.Name) && !IsPlumbingArg(x.Name)))
            {
                var input = new RpgInputDescription
                {
                    Name = arg.Name,
                    Type = arg.Type,
                    IsNullable = arg.IsNullable,
                    Value = arg.Value?.ToString()
                };

                if (arg is RpgObjectArg)
                {
                    var obj = graph.GetObject(arg.Value?.ToString());
                    if (obj != null)
                        input.ObjectName = ObjectName(graph, obj, obj.Id);
                }
                else
                {
                    var propData = graph.GetPropertyData<RpgPropertyDataModdable>(activityAction.Id, arg.Name);
                    if (propData != null)
                    {
                        input.IsRollPending = propData.IsRollPending(graph);
                        input.Property = DescribeProperty(graph, activityAction.Id, arg.Name, depth, new HashSet<string>());

                        //A dice input shows its expression even before it is rolled
                        if (input.Value == null && !input.IsRollPending && input.Property?.Expression != null)
                            input.Value = input.Property.Value.ToString();
                    }
                }

                step.Inputs.Add(input);
            }

            return step;
        }

        /// <summary>
        /// Inputs that hand the action its own machinery. They say nothing about what the action does.
        /// </summary>
        private static bool IsPlumbingArg(string argName)
            => argName == ActionReservedArgs.Action
                || argName == ActionReservedArgs.Activity
                || argName == ActionReservedArgs.ActivityAction
                || argName == ActionReservedArgs.Context;

        private static RpgEffectDescription DescribeEffect(RpgGraph graph, Mod mod, string? reason)
        {
            var objectId = mod.Target?.ObjectId ?? string.Empty;
            var prop = mod.Target?.Path ?? string.Empty;

            //A state switched on by an action reads better as the state than as its property
            var displayName = RpgState.IsStateProp(prop)
                ? $"state {prop.Substring("State/".Length)}"
                : DisplayName(graph, objectId, prop);

            return new RpgEffectDescription
            {
                ModId = mod.Id,
                Name = mod.Name,
                ObjectId = objectId,
                ObjectName = ObjectName(graph, graph.GetObject(objectId), objectId),
                Prop = prop,
                DisplayName = displayName,
                Change = ModCalculator.Value(graph, mod),
                Lifespan = LifespanText(mod),
                Reason = reason,
                IsStateActivation = RpgState.IsStateProp(prop)
            };
        }

        #endregion Action

        #region Object

        public static RpgObjectDescription DescribeObject(RpgGraph graph, RpgObject obj)
        {
            var description = new RpgObjectDescription
            {
                ObjectId = obj.Id,
                Name = ObjectName(graph, obj, obj.Id),
                Archetype = obj.Archetype
            };

            foreach (var propData in graph.GetPropertyData<RpgPropertyDataModdable>(obj.Id).Where(x => !RpgState.IsStateProp(x.Prop)))
            {
                var value = propData.GetValue<Dice>(graph);
                var original = ModCalculator.OriginalBaseValue(graph, propData.Mods);

                description.Properties.Add(new RpgPropertySummary
                {
                    Prop = propData.Prop,
                    DisplayName = DisplayName(graph, obj.Id, propData.Prop),
                    Value = value,
                    OriginalValue = original,
                    IsChanged = value != (original ?? Dice.Zero),
                    IsRollPending = propData.IsRollPending(graph)
                });
            }

            foreach (var state in graph.GetObjectStates(obj.Id).OrderBy(x => x.Name))
            {
                if (state.IsOn)
                    description.StatesOn.Add(state.Name ?? state.GetType().Name);
                else
                    description.StatesOff.Add(state.Name ?? state.GetType().Name);
            }

            return description;
        }

        #endregion Object

        #region Words

        private static string ObjectName(RpgGraph graph, RpgObject? obj, string? objectId)
        {
            if (obj is RpgActivityAction activityAction)
                return activityAction.GetAction()?.Name ?? activityAction.Archetype;

            if (obj == null)
                return objectId ?? string.Empty;

            return !string.IsNullOrEmpty(obj.Name)
                ? obj.Name
                : obj.Archetype;
        }

        private static string DisplayName(RpgGraph graph, string objectId, string prop)
        {
            var displayName = graph.GetMetaProperty(objectId, prop)?.DisplayName;
            return !string.IsNullOrEmpty(displayName)
                ? displayName
                : prop;
        }

        private static string? CalculationText(RpgGraph graph, RpgMethod<RpgObject, Dice> calc)
        {
            try
            {
                MethodInfo? methodInfo = calc.IsStatic
                    ? RpgTypeUtilities.ForMethod($"{calc.ClassName}.{calc.MethodName}")
                    : graph.GetObject(calc.EntityId) is RpgObject obj
                        ? RpgTypeUtilities.ForMethod(obj.GetType(), calc.MethodName)
                        : null;

                return methodInfo?.GetCustomAttribute<DescribeAttribute>()?.Text;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string LowerFirst(string text)
            => string.IsNullOrEmpty(text)
                ? text
                : char.ToLower(text[0]) + text.Substring(1);

        internal static string PointText(TimePoint point)
            => point.Type switch
            {
                TimePointType.Turn => $"turn {point.Count}",
                TimePointType.Event => point.Event == Clock.TimePassesEvent ? "time passes" : point.Event ?? "a time event",
                TimePointType.TimePasses => "time passes",
                TimePointType.EncounterBegins => "the start of turn tracking",
                TimePointType.EncounterEnds => "the end of turn tracking",
                TimePointType.TimeEnds => "the end",
                _ => "the start"
            };

        /// <summary>
        /// When something starts and ends, in words: "permanent", "until turn 4", "from Sunset until Sunrise",
        /// "for 2 turns"
        /// </summary>
        internal static string LifespanText(RpgLifecycleObject lifecycle)
        {
            string text;

            if (lifecycle.IsLifespanRelative)
            {
                //Turn counts that have not been given actual turn numbers yet
                var duration = lifecycle.End.Count - lifecycle.Start.Count;
                text = lifecycle.End.Type == TimePointType.Turn
                    ? $"for {duration} {(duration == 1 ? "turn" : "turns")}"
                    : $"until {PointText(lifecycle.End)}";

                if (lifecycle.Start.Type == TimePointType.Turn && lifecycle.Start.Count > 0)
                    text += $", starting in {lifecycle.Start.Count} {(lifecycle.Start.Count == 1 ? "turn" : "turns")}";
            }
            else
            {
                var hasStart = lifecycle.Start.Type == TimePointType.Turn || lifecycle.Start.Type == TimePointType.Event;
                var hasEnd = lifecycle.End.Type != TimePointType.TimeEnds;

                var parts = new List<string>();
                if (hasStart)
                    parts.Add($"from {PointText(lifecycle.Start)}");

                if (hasEnd)
                    parts.Add($"until {PointText(lifecycle.End)}");

                text = parts.Any()
                    ? string.Join(" ", parts)
                    : RpgDescriptionText.Permanent;
            }

            //A combining mod that has been merged into a total has not ended in any way a player would
            //recognise, and outside turn tracking there is no turn to name
            var isMerged = lifecycle is Mod mod && mod.Behavior == ModBehavior.Combine;
            var expired = lifecycle.Expired;
            if (expired != null && !isMerged && (expired.Value.Type == TimePointType.Turn || expired.Value.Type == TimePointType.Event))
                text += $", ended at {PointText(expired.Value)}";

            return text;
        }

        #endregion Words
    }
}
