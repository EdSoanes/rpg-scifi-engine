// import { createSelector } from "@reduxjs/toolkit"
// import { ActionTemplate } from "../../lib/rpg-api/types"
// import { RootState } from "../store"
// import { ThunkStatus } from "../thunks"

// export const selectActions = (state: RootState): ActionTemplate[] => state.actionTemplates.actionTemplates
// export const selectActionName = (state: RootState, actionName: string) => actionName
// export const selectActionsStatus = (state: RootState): ThunkStatus => state.actionTemplates.status

// export const selectStateByName = createSelector(
//   [selectActions, selectActionName],
//   (actions, actionName) => actions.find((action) => action.name === actionName)
// )
