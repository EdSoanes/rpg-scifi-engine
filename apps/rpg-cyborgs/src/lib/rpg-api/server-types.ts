import { components } from './rpgtypes'

export type RpgContent = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgContent'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgContent']
>

export type SetStateRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.SetState'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.SetState']
>

export type SetState = Pick<
  components['schemas']['Rpg.ModObjects.Server.Ops.SetState'],
  keyof components['schemas']['Rpg.ModObjects.Server.Ops.SetState']
>

export type DescribeStateRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.DescribeState'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.DescribeState']
>

export type DescribePropRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.DescribeProp'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.DescribeProp']
>

export type DescribePropResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Props.PropDescription'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Props.PropDescription']
>

export type ModSetRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Mods.ModSet'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Mods.ModSet']
>

export type DescribeModSetRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.DescribeModSet'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.DescribeModSet']
>

export type DescribeModSetResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Mods.ModSetDescription'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Mods.ModSetDescription']
>

export type ActivityActRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityAct'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityAct']
>

export type ActivityOutcomeRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityOutcome'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityOutcome']
>

export type ActivityCreateRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityCreate'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityCreate']
>

export type ActivityCreate = Pick<
  components['schemas']['Rpg.ModObjects.Server.Ops.ActivityCreate'],
  keyof components['schemas']['Rpg.ModObjects.Server.Ops.ActivityCreate']
>

export type ActivityCreateByGroupRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.Ops.ActivityCreateByTemplate'],
  keyof components['schemas']['Rpg.ModObjects.Server.Ops.ActivityCreateByTemplate']
>

export type ActivityResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Actions.Activity'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Actions.Activity']
>

export type StringResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.String'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.String']
>

export type BooleanResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.Boolean'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.Boolean']
>

export type SetTimeRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Time.PointInTime'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Time.PointInTime']
>

export type SetTimeResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Time.PointInTime'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Time.PointInTime']
>
