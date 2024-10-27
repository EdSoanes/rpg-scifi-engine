import { createSlice } from '@reduxjs/toolkit'
import { Action } from '../../lib/rpg-api/types'
import { fetchGraphState, ThunkStatus } from '../thunks'
import { PlayerCharacter } from '../../lib/rpg-api/cyborg-types'

export declare interface ActionsState {
  actions: Action[]
  status: ThunkStatus
}

const initialState: ActionsState = {
  actions: [],
  status: 'idle',
}

export const actionsSlice = createSlice({
  name: 'rpgActions',
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
        const dict = playerCharacter?.actions

        state.actions = dict
          ? Object.entries(dict).map((pair) => pair[1] as Action)
          : []
        state.status = action.payload ? 'loaded' : 'idle'
      })
  },
})

// Action creators are generated for each case reducer function

export default actionsSlice.reducer
