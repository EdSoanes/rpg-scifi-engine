import { atom } from 'jotai'
import { Action } from '../../lib/rpg-api/types'
import { playerCharacterAtom } from './playerCharacter.atom'

export const playerCharacterActionsAtom = atom<Action[]>((get) => {
  const dict = get(playerCharacterAtom)?.actions ?? {}
  return Object.entries(dict).map((pair) => pair[1] as Action)
})
