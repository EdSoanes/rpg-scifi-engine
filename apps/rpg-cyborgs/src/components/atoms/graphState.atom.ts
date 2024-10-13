import { atom } from 'jotai'
import { RpgGraphState } from '../../lib/rpg-api/types'

export const graphStateAtom = atom<RpgGraphState | undefined>(undefined)
