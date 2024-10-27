import { components } from './rpgtypes'

export type PointInTimeType = Pick<
  components['schemas']['Rpg.ModObjects.Time.PointInTimeType'],
  keyof components['schemas']['Rpg.ModObjects.Time.PointInTimeType']
>

export type PointInTime = Pick<
  components['schemas']['Rpg.ModObjects.Time.PointInTime'],
  keyof components['schemas']['Rpg.ModObjects.Time.PointInTime']
>

export type RpgGraphState = Pick<
  components['schemas']['Rpg.ModObjects.RpgGraphState'],
  keyof components['schemas']['Rpg.ModObjects.RpgGraphState']
>

export type RpgContainer = Pick<
  components['schemas']['Rpg.ModObjects.RpgContainer'],
  keyof components['schemas']['Rpg.ModObjects.RpgContainer']
>

export type RpgEntity = Pick<
  components['schemas']['Rpg.ModObjects.RpgEntity'],
  keyof components['schemas']['Rpg.ModObjects.RpgEntity']
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

export type PropDescription = Pick<
  components['schemas']['Rpg.ModObjects.Props.PropDescription'],
  keyof components['schemas']['Rpg.ModObjects.Props.PropDescription']
>

export type ModSetDescription = Pick<
  components['schemas']['Rpg.ModObjects.Mods.ModSetDescription'],
  keyof components['schemas']['Rpg.ModObjects.Mods.ModSetDescription']
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
