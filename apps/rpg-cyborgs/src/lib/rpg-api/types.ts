import { components } from './rpgtypes'

export type RpgGraphState = Pick<
  components['schemas']['Rpg.ModObjects.RpgGraphState'],
  keyof components['schemas']['Rpg.ModObjects.RpgGraphState']
>

export type PlayerCharacter = Pick<
  components['schemas']['Rpg.Cyborgs.PlayerCharacter'],
  keyof components['schemas']['Rpg.Cyborgs.PlayerCharacter']
>

export type State = Pick<
  components['schemas']['Rpg.ModObjects.States.State'],
  keyof components['schemas']['Rpg.ModObjects.States.State']
>

export type Action = Pick<
  components['schemas']['Rpg.ModObjects.Actions.Action'],
  keyof components['schemas']['Rpg.ModObjects.Actions.Action']
>

export type Activity = Pick<
  components['schemas']['Rpg.ModObjects.Actions.Activity'],
  keyof components['schemas']['Rpg.ModObjects.Actions.Activity']
>

export type ActivityTemplate = Pick<
  components['schemas']['Rpg.ModObjects.Actions.ActivityTemplate'],
  keyof components['schemas']['Rpg.ModObjects.Actions.ActivityTemplate']
>

export type PropDesc = Pick<
  components['schemas']['Rpg.ModObjects.Props.PropDesc'],
  keyof components['schemas']['Rpg.ModObjects.Props.PropDesc']
>

export type PropValue = Pick<
  components['schemas']['Rpg.Cyborgs.Components.PropValue'],
  keyof components['schemas']['Rpg.Cyborgs.Components.PropValue']
>

export type ActionInstance = Pick<
  components['schemas']['Rpg.ModObjects.Actions.ActionInstance'],
  keyof components['schemas']['Rpg.ModObjects.Actions.ActionInstance']
>

export type ModSet = Pick<
  components['schemas']['Rpg.ModObjects.Mods.ModSet'],
  keyof components['schemas']['Rpg.ModObjects.Mods.ModSet']
>

export type RpgArg = Pick<
  components['schemas']['Rpg.ModObjects.Reflection.Args.RpgArg'],
  keyof components['schemas']['Rpg.ModObjects.Reflection.Args.RpgArg']
>
