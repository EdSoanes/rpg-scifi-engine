import {
  ActivityActRequest,
  ActivityCreateRequest,
  ActivityOutcomeRequest,
  ActivityResponse,
  BooleanResponse,
  StringResponse,
  DescribeRequest,
  DescribeResponse,
  ModSetRequest,
  SetStateRequest,
} from './server-types'

import { Activity, ModSet, RpgGraphState } from './types'

export const getGraphState = async (
  id: string
): Promise<StringResponse | null> => {
  const response = await get(`Cyborgs/PlayerCharacter/${id}`)

  return (await response.json()) as StringResponse
}

export const getPropDesc = async (
  entityId: string,
  prop: string,
  graphState: RpgGraphState
): Promise<DescribeResponse | null> => {
  const describe: DescribeRequest = {
    graphState: graphState,
    op: {
      entityId: entityId,
      prop: prop,
    },
  }

  const response = await post('Cyborgs/describe', describe)
  return (await response.json()) as DescribeResponse
}

export const getActionInstance = async (
  ownerId: string,
  initiatorId: string,
  action: string,
  graphState: RpgGraphState
): Promise<ActivityResponse | null> => {
  const op: ActivityCreateRequest = {
    graphState: graphState,
    op: {
      ownerId,
      initiatorId,
      action,
    },
  }

  const response = await post('Cyborgs/activity/create', op)
  return (await response.json()) as ActivityResponse
}

export const getActionAct = async (
  activityId: string,
  argValues: { [key: string]: string | null | undefined },
  graphState: RpgGraphState
): Promise<Activity | undefined> => {
  const op: ActivityActRequest = {
    graphState: graphState,
    op: {
      activityId: activityId,
      args: argValues,
    },
  }

  const response = await post('Cyborgs/activity/act', op)
  return response.status === 200
    ? ((await response.json()) as Activity)
    : undefined
}

export const getActionOutcome = async (
  activityId: string,
  argValues: { [key: string]: string | null | undefined },
  graphState: RpgGraphState
): Promise<Activity | undefined> => {
  const op: ActivityOutcomeRequest = {
    graphState: graphState,
    op: {
      activityId: activityId,
      args: argValues,
    },
  }

  const response = await post('Cyborgs/activity/outcome', op)
  return response.status === 200
    ? ((await response.json()) as Activity)
    : undefined
}

export const postModSet = async (
  modSet: ModSet,
  graphState: RpgGraphState
): Promise<BooleanResponse | null> => {
  const op: ModSetRequest = {
    graphState: graphState!,
    op: modSet,
  }

  const response = await post('Cyborgs/modSet', op)
  return (await response.json()) as BooleanResponse
}

export const postSetState = async (
  entityId: string,
  stateName: string,
  on: boolean,
  graphState: RpgGraphState
): Promise<StringResponse | undefined> => {
  const setState: SetStateRequest = {
    graphState: graphState!,
    op: {
      entityId: entityId,
      state: stateName,
      on: on,
    },
  }

  const response = await post('Cyborgs/state', setState)
  return (await response.json()) as StringResponse
}

const get = async (path: string) => {
  const response = await fetch(`https://localhost:44349/api/v1/rpg/${path}`, {
    method: 'GET',
    headers: {
      Accept: 'application/json; charset=utf-8',
      'Content-Type': 'application/json; charset=utf-8',
    },
  })

  return response
}

const post = async (path: string, body?: unknown) => {
  const response = await fetch(`https://localhost:44349/api/v1/rpg/${path}`, {
    method: 'POST',
    headers: {
      Accept: 'application/json; charset=utf-8',
      'Content-Type': 'application/json; charset=utf-8',
    },
    body: body ? JSON.stringify(body) : null,
  })

  return response
}
