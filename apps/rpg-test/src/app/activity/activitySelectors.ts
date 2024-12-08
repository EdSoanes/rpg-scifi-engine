import { createSelector } from "@reduxjs/toolkit"
import { Action, Activity } from "../../lib/rpg-api/types"
import { RootState } from "../store"
import { ThunkStatus } from "../thunks"

export const selectActivity = (state: RootState): Activity | undefined => state.activity.activity
export const selectActivityStatus = (state: RootState): ThunkStatus => state.activity.status

export const selectActions = createSelector([selectActivity], (activity?: Activity): Action[] => {
  return activity?.actions ?? []
})

export const selectCurrentAction = createSelector([selectActivity], (activity?: Activity): Action | undefined => {
  return activity?.actions[activity.currentActionNo]
})
