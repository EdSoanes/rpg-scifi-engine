import { createSlice } from '@reduxjs/toolkit'
import { State } from '../../lib/rpg-api/types'
import { fetchGraphState, ThunkStatus, toggleState } from '../thunks'
import { PlayerCharacter } from '../../lib/rpg-api/cyborg-types'

export declare interface StatesState {
  states: State[]
  status: ThunkStatus
}

const initialState: StatesState = {
  states: [],
  status: 'idle',
}

export const statesSlice = createSlice({
  name: 'rpgStates',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchGraphState.pending, (state) => {
        state.status = 'loading'
      })
      .addCase(fetchGraphState.fulfilled, (state, action) => {
        const playerCharacter = action.payload?.entities?.find(
          (item) => item.archetype === 'PlayerCharacter'
        ) as PlayerCharacter
        const dict = playerCharacter?.states
        state.states = dict
          ? Object.entries(dict).map((pair) => pair[1] as State)
          : []
        state.status = action.payload ? 'loaded' : 'idle'
      })
      .addCase(toggleState.pending, (state) => {
        state.status = 'loading'
      })
      .addCase(toggleState.fulfilled, (state, action) => {
        const playerCharacter = action.payload?.graphState.entities?.find(
          (item) => item.archetype === 'PlayerCharacter'
        ) as PlayerCharacter
        const dict = playerCharacter?.states
        state.states = dict
          ? Object.entries(dict).map((pair) => pair[1] as State)
          : []
        state.status = action.payload ? 'loaded' : 'idle'
      })
  },
})

// Action creators are generated for each case reducer function

export default statesSlice.reducer
