import createClient from 'openapi-fetch'

import { paths } from './rpgtypes'
import { PlayerCharacter, RpgGraphState, State, Action } from './types'
import { atom } from 'jotai'
import { splitAtom } from 'jotai/utils'

export const playerCharacterAtom = atom<PlayerCharacter | null>((get) => {
  const graphState = get(graphStateAtom)
  return graphState?.entities.find(
    (x) => x.id === graphState.contextId
  ) as PlayerCharacter
})

const statesAtom = atom<State[]>((get) => {
  const dict = get(playerCharacterAtom)?.states ?? {}
  return Object.entries(dict).map((pair) => pair[1] as State)
})

const actionsAtom = atom<Action[]>((get) => {
  const dict = get(playerCharacterAtom)?.actions ?? {}
  const actions = Object.entries(dict).map((pair) => pair[1] as Action)

  return actions
})

export const stateAtomsAtom = splitAtom(statesAtom)
export const actionAtomsAtom = splitAtom(actionsAtom)
export const graphStateAtom = atom<RpgGraphState | null>(null)

export const graphFetchAtom = atom(null, async (get, set) => {
  const graphState = await getGraphState('Benny')
  set(graphStateAtom, graphState)
})

// const client = createClient<paths>({
//   baseUrl: '',
//   headers: {
//     Accept: 'application/json',
//     'Content-Type': 'application/json',
//   },
//   mode: 'cors',
// })

const client = createClient<paths>({
  mode: 'cors',
  headers: { Accept: 'application/json; charset=utf-8' },
})

export const getGraphState = async (
  id: string
): Promise<RpgGraphState | null> => {
  const response = await client.GET('/api/v1/rpg/{system}/{archetype}/{id}', {
    params: {
      path: {
        system: process.env.RPG_SYSTEM ?? 'Cyborgs',
        archetype: process.env.RPG_PC_ARCHETYPE ?? 'PlayerCharacter',
        id: id,
      },
    },
  })

  return !response.error ? response.data : null
}

export const postSetState = async (
  entityId: string,
  stateName: string,
  on: boolean,
  graphState: RpgGraphState
): Promise<RpgGraphState | null> => {
  const response = await fetch(`/api/v1/rpg/Cyborgs/state`, {
    method: 'POST',
    headers: {
      Accept: 'application/json; charset=utf-8',
      'Content-Type': 'application/json; charset=utf-8',
    },
    body: JSON.stringify({
      graphState: graphState!,
      operation: {
        entityId: entityId,
        state: stateName,
        on: on,
      },
    }),
  })

  return (await response.json()) as RpgGraphState
}
