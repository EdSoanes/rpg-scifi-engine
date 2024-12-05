import { createSelector } from '@reduxjs/toolkit'
import { State } from '../../lib/rpg-api/types'
import { RootState } from '../store'

export const selectStates = (state: RootState): State[] =>
  state.states.states.filter((item) => item.classification == 'State')
export const selectConditions = (state: RootState): State[] =>
  state.states.states.filter((item) => item.classification == 'Condition')
export const selectStateName = (state: RootState, stateName: string) =>
  stateName

export const selectStateByName = createSelector(
  [selectStates, selectStateName],
  (states, stateName) => states.find((state) => state.name === stateName)
)
