import { components } from './rpgtypes'

export type LifecycleExpiry = Pick<
  components['schemas']['Time.LifecycleExpiry'],
  keyof components['schemas']['Time.LifecycleExpiry']
>

export type PointInTimeType = Pick<
  components['schemas']['Time.PointInTimeType'],
  keyof components['schemas']['Time.PointInTimeType']
>

export type PointInTime = Pick<
  components['schemas']['Time.PointInTime'],
  keyof components['schemas']['Time.PointInTime']
>

export type Lifespan = Pick<
  components['schemas']['Time.Lifespan'],
  keyof components['schemas']['Time.Lifespan']
>

export type RpgGraphState = Pick<
  components['schemas']['RpgGraphState'],
  keyof components['schemas']['RpgGraphState']
>

export type RpgContainer = Pick<
  components['schemas']['RpgContainer'],
  keyof components['schemas']['RpgContainer']
>

export type RpgEntity = Pick<
  components['schemas']['RpgEntity'],
  keyof components['schemas']['RpgEntity']
>

export type PropRef = Pick<
  components['schemas']['Props.PropRef'],
  keyof components['schemas']['Props.PropRef']
>

export interface State {
  expiry: LifecycleExpiry
  readonly id: string
  readonly name: string
  readonly classification: string
  readonly ownerId: string
  readonly ownerArchetype?: string | null
  readonly isPlayerVisible: boolean
  readonly isOn: boolean
  readonly isOnTimed: boolean
  readonly isOnManually: boolean
  readonly isOnConditionally: boolean
}

export interface Dice {
  isConstant: boolean
  expr?: string
}

export type RpgArg = Pick<
  components['schemas']['Reflection.Args.RpgArg'],
  keyof components['schemas']['Reflection.Args.RpgArg']
>

export type ActionTemplateMethod = Pick<
  components['schemas']['Reflection.RpgMethod_ActionTemplate_Boolean'],
  keyof components['schemas']['Reflection.RpgMethod_ActionTemplate_Boolean']
>

export interface ActionTemplate {
  readonly id: string
  readonly name: string
  readonly classification: string
  readonly ownerId: string
  readonly ownerArchetype: string
  readonly actionArgs: RpgArg[]
  canPerformMethod: ActionTemplateMethod
  costMethod: ActionTemplateMethod
  performMethod: ActionTemplateMethod
  outcomeMethod: ActionTemplateMethod
  readonly isPerformable: boolean
}

export type Action = Pick<
  components['schemas']['Activities.Action'],
  keyof components['schemas']['Activities.Action']
>

export type Activity = Pick<
  components['schemas']['Activities.Activity'],
  keyof components['schemas']['Activities.Activity']
>

export type ObjectPropInfo = Pick<
  components['schemas']['Description.ObjectPropInfo'],
  keyof components['schemas']['Description.ObjectPropInfo']
>

export type PropInfo = Pick<
  components['schemas']['Description.PropInfo'],
  keyof components['schemas']['Description.PropInfo']
>

export type ModInfo = Pick<
  components['schemas']['Description.ModInfo'],
  keyof components['schemas']['Description.ModInfo']
>

export type ModSetValues = Pick<
  components['schemas']['Description.ModSetValues'],
  keyof components['schemas']['Description.ModSetValues']
>

export type ModSet = Pick<
  components['schemas']['Mods.ModSet'],
  keyof components['schemas']['Mods.ModSet']
>

export type Mod = Pick<
  components['schemas']['Mods.Mod'],
  keyof components['schemas']['Mods.Mod']
>

export type ThresholdMod = Pick<
  components['schemas']['Mods.Mods.Threshold'],
  keyof components['schemas']['Mods.Mods.Threshold']
>

export type BaseMod = Pick<
  components['schemas']['Mods.Mods.Base'],
  keyof components['schemas']['Mods.Mods.Base']
>

export type InitialMod = Pick<
  components['schemas']['Mods.Mods.Initial'],
  keyof components['schemas']['Mods.Mods.Initial']
>

export type OverrideMod = Pick<
  components['schemas']['Mods.Mods.Override'],
  keyof components['schemas']['Mods.Mods.Override']
>
