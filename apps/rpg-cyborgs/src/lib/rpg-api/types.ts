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

export type ActionModSet = Pick<
  components['schemas']['Rpg.ModObjects.Actions.ActionModSet'],
  keyof components['schemas']['Rpg.ModObjects.Actions.ActionModSet']
>

export type OutcomeModSet = Pick<
  components['schemas']['Rpg.ModObjects.Actions.OutcomeModSet'],
  keyof components['schemas']['Rpg.ModObjects.Actions.OutcomeModSet']
>

export type RpgArg = Pick<
  components['schemas']['Rpg.ModObjects.Reflection.Args.RpgArg'],
  keyof components['schemas']['Rpg.ModObjects.Reflection.Args.RpgArg']
>
