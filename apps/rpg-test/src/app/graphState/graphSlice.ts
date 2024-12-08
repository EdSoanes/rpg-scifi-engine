import { createSlice } from '@reduxjs/toolkit'
import { RpgGraphState } from '../../lib/rpg-api/types'
import {
  initiateAction,
  fetchGraphState,
  setGraphTime,
  ThunkStatus,
  toggleState,
  overridePropValue,
} from '../thunks'

export declare interface GraphState {
  graphState?: RpgGraphState
  status: ThunkStatus
}

const initialState: GraphState = {
  status: 'idle',
}

export const graphSlice = createSlice({
  name: 'rpgGraph',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchGraphState.pending, (state) => {
        state.status = 'loading'
      })
      .addCase(fetchGraphState.fulfilled, (state, action) => {
        state.graphState = action.payload
        state.status = action.payload ? 'loaded' : 'idle'
      })
      .addCase(initiateAction.pending, (state) => {
        state.status = 'loading'
      })
      .addCase(initiateAction.fulfilled, (state, action) => {
        state.graphState = action.payload?.graphState
        state.status = action.payload ? 'loaded' : 'idle'
      })
      .addCase(toggleState.pending, (state) => {
        state.status = 'loading'
      })
      .addCase(toggleState.fulfilled, (state, action) => {
        state.graphState = action.payload?.graphState
        state.status = action.payload ? 'loaded' : 'idle'
      })
      .addCase(setGraphTime.pending, (state) => {
        state.status = 'loading'
      })
      .addCase(setGraphTime.fulfilled, (state, action) => {
        state.graphState = action.payload?.graphState
        state.status = action.payload ? 'loaded' : 'idle'
      })
      .addCase(overridePropValue.pending, (state) => {
        state.status = 'loading'
      })
      .addCase(overridePropValue.fulfilled, (state, action) => {
        state.graphState = action.payload?.graphState
        state.status = action.payload ? 'loaded' : 'idle'
      })
  },
})

// Action creators are generated for each case reducer function

export default graphSlice.reducer
