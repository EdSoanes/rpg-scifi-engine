import { components } from './rpgtypes'

export type RpgContent = Pick<
  components['schemas']['Server.RpgContent'],
  keyof components['schemas']['Server.RpgContent']
>

export type SetStateRequest = Pick<
  components['schemas']['Server.RpgRequest_SetState'],
  keyof components['schemas']['Server.RpgRequest_SetState']
>

export type SetState = Pick<
  components['schemas']['Server.Ops.SetState'],
  keyof components['schemas']['Server.Ops.SetState']
>

export type DescribeStateRequest = Pick<
  components['schemas']['Server.RpgRequest_DescribeState'],
  keyof components['schemas']['Server.RpgRequest_DescribeState']
>

export type DescribePropRequest = Pick<
  components['schemas']['Server.RpgRequest_DescribeProp'],
  keyof components['schemas']['Server.RpgRequest_DescribeProp']
>

export type DescribePropResponse = Pick<
  components['schemas']['Server.RpgResponse_PropDescription'],
  keyof components['schemas']['Server.RpgResponse_PropDescription']
>

export type ModSetRequest = Pick<
  components['schemas']['Server.RpgRequest_ModSet'],
  keyof components['schemas']['Server.RpgRequest_ModSet']
>

export type DescribeModSetRequest = Pick<
  components['schemas']['Server.RpgRequest_DescribeModSet'],
  keyof components['schemas']['Server.RpgRequest_DescribeModSet']
>

export type DescribeModSetResponse = Pick<
  components['schemas']['Server.RpgResponse_ModSetDescription'],
  keyof components['schemas']['Server.RpgResponse_ModSetDescription']
>

export type ActivityCreateRequest = Pick<
  components['schemas']['Server.RpgRequest_ActivityCreate'],
  keyof components['schemas']['Server.RpgRequest_ActivityCreate']
>

export type ActivityCreate = Pick<
  components['schemas']['Server.Ops.ActivityCreate'],
  keyof components['schemas']['Server.Ops.ActivityCreate']
>

export type ActionResponse = Pick<
  components['schemas']['Server.RpgResponse_Action2'],
  keyof components['schemas']['Server.RpgResponse_Action2']
>

export type StringResponse = Pick<
  components['schemas']['Server.RpgResponse_String'],
  keyof components['schemas']['Server.RpgResponse_String']
>

export type BooleanResponse = Pick<
  components['schemas']['Server.RpgResponse_Boolean'],
  keyof components['schemas']['Server.RpgResponse_Boolean']
>

export type SetTimeRequest = Pick<
  components['schemas']['Server.RpgRequest_PointInTime'],
  keyof components['schemas']['Server.RpgRequest_PointInTime']
>

export type SetTimeResponse = Pick<
  components['schemas']['Server.RpgResponse_PointInTime'],
  keyof components['schemas']['Server.RpgResponse_PointInTime']
>
