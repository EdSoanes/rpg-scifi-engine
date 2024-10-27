import { createSelector } from "@reduxjs/toolkit"
import { ActionInstance, Activity } from "../../lib/rpg-api/types"
import { RootState } from "../store"
import { ThunkStatus } from "../thunks"

export const selectActivity = (state: RootState): Activity | undefined => state.activity.activity
export const selectActivityStatus = (state: RootState): ThunkStatus => state.activity.status

export const selectActionInstances = createSelector([selectActivity], (activity?: Activity): ActionInstance[] => {
  return activity?.actionInstances ?? []
})

export const selectActionInstance = createSelector([selectActivity], (activity?: Activity): ActionInstance | undefined => {
  return activity?.actionInstance
})
