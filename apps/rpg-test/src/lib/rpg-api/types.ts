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

export type PropDescription = Pick<
  components['schemas']['Props.PropDescription'],
  keyof components['schemas']['Props.PropDescription']
>

export type ModDescription = Pick<
  components['schemas']['Props.ModDescription'],
  keyof components['schemas']['Props.ModDescription']
>

export type ModSetDescription = Pick<
  components['schemas']['Mods.ModSetDescription'],
  keyof components['schemas']['Mods.ModSetDescription']
>

export type ModSet = Pick<
  components['schemas']['Mods.ModSet'],
  keyof components['schemas']['Mods.ModSet']
>
