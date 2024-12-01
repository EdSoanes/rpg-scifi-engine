import {
  BooleanResponse,
  StringResponse,
  DescribePropRequest,
  DescribePropResponse,
  ModSetRequest,
  SetStateRequest,
  DescribeModSetResponse,
  DescribeStateRequest,
} from './server-types'

import { ModSet, RpgGraphState } from './types'

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
): Promise<DescribePropResponse | null> => {
  const describe: DescribePropRequest = {
    graphState: graphState,
    op: {
      entityId: entityId,
      prop: prop,
    },
  }

  const response = await post('Cyborgs/describe', describe)
  return (await response.json()) as DescribePropResponse
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

export const getStateDescription = async (
  entityId: string,
  stateName: string,
  graphState: RpgGraphState
): Promise<DescribeModSetResponse | undefined> => {
  const setState: DescribeStateRequest = {
    graphState: graphState!,
    op: {
      entityId: entityId,
      state: stateName,
    },
  }

  const response = await post('Cyborgs/state/describe', setState)
  return (await response.json()) as DescribeModSetResponse
}

const get = async (path: string) => {
  const response = await fetch(`https://localhost:44349/api/rpg/${path}`, {
    method: 'GET',
    headers: {
      Accept: 'application/json; charset=utf-8',
      'Content-Type': 'application/json; charset=utf-8',
    },
  })

  return response
}

const post = async (path: string, body?: unknown) => {
  const response = await fetch(`https://localhost:44349/api/rpg/${path}`, {
    method: 'POST',
    headers: {
      Accept: 'application/json; charset=utf-8',
      'Content-Type': 'application/json; charset=utf-8',
    },
    body: body ? JSON.stringify(body) : null,
  })

  return response
}
