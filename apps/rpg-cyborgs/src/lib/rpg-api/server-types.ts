import { components } from './rpgtypes'

export type RpgContent = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgContent'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgContent']
>

export type SetStateRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.SetState'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.SetState']
>

export type DescribeRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.Describe'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.Describe']
>

export type ModSetRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Mods.ModSet'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Mods.ModSet']
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

export type ActivityCreateByGroupRequest = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityCreateByGroup'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgRequest.Rpg.ModObjects.Server.Ops.ActivityCreateByGroup']
>

export type ActivityResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Actions.Activity'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Actions.Activity']
>

export type DescribeResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Props.PropDesc'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.Rpg.ModObjects.Props.PropDesc']
>

export type CreateGraphStateResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.String'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.String']
>

export type BooleanResponse = Pick<
  components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.Boolean'],
  keyof components['schemas']['Rpg.ModObjects.Server.RpgResponse.System.Boolean']
>
