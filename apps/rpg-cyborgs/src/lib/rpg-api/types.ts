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

export interface State {
  expiry: LifecycleExpiry;
  readonly id: string;
  readonly name: string;
  readonly ownerId: string;
  readonly ownerArchetype?: string | null;
  readonly isPlayerVisible: boolean;
  readonly isOn: boolean;
  readonly isOnTimed: boolean;
  readonly isOnManually: boolean;
  readonly isOnConditionally: boolean;
}

export type Action = Pick<
  components['schemas']['Actions2.Action2'],
  keyof components['schemas']['Actions2.Action2']
>

export type Activity = Pick<
  components['schemas']['Actions2.Activity2'],
  keyof components['schemas']['Actions2.Activity2']
>

export type PropDescription = Pick<
  components['schemas']['Props.PropDescription'],
  keyof components['schemas']['Props.PropDescription']
>

export type ModSetDescription = Pick<
  components['schemas']['Mods.ModSetDescription'],
  keyof components['schemas']['Mods.ModSetDescription']
>

export type ModSet = Pick<
  components['schemas']['Mods.ModSet'],
  keyof components['schemas']['Mods.ModSet']
>

export type RpgArg = Pick<
  components['schemas']['Reflection.Args.RpgArg'],
  keyof components['schemas']['Reflection.Args.RpgArg']
>
