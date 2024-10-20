import { createSlice } from '@reduxjs/toolkit'
import { Activity } from '../../lib/rpg-api/types'
import { fetchActivity, ThunkStatus } from '../thunks'

export declare interface ActivitiesState {
  activity?: Activity
  status: ThunkStatus
}

const initialState: ActivitiesState = {
  status: 'idle',
}

export const activitySlice = createSlice({
  name: 'rpgActivity',
  initialState,   
  reducers: {
  },
  extraReducers: builder => {
    builder
    .addCase(fetchActivity.pending, (state) => {
      state.status = 'loading'
    })
    .addCase(fetchActivity.fulfilled, (state, action) => {
      state.activity = action.payload?.data
      state.status = action.payload ? 'loaded' : 'idle'
    })
  }
})


// Action creators are generated for each case reducer function

export default activitySlice.reducer