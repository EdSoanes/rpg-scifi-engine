import { createSlice } from '@reduxjs/toolkit'
import { RpgContainer } from '../../lib/rpg-api/types'
import { fetchGraphState, ThunkStatus } from '../thunks'
import { PlayerCharacter } from '../../lib/rpg-api/cyborg-types'

export declare interface GearState {
  hands?: RpgContainer
  wearing?: RpgContainer
  status: ThunkStatus
}

const initialState: GearState = {
  status: 'idle',
}

export const gearSlice = createSlice({
  name: 'rpgGear',
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
        state.hands = playerCharacter?.hands
        state.wearing = playerCharacter?.wearing
        state.status = action.payload ? 'loaded' : 'idle'
      })
  },
})

// Action creators are generated for each case reducer function

export default gearSlice.reducer
