import { createSelector } from "@reduxjs/toolkit"
import { Action } from "../../lib/rpg-api/types"
import { RootState } from "../store"
import { ThunkStatus } from "../thunks"

export const selectActions = (state: RootState): Action[] => state.actions.actions
export const selectActionName = (state: RootState, actionName: string) => actionName
export const selectActionsStatus = (state: RootState): ThunkStatus => state.actions.status

export const selectStateByName = createSelector(
  [selectActions, selectActionName],
  (actions, actionName) => actions.find((action) => action.name === actionName)
)
