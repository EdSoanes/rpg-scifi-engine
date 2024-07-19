import { components } from './rpgtypes'

export type RpgGraphState = Pick<
  components['schemas']['Rpg.ModObjects.RpgGraphState'],
  keyof components['schemas']['Rpg.ModObjects.RpgGraphState']
>

export type RpgContent = Pick<
  components['schemas']['Rpg.Cms.Models.RpgContent'],
  keyof components['schemas']['Rpg.Cms.Models.RpgContent']
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

export type SetState = Pick<
  components['schemas']['Rpg.Cms.Models.RpgOperation.SetState'],
  keyof components['schemas']['Rpg.Cms.Models.RpgOperation.SetState']
>

export type Describe = Pick<
  components['schemas']['Rpg.Cms.Models.RpgOperation.Describe'],
  keyof components['schemas']['Rpg.Cms.Models.RpgOperation.Describe']
>

export type PropDesc = Pick<
  components['schemas']['Rpg.ModObjects.Props.PropDesc'],
  keyof components['schemas']['Rpg.ModObjects.Props.PropDesc']
>

export type PropValue = Pick<
  components['schemas']['Rpg.Cyborgs.Components.PropValue'],
  keyof components['schemas']['Rpg.Cyborgs.Components.PropValue']
>
