import { Describe, PropDesc, RpgGraphState, SetState } from './types'

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
