import {
  Act,
  ActionInstance,
  AddModSet,
  CreateActionInstance,
  Describe,
  ModSet,
  PropDesc,
  RpgGraphState,
  SetState,
} from './types'

export const getGraphState = async (
  id: string
): Promise<RpgGraphState | null> => {
  const response = await get(`Cyborgs/PlayerCharacter/${id}`)

  return (await response.json()) as RpgGraphState
}

export const getPropDesc = async (
  entityId: string,
  prop: string,
  graphState: RpgGraphState
): Promise<PropDesc | null> => {
  const describe: Describe = {
    graphState: graphState,
    operation: {
      entityId: entityId,
      prop: prop,
    },
  }

  const response = await post('Cyborgs/describe', describe)
  return (await response.json()) as PropDesc
}

export const getActionInstance = async (
  ownerId: string,
  initiatorId: string,
  actionName: string,
  actionNo: number,
  graphState: RpgGraphState
): Promise<ActionInstance | null> => {
  const op: CreateActionInstance = {
    graphState: graphState,
    operation: {
      ownerId,
      initiatorId,
      actionName,
      actionNo,
    },
  }

  const response = await post('Cyborgs/actioninstance/create', op)
  return (await response.json()) as ActionInstance
}

export const getActionCost = async (
  ownerId: string,
  initiatorId: string,
  actionName: string,
  actionNo: number,
  graphState: RpgGraphState
): Promise<ModSet | null> => {
  const op: Act = {
    graphState: graphState,
    operation: {
      ownerId,
      initiatorId,
      actionName,
      actionNo,
      argValues: {},
    },
  }

  const response = await post('Cyborgs/actioninstance/cost', op)
  return response.status === 200 ? ((await response.json()) as ModSet) : null
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
