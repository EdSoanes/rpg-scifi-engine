import { components } from './rpgtypes'

export type PlayerCharacter = Pick<
  components['schemas']['Rpg.Cyborgs.PlayerCharacter'],
  keyof components['schemas']['Rpg.Cyborgs.PlayerCharacter']
>

export type BodyPart = Pick<
  components['schemas']['Rpg.Cyborgs.BodyPart'],
  keyof components['schemas']['Rpg.Cyborgs.BodyPart']
>

export type Armour = Pick<
  components['schemas']['Rpg.Cyborgs.Armour'],
  keyof components['schemas']['Rpg.Cyborgs.Armour']
>

export type MeleeWeapon = Pick<
  components['schemas']['Rpg.Cyborgs.MeleeWeapon'],
  keyof components['schemas']['Rpg.Cyborgs.MeleeWeapon']
>

export type RangedWeapon = Pick<
  components['schemas']['Rpg.Cyborgs.RangedWeapon'],
  keyof components['schemas']['Rpg.Cyborgs.RangedWeapon']
>
