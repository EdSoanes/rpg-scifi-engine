import {
  ActivityCreateRequest,
  ActivityResponse,
  CreateGraphStateResponse,
  DescribeRequest,
  DescribeResponse,
} from './server-types'

import { ActionInstance, ActionModSet, ModSet, RpgGraphState } from './types'

export const getGraphState = async (
  id: string
): Promise<CreateGraphStateResponse | null> => {
  const response = await get(`Cyborgs/PlayerCharacter/${id}`)

  return (await response.json()) as CreateGraphStateResponse
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

export const getActionCost = async (
  actionInstance: ActionInstance,
  argValues: { [key: string]: string | null | undefined },
  graphState: RpgGraphState
): Promise<ModSet | null> => {
  const op: Act = {
    graphState: graphState,
    operation: {
      ownerId: actionInstance.ownerId,
      initiatorId: actionInstance.initiatorId,
      actionName: actionInstance.actionName,
      actionNo: actionInstance.actionNo,
      argValues: argValues,
    },
  }

  const response = await post('Cyborgs/actioninstance/cost', op)
  return response.status === 200 ? ((await response.json()) as ModSet) : null
}

export const getActionAct = async (
  actionInstance: ActionInstance,
  argValues: { [key: string]: string | null | undefined },
  graphState: RpgGraphState
): Promise<ActionModSet | null> => {
  const op: Act = {
    graphState: graphState,
    operation: {
      ownerId: actionInstance.ownerId,
      initiatorId: actionInstance.initiatorId,
      actionName: actionInstance.actionName,
      actionNo: actionInstance.actionNo,
      argValues: argValues,
    },
  }

  const response = await post('Cyborgs/actioninstance/act', op)
  return response.status === 200
    ? ((await response.json()) as ActionModSet)
    : null
}

export const getActionOutcome = async (
  actionInstance: ActionInstance,
  argValues: { [key: string]: string | null | undefined },
  graphState: RpgGraphState
): Promise<ModSet[] | null> => {
  const op: Act = {
    graphState: graphState,
    operation: {
      ownerId: actionInstance.ownerId,
      initiatorId: actionInstance.initiatorId,
      actionName: actionInstance.actionName,
      actionNo: actionInstance.actionNo,
      argValues: argValues,
    },
  }

  const response = await post('Cyborgs/actioninstance/outcome', op)
  return response.status === 200 ? ((await response.json()) as ModSet[]) : null
}

export const postModSet = async (
  modSet: ModSet,
  graphState: RpgGraphState
): Promise<RpgGraphState | null> => {
  const op: AddModSet = {
    graphState: graphState!,
    operation: modSet,
  }

  const response = await post('Cyborgs/modSet', op)
  return (await response.json()) as RpgGraphState
}

export const postSetState = async (
  entityId: string,
  stateName: string,
  on: boolean,
  graphState: RpgGraphState
): Promise<RpgGraphState | null> => {
  const setState: SetState = {
    graphState: graphState!,
    operation: {
      entityId: entityId,
      state: stateName,
      on: on,
    },
  }

  const response = await post('Cyborgs/state', setState)
  return (await response.json()) as RpgGraphState
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
