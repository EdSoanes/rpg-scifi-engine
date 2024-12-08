import { createSlice } from '@reduxjs/toolkit'
import { ActionTemplate } from '@lib/rpg-api/types'
import { fetchGraphState, ThunkStatus } from '@app/thunks'
import { PlayerCharacter } from '@lib/rpg-api/cyborg-types'

export declare interface ActionTemplatesState {
  actionTemplates: ActionTemplate[]
  status: ThunkStatus
}

const initialState: ActionTemplatesState = {
  actionTemplates: [],
  status: 'idle',
}

export const actionTemplatesSlice = createSlice({
  name: 'rpgActionTemplates',
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
        const dict = playerCharacter?.actionTemplates

        state.actionTemplates = dict
          ? Object.entries(dict).map((pair) => pair[1] as ActionTemplate)
          : []
        state.status = action.payload ? 'loaded' : 'idle'
      })
  },
})

// Action creators are generated for each case reducer function

export default actionTemplatesSlice.reducer
