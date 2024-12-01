import { components } from './rpgtypes'

export type PropValue = Pick<
  components['schemas']['Cyborgs.Components.PropValue'],
  keyof components['schemas']['Cyborgs.Components.PropValue']
>

export type PlayerCharacter = Pick<
  components['schemas']['Cyborgs.PlayerCharacter'],
  keyof components['schemas']['Cyborgs.PlayerCharacter']
>

export type BodyPart = Pick<
  components['schemas']['Cyborgs.BodyPart'],
  keyof components['schemas']['Cyborgs.BodyPart']
>

export type Armour = Pick<
  components['schemas']['Cyborgs.Armour'],
  keyof components['schemas']['Cyborgs.Armour']
>

export type MeleeWeapon = Pick<
  components['schemas']['Cyborgs.MeleeWeapon'],
  keyof components['schemas']['Cyborgs.MeleeWeapon']
>

export type RangedWeapon = Pick<
  components['schemas']['Cyborgs.RangedWeapon'],
  keyof components['schemas']['Cyborgs.RangedWeapon']
>

export type MeleeAttack = Pick<
  components['schemas']['Cyborgs.Actions.MeleeAttack'],
  keyof components['schemas']['Cyborgs.Actions.MeleeAttack']
>
